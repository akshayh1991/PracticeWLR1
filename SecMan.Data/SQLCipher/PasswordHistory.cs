using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class PasswordHistory
    {
        [Key]
        public int EntryId { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
