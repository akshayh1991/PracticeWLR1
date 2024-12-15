using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class DevPermInit
    {
        public string Zone { get; set; } = string.Empty;
        public string DevDef { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Perms { get; set; } = [];
    }
}
