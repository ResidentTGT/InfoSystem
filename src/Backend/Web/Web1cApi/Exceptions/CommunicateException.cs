using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Web1cApi.Exceptions
{
    public class CommunicateException : Exception
    {
        public CommunicateError Error { get; set; }
        public new string Message { get; set; }

        public CommunicateException()
        {
        }

        public CommunicateException(CommunicateError error)
        {
            Error = error;
        }

        public CommunicateException(CommunicateError error, string message)
        {
            Error = error;
            Message = message;
        }

        public CommunicateException(HttpStatusCode statusCode, string body)
        {
            switch (statusCode)
            {

                case HttpStatusCode.Unauthorized:
                    Error = CommunicateError.Unathorized;
                    break;
                case HttpStatusCode.NotFound:
                    Error = CommunicateError.NotFound;
                    break;
                case HttpStatusCode.BadRequest:
                    Error = CommunicateError.InvalidRequest;
                    break;
                default:
                    Error = CommunicateError.Unknown;
                    break;
            }
            Message = body;
        }
    }

    public enum CommunicateError
    {
        NoResponse,
        Unathorized,
        NotFound,
        InvalidRequest,
        Unknown
    }
}