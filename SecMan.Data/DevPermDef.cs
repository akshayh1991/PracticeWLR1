using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SecMan.Data
{
    public class DevPermDef
    {
        internal DevPermDef(string langCode, SQLCipher.DevPermDef devPermDef)
        {
            Id = devPermDef.Id;
            Vers = devPermDef.Vers;

            SQLCipher.DevPermDefLang lang = null;
            if (langCode != null)
            {
                lang = devPermDef.Langs.Where(o => o.Code == langCode).FirstOrDefault();
            }
            if (lang == null)
            {
                lang = new();
            }
            if (string.IsNullOrEmpty(lang.Name)) lang.Name = devPermDef.Name;
            if (string.IsNullOrEmpty(lang.Name)) lang.Desc = devPermDef.Desc;
            if (string.IsNullOrEmpty(lang.Name)) lang.Cat = devPermDef.Cat;

            Name = lang.Name;
            Desc = lang.Desc;
            Cat = lang.Cat;
            Posn = devPermDef.Posn;
        }
        public ulong Id { get; set; }
        public ulong Vers { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public string? Cat { get; set; }
        public int Posn { get; set; }

    }
}
