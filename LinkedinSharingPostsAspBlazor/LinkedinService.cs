using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

public class LinkedInService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public LinkedInService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

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
    public const string TextConst = "LinkedInPostText";
    public const string UrlConst = "LinkedInPostUrl";
    public const string ImageUrlRemoteConst = "LinkedInPostImageUrlRemote";
    public const string ImageUrlLocalConst = "LinkedInPostImageUrlLocal";

    public string Text { get; set; } = "Default Testing Text Here only";
    public string ImageUrlRemote { get; set; } = "https://www.w3schools.com/howto/img_nature.jpg";
    public string? ImageUrlLocal { get; set; }//for uploading from local server
    public string TextUrl { get; set; } = "SmartTown.in";

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

