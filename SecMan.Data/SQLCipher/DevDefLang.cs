﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class DevDefLang
    {
        [Key]
        public ulong Id { get; set; }
        public string? Code { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
    }
}
