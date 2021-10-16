using Microsoft.AspNetCore.Http;

namespace IPSB.ViewModels
{
    public class UploadFileCM
    {
        public IFormFile File { get; set; }
    }

    public class UploadFileDM
    {
        public string ImageUrl { get; set; }
    }
}
