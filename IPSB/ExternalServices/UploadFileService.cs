using Firebase.Storage;
using IPSB.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;
namespace IPSB.ExternalServices
{
    public interface IUploadFileService
    {
        Task<string> UploadFile(string idToken, IFormFile file, string bucket, string directory);
    }
    public class UploadFileService : IUploadFileService
    {
        private readonly IConfiguration _configuration;

        private readonly IJwtTokenProvider _jwtTokenProvider;

        public UploadFileService(IConfiguration configuration, IJwtTokenProvider jwtTokenProvider)
        {
            _configuration = configuration;
            _jwtTokenProvider = jwtTokenProvider;
        }

        public async Task<string> UploadFile(string idToken, IFormFile file, string bucket, string directory)
        {
            var task = new FirebaseStorage(
                _configuration[AppSetting.FirebaseBucket],
                new FirebaseStorageOptions()
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(idToken)
                });
            string fileExtension = Path.GetExtension(file.FileName);
            Guid guid = Guid.NewGuid();
            string fileName = guid.ToString() + "." + fileExtension;
            return await task.Child(bucket)
                .Child(directory)
                .Child(fileName)
                .PutAsync(file.OpenReadStream());
        }
    }
}
