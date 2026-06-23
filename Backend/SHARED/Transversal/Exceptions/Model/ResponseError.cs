namespace Transversal.Exceptions.Model
{
    internal class ResponseError<T>(string code, string message, T data)
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public bool IsSuccess { get; } = false;
        public T Data { get; } = data;
    }
}
