using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Models
{
    public class GroupInfo
    {
        public string Id { get; set; }
        public bool? AllowExternalSenders { get; set; }
        public bool? AutoSubscribeNewMembers { get; set; }
        public bool? HideFromAddressLists { get; set; }
        public bool? HideFromOutlookClients { get; set; }
    }
}
