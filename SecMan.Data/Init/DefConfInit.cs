using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Init
{
    public class DefConfInit
    {
        public List<UserInit> Users { get; set; } = [];
        public List<RoleInit> Roles { get; set; } = [];
        public List<DevInit> Devs { get; set; } = [];
        public List<string> Zones { get; set; } = [];
        public List<DevPolInit> DevPols { get; set; } = [];
        public List<DevSigInit> DevSigs { get; set; } = [];
        public List<DevPermInit> DevPerms { get; set; } = [];

    }
}
