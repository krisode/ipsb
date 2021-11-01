using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using IPSB.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;
namespace IPSB.ExternalServices
{
    public interface IUploadFileService
    {
        Task<string> UploadFile(string idToken, IFormFile file, string bucket, string directory);

        string UploadTemp(string idToken, IFormFile file, string bucket, string directory);
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


            string fileName = GetFileName(file);
            return await task.Child(bucket)
                .Child(directory)
                .Child(fileName)
                .PutAsync(file.OpenReadStream());
        }

        public string UploadTemp(string idToken, IFormFile file, string directory, string subDirectory)
        {
            string bucketName = _configuration[AppSetting.FirebaseBucket];
            var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "firebase_admin_sdk.json");
            var credential = GoogleCredential.FromFile(pathToKey);
            var storageClient = StorageClient.Create(credential);

            // File name & file path acquire
            string fileName = GetFileName(file);
            string filePath = directory + "/" + subDirectory + "/" + fileName;
            String filePathReturn = directory + "%2F" + subDirectory + fileName;

            String token = Guid.NewGuid().ToString();

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            var obj = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = bucketName,
                Name = filePath,
                ContentType = "image/png",
                Metadata = new Dictionary<string, string>(){
                    {"customTime", localTime.DateTime.ToString() }
                },
            };
            var storageObject = storageClient.UploadObject(obj, file.OpenReadStream());
            var url = "https://firebasestorage.googleapis.com/v0/b/" + bucketName + "/o/" + filePathReturn + "?alt=media&token=" + token;
            return url;
        }

        public bool ConfirmUpload(string imageUrl)
        {
            string bucketName = _configuration[AppSetting.FirebaseBucket];
            var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "firebase_admin_sdk.json");
            var credential = GoogleCredential.FromFile(pathToKey);
            var client = StorageClient.Create(credential);
            string objectName = GetObjectName(bucketName, imageUrl);
            var storageObject = client.GetObject(bucketName, objectName);
            Stream fileStream = new MemoryStream();
            client.DownloadObject(bucketName, objectName, fileStream);

            return false;
        }

        string GetObjectName(string bucketName, string url)
        {
            string result;
            string urlPart = "https://firebasestorage.googleapis.com/v0/b/" + bucketName + "/o/";
            var queryIndex = url.IndexOf("?");
            result = url.Remove(queryIndex);
            result = result.Replace(urlPart, "").Replace("%2F", "/");
            return result;
        }

        string GetFileName(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            Guid guid = Guid.NewGuid();
            return guid.ToString() + fileExtension;
        }
    }
}
