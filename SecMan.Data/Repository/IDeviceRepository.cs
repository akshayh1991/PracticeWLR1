using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Repository
{
    public interface IDeviceRepository
    {
        Task<GetDevice> AddDeviceAsync(CreateDevice device);
        Task<SecMan.Data.SQLCipher.Dev> GetDeviceByDevicename(string? devicename);
        Task<bool> IsTypeIdExists(ulong? typeId);
        Task<bool> IsZoneIdExists(ulong? zoneId);
        Task<GetDevice> UpdateDeviceAsync(ulong id, UpdateDevice updateDevice);
        Task<bool> IsDeviceNameExistsAsync(ulong id, string name);
        Task<GetDevice?> GetDeviceByIdAsync(ulong id);
        Task<bool> DeleteDeviceAsync(ulong id);
    }
}
