using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Models
{
    public class UserCredentials
    {
        public string Id { get; set; }
        public string UserPrincipalName { get; set; }
        public string Password { get; set; }
    }
}
