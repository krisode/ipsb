using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class ResponseModel
    {
        public ResponseModel() { }

        public bool Succeeded
        {
            get
            {
                if (Code == StatusCodes.Status200OK 
                    || Code == StatusCodes.Status204NoContent 
                    ||Code == StatusCodes.Status202Accepted 
                    ||Code == StatusCodes.Status203NonAuthoritative
                    ||Code == StatusCodes.Status204NoContent 
                    ||Code == StatusCodes.Status205ResetContent
                    ||Code == StatusCodes.Status206PartialContent)
                {
                    return true;
                } else
                {
                    return false;
                }
            } 
        }

        public int Code { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }

        public DateTime Timestamp
        {
            get 
            {
                var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTimeOffset localServerTime = DateTimeOffset.Now;
                DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
                return localTime.DateTime;
            }

        }

    }
}
