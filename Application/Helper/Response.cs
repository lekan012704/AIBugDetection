namespace Application.Helper
{
    public class Response<T>
    {
        public Response()
        {
        }
        public Response(T data, bool succeed, string message = null)
        {
            Succeeded = succeed;
            Message = message;
            Data = data;
        }
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }
        public Response(string message, bool succeed)
        {
            Succeeded = succeed;
            Message = message;
        }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
    }
}
