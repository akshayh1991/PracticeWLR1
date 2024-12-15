using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class InitFileType
    {
        [Key]
        public ulong Id { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public ulong Vers { get; set; }
    }
}
