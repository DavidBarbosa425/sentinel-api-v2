namespace sentinel_api.Application.Common
{
    public class Result
    {
        public bool Succees { get; set; }
        public string Message { get; set; } = string.Empty;

        public static Result Success(string message = "Operação realizada com sucesso")
            => new Result { Succees = true, Message = message };

        public static Result Failure(string message = "Erro na operação")
            => new Result { Succees = false, Message = message };
    }

    public class Result<T> : Result
    {
        public T Data { get; set; } = default!;
        public static Result<T> Success(T data, string message = "Operação realizada com sucesso")
            => new Result<T> { Succees = true, Message = message, Data = data };

    }
}
