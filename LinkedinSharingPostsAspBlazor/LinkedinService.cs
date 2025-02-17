using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace LinkedinSharingPostsAspBlazor;
public class LinkedInService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly IConfiguration _configuration = configuration;

    public async Task<string> GetAccessTokenAsync(string authorizationCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "redirect_uri", _configuration["LinkedIn:RedirectUri"] },
                { "client_id", _configuration["LinkedIn:ClientId"] },
                { "client_secret", _configuration["LinkedIn:ClientSecret"] }
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LinkedInAccessTokenResponse>();
        return payload.AccessToken;
    }

    public async Task<string> GetPersonIdAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/me")
        {
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            }
        };

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error fetching person ID: {response.StatusCode}, {errorContent}");
        }

        var payload = await response.Content.ReadFromJsonAsync<LinkedInProfileResponse>();
        return payload.Id;
    }

    public async Task<(string uploadUrl, string asset)> RegisterUploadAsync(string accessToken, string personId)
    {
        // var client = new HttpClient();
        var uploadUrl = "https://api.linkedin.com/v2/assets?action=registerUpload";

        var jsonPayload = $@"
    {{
        ""registerUploadRequest"": {{
            ""owner"": ""urn:li:person:{personId}"",
            ""recipes"": [""urn:li:digitalmediaRecipe:feedshare-image""],
            ""serviceRelationships"": [
                {{
                    ""identifier"": ""urn:li:userGeneratedContent"",
                    ""relationshipType"": ""OWNER""
                }}
            ],
            ""supportedUploadMechanism"": [""SYNCHRONOUS_UPLOAD""]
        }}
    }}";

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Headers =
        {
            { "Authorization", $"Bearer {accessToken}" }
        },
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadFromJsonAsync<LinkedInUploadResponse>();

        if (!response.IsSuccessStatusCode || responseContent == null)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error registering image upload: {response.StatusCode}, {errorContent}");
        }

        return (responseContent.value.uploadMechanism["com.linkedin.digitalmedia.uploading.MediaUploadHttpRequest"].uploadUrl
            , responseContent.value.asset);
        //.uploadMechanism["com.linkedin.digitalmedia.uploading.MediaUploadHttpRequest"].uploadUrl;
    }

    public async Task<string> UploadImageAsync(string uploadUrl, byte[]? imageBytes = null, string? remoteImageUrl = null)
    {
        if (imageBytes == null && remoteImageUrl == null)
            throw new ArgumentNullException("imageBytes or remoteImageUrl must be provided");
        imageBytes ??= await _httpClient.GetByteArrayAsync("https://www.w3schools.com/howto/img_nature.jpg");
        var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
        {
            Content = new ByteArrayContent(imageBytes)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return uploadUrl;
    }

    public async Task SharePostAsync(string accessToken, string personId, string text, string assetUrl, string url)
    {
        string? jsonPayload = $@"
    {{
        ""author"": ""urn:li:person:{personId}"",
        ""lifecycleState"": ""PUBLISHED"",
        ""specificContent"": {{
            ""com.linkedin.ugc.ShareContent"": {{
                ""shareCommentary"": {{
                    ""text"": ""{text} {url ?? ""}""
                }},
               ""shareMediaCategory"": ""NONE""
            }}
        }},
        ""visibility"": {{
            ""com.linkedin.ugc.MemberNetworkVisibility"": ""PUBLIC""
        }}
    }}";
        if (assetUrl != null)
        {
            jsonPayload = $@"
    {{
        ""author"": ""urn:li:person:{personId}"",
        ""lifecycleState"": ""PUBLISHED"",
        ""specificContent"": {{
            ""com.linkedin.ugc.ShareContent"": {{
                ""shareCommentary"": {{
                    ""text"": ""{text} {url ?? ""}""
                }},
               ""shareMediaCategory"": ""IMAGE"",
                ""media"": [
                    {{
                        ""status"": ""READY"",
                        ""description"": {{
                            ""text"": ""Achievement Image""
                        }},
                        ""media"": ""{assetUrl}"",
                        ""title"": {{
                            ""text"": ""My Achievement""
                        }}
                    }}
                ]
            }}
        }},
        ""visibility"": {{
            ""com.linkedin.ugc.MemberNetworkVisibility"": ""PUBLIC""
        }}
    }}";
        }
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.linkedin.com/v2/ugcPosts")
        {
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" }
            },
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error sharing post: {response.StatusCode}, {errorContent}");
        }
    }
}

