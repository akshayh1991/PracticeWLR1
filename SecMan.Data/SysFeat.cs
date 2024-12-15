using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;

namespace SecMan.Data
{
    public class SysFeat
    {
        public SysFeat() { }
        internal SysFeat(string langCode, SQLCipher.SysFeat sysFeat)
        {
            Id = sysFeat.Id;
            Common = sysFeat.Common;

            SQLCipher.SysFeatLang lang = null;
            if (langCode != null)
            {
                lang = sysFeat.Langs.Where(o => o.Code == langCode).FirstOrDefault();
            }
            if (lang == null)
            {
                lang = new();
            }
            if (string.IsNullOrEmpty(lang.Name)) lang.Name = sysFeat.Name;

            Name = lang.Name;
            sysFeat.SysFeatProps.ToList().ForEach(o => SysFeatProps.Add(new(langCode, o)));
        }
        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Common { get; set; } = false;
        public List<SysFeatProp>? SysFeatProps { get; set; } = [];
    }
}
