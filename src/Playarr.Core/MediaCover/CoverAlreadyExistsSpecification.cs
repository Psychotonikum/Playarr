using Playarr.Common.Disk;
using Playarr.Common.Http;

namespace Playarr.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(string url, string path);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpClient _httpClient;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider, IHttpClient httpClient)
        {
            _diskProvider = diskProvider;
            _httpClient = httpClient;
        }

        public bool AlreadyExists(string url, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return false;
            }

            var headers = _httpClient.Head(new HttpRequest(url)).Headers;
            var fileSize = _diskProvider.GetFileSize(path);
            return fileSize == headers.ContentLength;
        }
    }
}
