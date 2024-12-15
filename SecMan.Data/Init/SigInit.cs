using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class SigInit
    {
        public string Perm { get; set; } = string.Empty;
        public bool Sign { get; set; } = false;
        public bool Auth { get; set; } = false;
        public bool Note { get; set; } = false;
    }
}
