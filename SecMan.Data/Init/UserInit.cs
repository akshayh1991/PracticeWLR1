using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class UserInit
    {
        public ulong Id { get; set; }
        public string? Domain { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public List<string>? Roles { get; set; }
    }
}
