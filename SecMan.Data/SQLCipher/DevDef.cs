using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevDef
    {
        [Key]
        public ulong Id { get; set; } = 0;

        public string? Name { get; set; } = string.Empty;
        public bool App { get; set; } = false;

        public ulong Vers { get; set; } = 0;
        public List<DevDefLang>? Langs { get; set; } = new ();

        public List<DevPolDef>? DevPolDefs { get; set; } = new();
        public List<DevPermDef>? DevPermDefs { get; set; } = new();
    }
}
