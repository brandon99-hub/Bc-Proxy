using System.Net;

namespace BcProxy.Services;

public class BusinessCentralException : HttpRequestException
{
    public BusinessCentralException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException, statusCode)
    {
    }
}

