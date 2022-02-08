namespace Azure.Models
{
    public class UserCreate
    {
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public bool? AccountEnabled { get; set; }
        public string Password { get; set; }
        public bool? ForceChangePasswordNextSignIn { get; set; }
    }
}
