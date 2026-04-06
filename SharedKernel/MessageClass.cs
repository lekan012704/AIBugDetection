namespace SharedKernel
{
    public class MessageClass<T>
    {
        public int StatusId { get; set; }
        public string? Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
        public bool IsSuccessful => StatusId == 1;
        public bool Succeeded => IsSuccessful;
    }

    public static class MessageClassExtensions
    {
        public static MessageClass<T> Success<T>(T data, string message = "Successful")
        {
            return new MessageClass<T>
            {
                StatusId = 1,
                Message = message,
                Data = data,
            };
        }

        public static MessageClass<T> Failure<T>(string message, T defaultValue = default!)
        {
            return new MessageClass<T>
            {
                StatusId = 0,
                Message = message,
                Data = defaultValue
            };
        }
    }
    public class MessageClass
    {
        public int StatusId { get; set; }
        public string Message { get; set; } = string.Empty;
        public object Data { get; set; } = default!;
        public bool IsSuccessful => StatusId == 1;
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = [];
        public bool Succeeded => IsSuccessful;
    }

    public class TaxSmartResponse
    {
        public object ReferenceNumber { get; set; } = default!;
        public bool Status { get; set; }
        public object ResponseObject { get; set; } = default!;
        public object StatusCode { get; set; } = default!;
        public string StatusMessage { get; set; } = string.Empty;
    }
    public class TaxSmartResponse<T>
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public bool Status { get; set; }
        public T ResponseObject { get; set; } = default!;
        public string StatusCode { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }

    public class MessageClassUsage
    {
        public MessageClass<bool> DeleteUser(int id)
        {
            var user = "'";

            if (user == null)
            {
                return MessageClassExtensions.Failure("User not found", false);
            }

            if (user == null)
            {
                //return MessageClassExtensions.Failure<User>("User not found");
            }

            //return MessageClassExtensions.Success(user, "User retrieved successfully");

            return MessageClassExtensions.Success(true, "User deleted successfully");
        }
    }

}
