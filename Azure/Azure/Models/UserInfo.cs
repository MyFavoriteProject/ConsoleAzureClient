using System;
using System.Collections.Generic;

namespace Azure.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string UserPrincipalName { get; set; }
        public bool? AccountEnabled { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public string GivenName { get; set; }
        public PasswordPolicies PasswordPolicies { get; set; }
        public IEnumerable<string> BusinessPhones { get; set; }
        
        #region Additional props

        public string AboutMe { get; set; }
        public DateTimeOffset? Birthday { get; set; }
        public DateTimeOffset? HireDate { get; set; }
        public IEnumerable<string> Interests { get; set; }
        public string MySite { get; set; }
        public IEnumerable<string> PastProjects { get; set; }
        public string PreferredName { get; set; }
        public IEnumerable<string> Responsibilities { get; set; }
        public IEnumerable<string> Schools { get; set; }
        public IEnumerable<string> Skills { get; set; }

        #endregion

    }
}
