using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;

namespace SecMan.Data
{
    public class SysFeatProp
    {
        internal SysFeatProp(string langCode, SQLCipher.SysFeatProp sysFeatProp)
        {
            SQLCipher.SysFeatPropLang lang = null;
            if (langCode != null)
            {
                lang = sysFeatProp.Langs.Where(o => o.Code == langCode).FirstOrDefault();
            }
            if (lang == null)
            {
                lang = new();
            }
            if (string.IsNullOrEmpty(lang.Name)) lang.Name = sysFeatProp.Name;
            if (string.IsNullOrEmpty(lang.Desc)) lang.Desc = sysFeatProp.Desc;
            if (string.IsNullOrEmpty(lang.Units)) lang.Units = sysFeatProp.Units;
            if (string.IsNullOrEmpty(lang.Cat)) lang.Cat = sysFeatProp.Cat;


            Id = sysFeatProp.Id;
            Vers = sysFeatProp.Vers;
            Name = lang.Name;
            Desc = lang.Desc;
            Units = lang.Units;
            Cat = lang.Cat;
            Posn = sysFeatProp.Posn;
            ValType valType = ValType.None;
            Enum.TryParse(sysFeatProp.ValType, out valType);
            ValType = valType;
            ValMin = sysFeatProp.ValMin;
            ValMax = sysFeatProp.ValMax;

            Val = sysFeatProp.Val;
        }
        public ulong Id { get; set; }
        public ulong Vers { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public string? Desc { get; set; } = string.Empty;
        public string? Units { get; set; } = string.Empty;
        public string? Cat { get; set; } = string.Empty;
        public int Posn { get; set; } = 0;
        public ValType ValType { get; set; } = ValType.None;
        public ulong ValMin { get; set; } = 0;
        public ulong ValMax { get; set; } = 0;
        public string? Val { get; set; } = string.Empty;
    }
}
