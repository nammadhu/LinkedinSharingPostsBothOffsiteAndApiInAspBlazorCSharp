using Microsoft.AspNetCore.Mvc;

namespace LinkedinSharingPostsAspBlazor.Controllers;

//[ApiController]
[Route("[controller]")]
public class LinkedinController(LinkedInService linkedInService) : ControllerBase
{
    [HttpGet(nameof(CallBack))]
    public async Task CallBack()
    {
        var context = this.HttpContext;
        var code = context.Request.Query["code"];
        if (string.IsNullOrEmpty(code)) throw new Exception("Auth Code Not Received");

        var queryParameters = LinkedInPost.ParseState(context.Request.Query[nameof(LinkedInPost.state)]!);
        if (queryParameters != null)
        {
            queryParameters.TryGetValue(nameof(LinkedInPost.Text), out string? text);
            queryParameters.TryGetValue(nameof(LinkedInPost.ImageUrlRemote), out string? imageUrl);
            queryParameters.TryGetValue(nameof(LinkedInPost.TextUrl), out string? url);
            queryParameters.TryGetValue(nameof(LinkedInPost.ReturnUrl), out string? returnUrl);

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(imageUrl) && string.IsNullOrEmpty(url))
                throw new Exception("No Content,so nothing to post");

            var accessToken = await linkedInService.GetAccessTokenAsync(code!);
            if (string.IsNullOrEmpty(accessToken)) throw new Exception("AccessToken Fetching Failed");

            var personId = await linkedInService.GetPersonIdAsync(accessToken);
            if (string.IsNullOrEmpty(personId)) throw new Exception("personId Fetching Failed");

            if (string.IsNullOrEmpty(context.Request.Query[nameof(LinkedInPost.state)])) throw new Exception("no State parameter,so nothing to post");

            //cache can be used but in case of detached mode of clinet server it makes problem
            string? uploadedAssetUrl = null;
            if (!string.IsNullOrEmpty(imageUrl))//imageBytes != null || 
            {
                (string uploadUrl, uploadedAssetUrl) = await linkedInService.RegisterUploadAsync(accessToken, personId);

                await linkedInService.UploadImageAsync(uploadUrl, imageBytes: null, remoteImageUrl: imageUrl);
            }

            await linkedInService.SharePostAsync(accessToken, personId, text, uploadedAssetUrl, url);
            context.Response.Redirect(returnUrl ?? "/");
        }
        context.Response.Redirect("/");
    }
}
