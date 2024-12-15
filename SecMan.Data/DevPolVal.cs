using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;

namespace SecMan.Data
{
    public class DevPolVal
    {
        internal DevPolVal(SQLCipher.DevPolVal devPolVal)
        {
            Id = devPolVal.Id;
            Name = devPolVal.DevPolDef.Name;
            Desc = devPolVal.DevPolDef.Desc;
            Cat = devPolVal.DevPolDef.Cat;
            Posn = devPolVal.DevPolDef.Posn;
            ValType valType = ValType.None;
            Enum.TryParse(devPolVal.DevPolDef.ValType, out valType);
            ValType = valType;
            ValMin = devPolVal.DevPolDef.ValMin;
            ValMax = devPolVal.DevPolDef.ValMax;
            ValDflt = devPolVal.DevPolDef.ValDflt;
            Units = devPolVal.DevPolDef.Units;
            Val = devPolVal.Val;
        }

        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Cat { get; set; } = string.Empty;
        public int Posn { get; set; } = 0;
        public ValType ValType { get; set; } = ValType.None;
        public double ValMin { get; set; } = 0;
        public double ValMax { get; set; } = 0;
        public string ValDflt { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public string Val { get; set; } = string.Empty;
     }
}
