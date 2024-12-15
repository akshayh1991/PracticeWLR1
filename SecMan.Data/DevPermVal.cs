using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data
{
    public class DevPermVal
    {
        internal DevPermVal(SQLCipher.DevPermVal devPermVal)
        {
            Id = devPermVal.Id;
            Name = devPermVal.DevPermDef.Name;
            Cat = devPermVal.DevPermDef.Cat;
            Desc = devPermVal.DevPermDef.Desc;
            Val = devPermVal.Val;
        }

        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Cat { get; set; }
        public string? Desc { get; set; }
        public bool Val { get; set; }
    }
}
