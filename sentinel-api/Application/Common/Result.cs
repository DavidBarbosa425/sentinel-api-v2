namespace sentinel_api.Application.Common
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static Result Ok(string message = "Operação realizada com sucesso")
            => new Result { Success = true, Message = message };

        public static Result Failure(string message = "Erro na operação")
            => new Result { Success = false, Message = message };
    }

    public class Result<T> : Result
    {
        public T Data { get; set; } = default!;
        public static Result<T> Ok(T data, string message = "Operação realizada com sucesso")
            => new Result<T> { Success = true, Message = message, Data = data };

    }
}
