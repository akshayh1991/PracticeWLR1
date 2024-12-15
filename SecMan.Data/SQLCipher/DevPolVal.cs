using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevPolVal
    {
        [Key]
        public ulong Id { get; set; }
        public Zone? Zone { get; set; }
        public DevDef? DevDef { get; set; }
        public DevPolDef? DevPolDef { get; set; }
        public string? Val { get; set; }
    }
}
