namespace Contracts.Models;

public class ResponseModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
}

public class ResponseModel<T> : ResponseModel
{
    public T Result { get; set; }
}