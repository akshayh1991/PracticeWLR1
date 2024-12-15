using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class SysFeatProp
    {
        [Key]
        public ulong Id { get; set; }
        public ulong Vers { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string? Units { get; set; }
        public string? Cat { get; set; }
        public int Posn { get; set; }
        public string ValType { get; set; } = string.Empty;
        public ulong ValMin { get; set; }
        public ulong ValMax { get; set; }
        public string? Val { get; set; }
        public List<SysFeatPropLang>? Langs { get; set; }
        public SysFeat? SysFeat { get; set; }
    }
}
