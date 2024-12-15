using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class SysFeatPropLang
    {
        [Key]
        public ulong Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? Desc { get; set; } = string.Empty;
        public string? Cat { get; set; } = string.Empty;
        public string? Units { get; set; } = string.Empty;
        public ulong SysFeatPropId { get; set; }
    }
}
