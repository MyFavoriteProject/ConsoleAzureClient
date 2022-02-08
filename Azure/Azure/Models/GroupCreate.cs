using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Models
{
    public class GroupCreate
    {
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public bool MailEnabled { get; set; }
        public bool SecurityEnabled { get; set; }

        public List<string> GroupTypes { get; set; } = new List<string>
        {
            "Unified" /// Указывает на Microsoft 365 group
        };
    }
}
