using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevPermDefLang
    {
        [Key]
        public ulong Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string? Cat { get; set; }
    }
}
