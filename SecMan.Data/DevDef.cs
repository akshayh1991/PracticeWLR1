using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data
{
    public class DevDef
    {

        internal DevDef(string langCode, SQLCipher.DevDef devDef)
        {
            Id = devDef.Id;

            SQLCipher.DevDefLang lang = null;
            if (langCode != null)
            {
                lang = devDef.Langs.Where(o => o.Code == langCode).FirstOrDefault();
            }
            if (lang == null)
            {
                lang = new();
            }
            if (string.IsNullOrEmpty(lang.Name)) lang.Name = devDef.Name;

            Name = lang.Name;


            App = devDef.App;
            Vers = devDef.Vers;
            devDef.DevPolDefs.ForEach(o => DevPolDefs.Add(new(langCode, o)));
            devDef.DevPermDefs.ForEach(o => DevPermDefs.Add(new(langCode, o)));
        }


        public ulong Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public bool App { get; set; }

        public ulong Vers { get; set; }

        public List<DevPolDef>? DevPolDefs { get; set; } = [];
        public List<DevPermDef>? DevPermDefs { get; set; } = [];
    }
}
