using System;

namespace SA_ToolBelt
{
    public static class CredentialManager
    {
        private static string _username;
        private static string _password;
        private static string _domain;
        
        public static bool IsAuthenticated => !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password);
        
        public static void SetCredentials(string username, string password, string domain = null)
        {
            _username = username;
            _password = password;
            _domain = domain ?? Environment.UserDomainName;
        }
        
        public static void ClearCredentials()
        {
            _username = null;
            _password = null;
            _domain = null;
        }
        
        public static string GetUsername() => _username;
        public static string GetPassword() => _password;
        public static string GetDomain() => _domain;
        
        // Formatted for LDAP DN style
        public static string GetLdapUsername() => $"cn={_username}";
        
        // For future ESXi use
        public static (string username, string password) GetCredentials() => (_username, _password);
    }
}