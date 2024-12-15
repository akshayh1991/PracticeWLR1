using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data
{
    public class DevSigVal
    {
        internal DevSigVal(SQLCipher.DevSigVal devSigVal)
        {
            Id = devSigVal.Id;
            Name = devSigVal.DevPermDef.Name;
            Sign = devSigVal.Sign;
            Auth = devSigVal.Auth;
            Note = devSigVal.Note;
        }

        public string? Name { get; set; }
        public ulong Id { get; set; }
        public bool Sign { get; set; } = false;
        public bool Auth { get; set; } = false;
        public bool Note { get; set; } = false;
    }
}
