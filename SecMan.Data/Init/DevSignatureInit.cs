using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class DevSigInit
    {
        public string Zone { get; set; } = string.Empty;
        public string DevDef { get; set; } = string.Empty;
        public List<SigInit> Signatures { get; set; } = [];
    }
}
