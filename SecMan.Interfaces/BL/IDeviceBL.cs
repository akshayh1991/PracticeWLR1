using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Interfaces.BL
{
    public interface IDeviceBL
    {
        Task<ServiceResponse<GetDevice>> AddDeviceAsync(CreateDevice createDevice, bool saveToDb = false);
        Task<ServiceResponse<GetDevice?>> UpdateDeviceAsync(ulong id, UpdateDevice updateDevice, bool saveToDb = false);
        Task<ApiResponse> DeleteDeviceAsync(ulong id, bool saveToDb = false);
    }
}
