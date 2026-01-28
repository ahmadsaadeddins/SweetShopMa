namespace SweetShopMa.Models;

public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }

    public static ServiceResult Ok(string message = null) => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message, string code = null) => new() { Success = false, Message = message, ErrorCode = code };
}

public class ServiceResult<T> : ServiceResult
{
    public T Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = null) => new() { Success = true, Data = data, Message = message };
    public static new ServiceResult<T> Fail(string message, string code = null) => new() { Success = false, Message = message, ErrorCode = code };
}
