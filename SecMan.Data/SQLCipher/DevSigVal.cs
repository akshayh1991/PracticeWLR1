using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevSigVal
    {
        [Key]
        public ulong Id { get; set; }
        public Zone? Zone { get; set; }
        public DevDef? DevDef { get; set; }
        public Role? Role { get; set; }
        public DevPermDef? DevPermDef { get; set; }
        public bool Sign { get; set; } = false;
        public bool Auth { get; set; } = false;
        public bool Note { get; set; } = false;
    }
}
