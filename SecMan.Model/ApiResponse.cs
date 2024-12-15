using Newtonsoft.Json;
using System.Net;

namespace SecMan.Model
{
    public class ApiResponse
    {
        public ApiResponse()
        {

        }

        public ApiResponse(string message, HttpStatusCode statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }
    }



    public class ServiceResponse<T> : ApiResponse
    {
        public ServiceResponse()
        {

        }

        public ServiceResponse(string message, HttpStatusCode statusCode) : base(message, statusCode)
        {

        }

        public ServiceResponse(string message, HttpStatusCode statusCode, T? data) : base(message, statusCode)
        {
            Data = data;
        }

        [JsonProperty("date")]
        public T? Data { get; set; }
    }



    public class Unauthorized : CommonResponse
    {
        public Unauthorized()
        {

        }

        public Unauthorized(string title, HttpStatusCode status, string detail) : base(title, status, detail, ResponseUrlType.Unauthorized)
        {
            Title = title;
            Status = status;
            Detail = detail;
        }
    }


    public class Forbidden : CommonResponse
    {
        public Forbidden()
        {

        }

        public Forbidden(string title, string detail) : base(title, HttpStatusCode.Forbidden, detail, ResponseUrlType.Forbidden)
        {
            Title = title;
            Status = HttpStatusCode.Forbidden;
            Detail = detail;
        }
    }


    public class ServerError : CommonResponse
    {
        public ServerError()
        {

        }

        public ServerError(string title, HttpStatusCode status, string detail) : base(title, status, detail, ResponseUrlType.ServerError)
        {
            Title = title;
            Status = status;
            Detail = detail;
        }
    }


    public class Conflict : CommonResponse
    {
        public Conflict()
        {

        }

        public Conflict(string title, HttpStatusCode status, string detail) : base(title, status, detail, ResponseUrlType.Conflict)
        {
            Title = title;
            Status = status;
            Detail = detail;
        }
    }

    public class CommonResponse
    {
        public CommonResponse()
        {

        }

        public CommonResponse(string title, HttpStatusCode status, string detail, string type)
        {
            Title = title;
            Status = status;
            Detail = detail;
            Type = type;
        }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; } = string.Empty;
    }


    public class SuccessResponse<T> : CommonResponse
    {
        public SuccessResponse(string title, HttpStatusCode status = HttpStatusCode.OK, string detail = "")
            : base(title, status, detail, "success")
        {
        }

        public T? Data { get; set; }
    }


    public class BadRequest : CommonResponse
    {
        public BadRequest(string title, string detail, List<InvalidParams?>? invalidParams = null)
            : base(title, HttpStatusCode.BadRequest, detail, ResponseUrlType.ValidationError)
        {
            InvalidParams = invalidParams;
        }

        [JsonProperty("invalidParams")]
        public List<InvalidParams?>? InvalidParams { get; set; }
    }

    public class NotFound : CommonResponse
    {
        public NotFound()
        {

        }

        public NotFound(string title, string detail) : base(title, HttpStatusCode.NotFound, detail, ResponseUrlType.NotFound)
        {
            Title = title;
            Detail = detail;
        }
    }



    public class UpdatedResponse
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }
    }

    public class UpdatedIdsResponse
    {
        [JsonProperty("id")]
        public List<ulong>? Id { get; set; }
    }



    public class InvalidParams
    {
        public string In { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
