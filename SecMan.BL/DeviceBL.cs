using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;

namespace SecMan.BL
{
    public class DeviceBL: IDeviceBL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPendingChangesManager _pendingChangesManager;

        public DeviceBL(IUnitOfWork unitOfWork,IPendingChangesManager pendingChangesManager)
        {
            _unitOfWork = unitOfWork;
            _pendingChangesManager = pendingChangesManager;
        }
        public async Task<ServiceResponse<GetDevice>> AddDeviceAsync(CreateDevice createDevice, bool saveToDb = false)
        {
            Data.SQLCipher.Dev? device = await _unitOfWork.IDeviceRepository.GetDeviceByDevicename(createDevice.Name);
            if (device is not null)
            {
                return new ServiceResponse<GetDevice>(ResponseConstants.DeviceAlreadyExists, HttpStatusCode.Conflict);
            }
            bool isTypeIdExits=await _unitOfWork.IDeviceRepository.IsTypeIdExists(createDevice.TypeId);
            if (!isTypeIdExits) 
            {
                return new ServiceResponse<GetDevice>(ResponseConstants.DeviceInvalidTypeId, HttpStatusCode.BadRequest);
            }
            bool isZoneIdExits = await _unitOfWork.IDeviceRepository.IsZoneIdExists(createDevice.ZoneId);
            if (!isZoneIdExits)
            {
                return new ServiceResponse<GetDevice>(ResponseConstants.DeviceInvalidZoneId, HttpStatusCode.BadRequest);
            }
            if (saveToDb)
            {
                GetDevice deviceResponse = await _unitOfWork.IDeviceRepository.AddDeviceAsync(createDevice);
                return new ServiceResponse<GetDevice>(ResponseConstants.Success, HttpStatusCode.OK,deviceResponse);
            }
            else
            {
                var response = await _pendingChangesManager.AddToSessionJsonAsync(createDevice, Common.JsonEntities.Device);
                return new(response.Message, response.StatusCode);
            }
        }

        public async Task<ServiceResponse<GetDevice?>> UpdateDeviceAsync(ulong id, UpdateDevice updateDevice, bool saveToDb = false)
        {
            GetDevice? result = await _unitOfWork.IDeviceRepository.GetDeviceByIdAsync(id);
            if (result == null)
            {
                var badRequestResponse = new NotFound(DeviceResponseConstants.BadRequest, DeviceResponseConstants.InvalidDeviceId);
                return new ServiceResponse<GetDevice?>
                {
                    Message = badRequestResponse.Detail,
                    StatusCode = badRequestResponse.Status,
                    Data = null
                };
            }
            bool isDeviceNameExists = await _unitOfWork.IDeviceRepository.IsDeviceNameExistsAsync(id, updateDevice.Name);
            if (isDeviceNameExists)
            {
                return new ServiceResponse<GetDevice?>(ResponseConstants.DeviceAlreadyExists, HttpStatusCode.Conflict);
            }
            if (updateDevice.TypeId.HasValue)
            {


                bool isTypeIdExits = await _unitOfWork.IDeviceRepository.IsTypeIdExists(updateDevice.TypeId);
                if (!isTypeIdExits)
                {
                    return new ServiceResponse<GetDevice?>(ResponseConstants.DeviceInvalidTypeId, HttpStatusCode.BadRequest);
                }
            }
            if (updateDevice.ZoneId.HasValue)
            {


                bool isZoneIdExits = await _unitOfWork.IDeviceRepository.IsZoneIdExists(updateDevice.ZoneId);
                if (!isZoneIdExits)
                {
                    return new ServiceResponse<GetDevice?>(ResponseConstants.DeviceInvalidZoneId, HttpStatusCode.BadRequest);
                }
            }

            if (saveToDb)
            {
                GetDevice savedResult = await _unitOfWork.IDeviceRepository.UpdateDeviceAsync(id, updateDevice);
                return new ServiceResponse<GetDevice?>(ResponseConstants.Success, HttpStatusCode.OK, savedResult);
            }
            else
            {
                var device = await _unitOfWork.IDeviceRepository.GetDeviceByIdAsync(id);
                var originalObject = new UpdateDevice
                {
                    Name = device.Name,
                    TypeId=device.TypeId,
                    ZoneId = device.ZoneId,
                    Ip = device.Ip,
                    DeploymentStatus=device.DeploymentStatus,
                    IsLegacy=device.IsLegacy
                };
                var response = await _pendingChangesManager.UpdateToSessionJsonAsync(updateDevice, JsonEntities.Device, originalObject, device.Id, device.Name);
                return new ServiceResponse<GetDevice?>(response.Message, response.StatusCode);
            }
        }

        public async Task<ApiResponse> DeleteDeviceAsync(ulong id, bool saveToDb = false)
        {
            var device = await _unitOfWork.IDeviceRepository.GetDeviceByIdAsync(id);
            if (device == null)
            {
                return new ApiResponse(DeviceResponseConstants.InvalidDeviceId, HttpStatusCode.NotFound);
            }

            if (saveToDb)
            {
                _ = await _unitOfWork.IDeviceRepository.DeleteDeviceAsync(id);
            }
            else
            {
                await _pendingChangesManager.DeleteToSessionJsonAsync(JsonEntities.Device, device.Id, device.Name);
            }
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }
    }
}
