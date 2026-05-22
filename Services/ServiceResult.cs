namespace BanquetHall.Services;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ServiceResult<T> Fail(string field, string message) =>
        new() { Success = false, Errors = { [field] = message } };
    public static ServiceResult<T> Fail(Dictionary<string, string> errors) =>
        new() { Success = false, Errors = errors };
}
