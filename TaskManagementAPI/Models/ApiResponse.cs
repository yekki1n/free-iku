namespace TaskManagementAPI.Models;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(int code, string message, T? data)
    {
        return new ApiResponse<T> { Code = code, Message = message, Data = data };
    }

    public static ApiResponse<T> Fail(int code, string message)
    {
        return new ApiResponse<T> { Code = code, Message = message, Data = default };
    }
}
