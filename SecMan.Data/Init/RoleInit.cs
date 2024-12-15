using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class RoleInit
    {
        public string? Name { get; set; }
        public List<string> Zones { get; set; } = [];
    }
}
