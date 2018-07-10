#region USING_DIRECTIVES
using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Enums;
using Imgur.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services
{
    public class ImgurService : ITheGodfatherService
    {
        private readonly ImgurClient imgur;
        private readonly GalleryEndpoint gEndpoint;
        //private readonly ImageEndpoint iEndpoint;


        public ImgurService(string key)
        {
            this.imgur = new ImgurClient(key);
            this.gEndpoint = new GalleryEndpoint(this.imgur);
            //this.iEndpoint = new ImageEndpoint(this.imgur);
        }


        public async Task<IEnumerable<IGalleryItem>> GetItemsFromSubAsync(string sub, int amount,
            SubredditGallerySortOrder order, TimeWindow time)
        {
            IEnumerable<IGalleryItem> images = await this.gEndpoint.GetSubredditGalleryAsync(sub, order, time);
            return images.Take(amount);
        }
    }
}
