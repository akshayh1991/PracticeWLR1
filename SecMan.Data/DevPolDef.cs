using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;

namespace SecMan.Data
{
    public class DevPolDef
    {
        internal DevPolDef(string langCode, SQLCipher.DevPolDef devPolDef)
        {
            Id = devPolDef.Id;
            Vers = devPolDef.Vers;

            SQLCipher.DevPolDefLang lang = null;
            if (langCode != null)
            {
                lang = devPolDef.Langs.Where(o => o.Code == langCode).FirstOrDefault();
            }
            if (lang == null)
            {
                lang = new();
            }
            if (string.IsNullOrEmpty(lang.Name)) lang.Name = devPolDef.Name;
            if (string.IsNullOrEmpty(lang.Name)) lang.Desc = devPolDef.Desc;
            if (string.IsNullOrEmpty(lang.Name)) lang.Cat = devPolDef.Cat;
            if (string.IsNullOrEmpty(lang.Name)) lang.Units = devPolDef.Units;

            Name = lang.Name;
            Desc = lang.Desc;
            Cat = lang.Cat;
            Units = lang.Units;
            Posn = devPolDef.Posn;
            ValType valType = ValType.None;
            Enum.TryParse(devPolDef.ValType, out valType);
            ValType = valType;
            ValMin = devPolDef.ValMin;
            ValMax = devPolDef.ValMax;
            ValDflt = devPolDef.ValDflt;
        }
        public ulong Id { get; set; }
        public ulong Vers { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public string? Desc { get; set; } = string.Empty;
        public string? Cat { get; set; } = string.Empty;
        public int Posn { get; set; } = 0;
        public ValType ValType { get; set; } = ValType.None;
        public double ValMin { get; set; } = 0;
        public double ValMax { get; set; } = 0;
        public string? ValDflt { get; set; } = string.Empty;
        public string? Units { get; set; } = string.Empty;
    }
}
