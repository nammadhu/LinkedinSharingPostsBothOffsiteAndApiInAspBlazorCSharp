using System.Text.Json.Serialization;

namespace LinkedinSharingPostsAspBlazor;

/* Below Models are Requried only For Server Side to Process in Backend
 * Configurationwise this requires only ClientID,ClientSecret & RedirectUri
*/

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
