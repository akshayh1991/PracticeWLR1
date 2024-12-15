using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class SysFeat
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool Common { get; set; } = false;
        public bool TestConnection { get; set; } = false;
        public List<SysFeatLang>? Langs { get; set; }
        public List<SysFeatProp>? SysFeatProps { get; set; } = [];
    }
}
