namespace LinkedinSharingPostsAspBlazor;

/* Below Models are Requried only For Server Side to Process in Backend
 * Configurationwise this requires only ClientID,ClientSecret & RedirectUri
*/

public class LinkedInAccessTokenResponse
{
    public string access_token { get; set; }

    public int expires_in { get; set; }
}

public class LinkedInProfileResponse
{
    public string id { get; set; }

    public string vanityName { get; set; }
    
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
