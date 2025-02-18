using System.Text.Json.Serialization;

namespace LinkedinSharingPostsAspBlazor;

//For Both client and Server Side shared
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
    public string ReturnUrl { get; set; } = "https://localhost:7244/Counter";//Encoded("/");

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