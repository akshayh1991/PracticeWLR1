using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class SuperUser
    {
        private enum Property
        {
            UserName,
            Password
        }

        internal SuperUser()
        {
        }

        [Key]
        public ulong Id { get; set; } = 0;
        public string? UserName { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
    }
}
