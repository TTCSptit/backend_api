namespace job.Dtos
{
    public enum AuthResultStatus
    {
        Success,        // 200 OK or 201 Created
        Conflict,       // 409 Conflict (Email/Company exists)
        ValidationError, // 400 or 422 (Identity errors, weak password)
        NotFound,       // 404 (Used for Login if user doesn't exist)
        Unauthorized,   // 401 (Wrong password)
        Failure         // 500 or generic errors
    }

    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public AuthResultStatus Status { get; set; }

        public static AuthResult Ok(object? data, string? message = null) =>
            new() { Succeeded = true, Status = AuthResultStatus.Success, Data = data, Message = message };

        public static AuthResult Error(AuthResultStatus status, string message) =>
            new() { Succeeded = false, Status = status, Message = message };
    }
}