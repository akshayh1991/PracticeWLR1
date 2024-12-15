using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class Applications
    {
       public ulong Id { get; set; }
      public string Name { get; set; } 
      public DateTime InstalledDate { get; set; }
    }
}
