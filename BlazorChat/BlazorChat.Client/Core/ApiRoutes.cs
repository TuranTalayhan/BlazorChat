namespace BlazorChat.Client.Core;

public static class ApiRoutes
{
    public static class Users
    {
        public const string GetStatus = "api/users/me/status";
        public const string UpdateStatus = "api/users/me/status";
    }
    
    public static class Channels
    {
        private const string Base = "api/channels";
        public static string GetMessages(Guid id) => $"{Base}/{id}/messages";
    }

    public static class Messages
    {
        
    }

    public static class Auth
    {
        public const string Login = "api/auth/login";
        public const string Register = "api/auth/register";
        public const string Logout = "api/auth/logout";
        public const string Status = "api/auth/status";
        public const string Me = "api/auth/me";    
    }
}