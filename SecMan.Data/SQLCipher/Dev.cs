using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class Dev
    {
        [Key]
        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DevDef? DevDef { get; set; }
        public Zone? Zone { get; set; }
        public string Vers { get; set; } = string.Empty;
        public bool Legacy { get; set; }
        public ulong SysPolVer { get; set; }
        public ulong DevPolVer { get; set; }
        public ulong DevPermVer { get; set; }
        public ulong UserVer { get; set; }
        public ulong RoleVer { get; set; }
        public int ConnRate { get; set; }
        public int ConnState { get; set; }
        public DateTime LastConnDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Ip { get; set; }
        public string DeploymentStatus { get; set; }
    }
}
