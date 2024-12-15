using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class Role
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public List<User> Users { get; set; } = [];

        public List<Zone> Zones { get; set; } = [];

        public string? Description { get; set; }
        public bool? IsLoggedOutType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
