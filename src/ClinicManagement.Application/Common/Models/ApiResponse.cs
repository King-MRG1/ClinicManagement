namespace ClinicManagement.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Succeeded(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    public static ApiResponse<T> Failed(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors != null ? errors.ToList() : new List<string>()
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Succeeded(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = new List<string>()
        };
    }

    public static new ApiResponse Failed(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors != null ? errors.ToList() : new List<string>()
        };
    }
}
