using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class RSAKeys
    {
        [Key]
        public ulong Id { get; set; }

        public string PrivateKey { get; set; }

        public string PublicKey { get; set; }
    }
}
