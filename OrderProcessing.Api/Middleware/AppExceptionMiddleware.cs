using System.Net;

namespace OrderProcessing.Api.Middleware;

public class AppExceptionMiddleware: Exception
{
    public HttpStatusCode StatusCode { get; }

    public AppExceptionMiddleware(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : base(message)
    {
        StatusCode = statusCode;
    }
}