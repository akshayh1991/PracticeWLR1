using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevPolDef
    {
        [Key]
        public ulong Id { get; set; }
        public ulong Vers { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Cat { get; set; } = string.Empty;
        public int Posn { get; set; } = 0;
        public string ValType { get; set; } = string.Empty;
        public double ValMin { get; set; } = 0;
        public double ValMax { get; set; } = 0;
        public string ValDflt { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public List<DevPolDefLang>? Langs { get; set; }
    }
}