public class LinkedInPost
{
    public LinkedInPost() { }
    public LinkedInPost(string clientId, string redirectUri)
    {
        this.ClientId = clientId;
        this.RedirectUri = redirectUri;
    }
    public string ClientId { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string Text { get; set; } = "Default Linkedin Post Testing Text Here only";
    public string ImageUrlRemote { get; set; } = "https://www.w3schools.com/howto/img_nature.jpg";
    //public string? ImageUrlLocal { get; set; }//for uploading from local server
    public string TextUrl { get; set; } = "SmartTown.in";
    public string ReturnUrl { get; set; } = "/";//Encoded("/");

    public string GetAuthorizationUrl()// "%2F" means "/"
    {
        string queryParameters = string.Empty;
        if (!string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(ImageUrlRemote) || !string.IsNullOrEmpty(TextUrl))
        {
            //"Text=DefaultTest%20Of%20Linkedin%20Post%20Testing%20Text%20Here%20only&ImageUrlRemote=https%3A%2F%2Fwww.w3schools.com%2Fhowto%2Fimg_nature.jpg&TextUrl=SmartTown.in&ReturnUrl=%2FCounter"

            if (!string.IsNullOrEmpty(Text))
                queryParameters += $"{nameof(Text)}={Uri.EscapeDataString(Text)}";
            if (!string.IsNullOrEmpty(ImageUrlRemote))
                queryParameters += $"&{nameof(ImageUrlRemote)}={Uri.EscapeDataString(ImageUrlRemote)}";
            if (!string.IsNullOrEmpty(TextUrl))
                queryParameters += $"&{nameof(TextUrl)}={Uri.EscapeDataString(TextUrl)}";
            if (!string.IsNullOrEmpty(ReturnUrl))
                queryParameters += $"&{nameof(ReturnUrl)}={Uri.EscapeDataString(ReturnUrl)}";
        }

        //"https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=78y4e4touu7uv8&redirect_uri=https://localhost:7244/linkedin/callback&scope=openid%20profile%20email%20r_basicprofile%20w_member_social&state=Text%3DDefaultTest%2520Of%2520Linkedin%2520Post%2520Testing%2520Text%2520Here%2520only%26ImageUrlRemote%3Dhttps%253A%252F%252Fwww.w3schools.com%252Fhowto%252Fimg_nature.jpg%26TextUrl%3DSmartTown.in%26ReturnUrl%3D%252FCounter"

        var url = $"{AuthorizeCodeUrl}{ClientId}&redirect_uri={RedirectUri}&scope={Scopes}&{nameof(state)}={Uri.EscapeDataString(queryParameters)}";
        return url;
    }

    public static Dictionary<string, string>? ParseState(string state)
    {
        if (!string.IsNullOrEmpty(state))
            return Uri.UnescapeDataString(state).Split('&')
                               .Select(part => part.Split('='))
                               .ToDictionary(split => split[0], split => split[1]);
        else return null;
    }

    //var authorizationUrl = $"https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id={Configuration["LinkedIn:ClientId"]}&redirect_uri={Configuration["LinkedIn:RedirectUri"]}&scope=openid%20profile%20email%20r_basicprofile%20w_member_social";

    public const string AuthorizeCodeUrl = "https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=";
    //Uri.EscapeDataString("openid profile email r_basicprofile w_member_social");
    public const string Scopes = "openid%20profile%20email%20r_basicprofile%20w_member_social";
    public const string state = "state";

    public static string GetAuthorizationUrl(string clientId, string redirectUri, string returnUrl = "%2F")// "%2F" means "/"
    => $"{AuthorizeCodeUrl}{clientId}&redirect_uri={Uri.EscapeDataString($"?returnUrl={returnUrl}")}&scope={Scopes}";
}


public class LinkedInAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class LinkedInProfileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public class LinkedInUploadResponse
{
    public LinkedInUploadValue value { get; set; }
}

public class LinkedInUploadValue
{
    public Dictionary<string, LinkedInUploadMechanism> uploadMechanism { get; set; }
    public string asset { get; set; }
}

public class LinkedInUploadMechanism
{
    public string uploadUrl { get; set; }
}

