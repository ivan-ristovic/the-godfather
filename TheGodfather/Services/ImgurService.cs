#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API;
#endregion

namespace TheGodfather.Services
{
    public class ImgurService
    {
        private ImgurClient _imgur { get; set; }
        private GalleryEndpoint _gendpoint { get; set; }
        private ImageEndpoint _iendpoint { get; set; }


        public ImgurService(string key)
        {
            _imgur = new ImgurClient(key);
            _gendpoint = new GalleryEndpoint(_imgur);
            _iendpoint = new ImageEndpoint(_imgur);
        }


        public async Task<IEnumerable<IGalleryItem>> GetItemsFromSubAsync(string sub,
                                                                          int num,
                                                                          SubredditGallerySortOrder order,
                                                                          TimeWindow time)
        {

            var images = await _gendpoint.GetSubredditGalleryAsync(sub, order, time)
                .ConfigureAwait(false);
            return images.Take(num);
        }

        public async Task<string> UploadImageAsync(FileStream fs, string name)
        {
            var img = await _iendpoint.UploadImageStreamAsync(fs, name: name)
                .ConfigureAwait(false);
            return img.Link;
        }
    }
}
