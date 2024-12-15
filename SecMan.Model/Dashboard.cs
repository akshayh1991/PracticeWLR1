
namespace SecMan.Model
{
    public class Dashboard
    {
        public int Zones { get; set; }
        public int ZonesCreatedRecently { get; set; }
        public int Users { get; set; }
        public int UsersCreatedRecently { get; set; }
        public int Roles { get; set; }
        public int RolesCreatedRecently { get; set; }
        public int Devices { get; set; }
        public int DevicesNotConfigured { get; set; }
    }
}
