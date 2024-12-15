using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class DevPolInit
    {
        public string? Zone { get; set; }
        public string? DevDef { get; set; }
        public List<PolInit> Pols { get; set; } = [];
    }
}
