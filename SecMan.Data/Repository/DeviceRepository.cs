using Microsoft.EntityFrameworkCore;
using SecMan.Data.DAL;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using System.Data;
using System.Linq.Expressions;

namespace SecMan.Data.Repository
{
    public class DeviceRepository:IDeviceRepository
    {
        private readonly Db _context;

        public DeviceRepository(Db context)
        {
                _context = context;
        }
        public async Task<GetDevice> AddDeviceAsync(CreateDevice device)
        {
            var DevDef=_context.DevDefs.Where(x => x.Id == device.TypeId).FirstOrDefault();
            var zone=_context.Zones.Where(x => x.Id == device.ZoneId).FirstOrDefault();
            SQLCipher.Dev dev = new SQLCipher.Dev
            {
                Name = device.Name,
                DevDef = DevDef,
                Zone = zone,
                Ip = device.Ip,
                DeploymentStatus = device.DeploymentStatus,
                Legacy = device.IsLegacy
            };
            await _context.Devs.AddAsync(dev);
            return new GetDevice
            {
                Id=dev.Id,
                Name = device.Name,
                TypeId = device.TypeId,
                Ip = device.Ip,
                ZoneId = device.ZoneId,
                DeploymentStatus = device.DeploymentStatus,
                IsLegacy = device.IsLegacy
            };
        }
        public async Task<SecMan.Data.SQLCipher.Dev> GetDeviceByDevicename(string? devicename)
        {
            if (devicename != null)
            {
                return await _context.Devs.Where(x => x.Name != null &&
                                                  x.Name.ToLower().Equals(devicename.ToLower()))
                                                  .FirstOrDefaultAsync();
            }
            return null;
        }
        public async Task<bool> IsTypeIdExists(ulong? typeId)
        {
            return await _context.DevDefs.AnyAsync(x => x.Id == typeId);
        }
        public async Task<bool> IsZoneIdExists(ulong? zoneId)
        {
            return await _context.Zones.AnyAsync(x => x.Id == zoneId);
        }

        public async Task<GetDevice> UpdateDeviceAsync(ulong id, UpdateDevice updateDevice)
        {
            SQLCipher.Dev device = await _context.Devs.FirstOrDefaultAsync(x => x.Id == id);
            var DevDef = _context.DevDefs.Where(x => x.Id == updateDevice.TypeId).FirstOrDefault();
            var zone = _context.Zones.Where(x => x.Id == updateDevice.ZoneId).FirstOrDefault();
            if (device == null)
            {
                return null;
            }
            device.Name = !string.IsNullOrEmpty(updateDevice.Name) ? updateDevice.Name : device.Name;
            device.DevDef = !string.IsNullOrEmpty(updateDevice.TypeId.ToString()) ? DevDef : device.DevDef;
            device.Ip = !string.IsNullOrEmpty(updateDevice.Ip) ? updateDevice.Ip : device.Ip;
            device.Zone = !string.IsNullOrEmpty(updateDevice.ZoneId.ToString()) ? zone : device.Zone;
            device.DeploymentStatus = !string.IsNullOrEmpty(updateDevice.DeploymentStatus) ? updateDevice.DeploymentStatus : device.DeploymentStatus;
            //device.Legacy = !string.IsNullOrEmpty(updateDevice.IsLegacy.ToString()) ? updateDevice.IsLegacy : device.Legacy;
            device.Legacy = !string.IsNullOrEmpty(updateDevice.IsLegacy.ToString()) ? updateDevice.IsLegacy ?? false : device.Legacy;

            _context.Devs.Update(device);

            return new GetDevice
            {
                Id = id,
                Name=device.Name,
                TypeId = device.DevDef?.Id ?? 0,
                Ip = device.Ip,
                ZoneId = device.Zone.Id,
                DeploymentStatus = device.DeploymentStatus,
                IsLegacy = device.Legacy
            };
        }
        public async Task<bool> IsDeviceNameExistsAsync(ulong id, string name)
        {
            return await _context.Devs.AnyAsync(x => x.Name == name && x.Id != id);
        }
        public async Task<GetDevice?> GetDeviceByIdAsync(ulong id)
        {
            return await _context.Devs
                                 .Where(x => x.Id == id)
                                 .Select(x => new GetDevice
                                 {
                                     Id = x.Id,
                                     Name = x.Name,
                                     TypeId = x.DevDef.Id,
                                     ZoneId= x.Zone.Id,
                                     Ip= x.Ip,
                                     DeploymentStatus= x.DeploymentStatus,
                                     IsLegacy = x.Legacy
                                 })
                                 .FirstOrDefaultAsync();
        }


        public async Task<bool> DeleteDeviceAsync(ulong id)
        {
                SQLCipher.Dev device = await _context.Devs.FirstOrDefaultAsync(x => x.Id == id);
                _context.Devs.Remove(device);
                return true;
        }

    }
}
