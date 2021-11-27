namespace LWS_Node.Model.Response;

public class ErrorModel
{
    public int StatusCode { get; set; }
    public string RequestTraceId { get; set; }
    public string Message { get; set; }
}