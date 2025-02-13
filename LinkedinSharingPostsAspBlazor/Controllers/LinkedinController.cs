using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkedinSharingPostsAspBlazor.Controllers
{
    //[ApiController]
    [Route("[controller]")]
    public class LinkedinController(LinkedInService linkedInService, IMemoryCache cache) : ControllerBase
    {
        [HttpGet(nameof(CallBack))]
        public async Task CallBack()
        {
            var context = this.HttpContext;
            var code = context.Request.Query["code"];

            var accessToken = await linkedInService.GetAccessTokenAsync(code);
            var personId = await linkedInService.GetPersonIdAsync(accessToken);

            // Retrieve the text and imageUrl from the memory cache
            //var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            var text = cache.Get<string>(LinkedInPost.TextConst);
            var imageUrl = cache.Get<string>(LinkedInPost.ImageUrlRemoteConst);
            var url = cache.Get<string>(LinkedInPost.UrlConst);

            string? assetUrl = null;
            if (imageUrl != null)//imageBytes != null || 
            {
                (string uploadUrl, assetUrl) = await linkedInService.RegisterUploadAsync(accessToken, personId);

                await linkedInService.UploadImageAsync(uploadUrl, imageBytes: null, remoteImageUrl: imageUrl);
            }

            await linkedInService.SharePostAsync(accessToken, personId, text, assetUrl, url);
            context.Response.Redirect("/");
        }
    }
}
