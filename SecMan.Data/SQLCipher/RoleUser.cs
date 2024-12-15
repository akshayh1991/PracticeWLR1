using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    internal class RoleUser
    {
        [Key]
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong UserId { get; set; }
    }
}
