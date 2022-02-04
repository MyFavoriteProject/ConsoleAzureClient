using System;
using System.Collections.Generic;

namespace Azure.Models
{
    public class UserAdditionalInfo
    {
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
    }
}
