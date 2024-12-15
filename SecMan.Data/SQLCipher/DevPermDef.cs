using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevPermDef
    {
        [Key]
        public ulong Id { get; set; }
        public ulong Vers { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string? Cat { get; set; }
        public int Posn { get; set; }
        public List<DevPermDefLang>? Langs { get; set; }
    }
}
