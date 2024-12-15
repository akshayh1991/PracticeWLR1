using Microsoft.AspNetCore.Mvc;
using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Data;
using System.Net;

namespace SecMan.BL
{
    public class RoleBL : IRoleBL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPendingChangesManager _pendingChangesManager;

        public RoleBL(IUnitOfWork unitOfWork,
                      IPendingChangesManager pendingChangesManager)
        {
            _unitOfWork = unitOfWork;
            _pendingChangesManager = pendingChangesManager;
        }
        public async Task<IEnumerable<GetRoleDto>> GetAllRolesAsync()
        {
            IEnumerable<Data.SQLCipher.Role> result = await _unitOfWork.IRoleRepository.GetAll(r => r.Users);

            return result.
                OrderBy(r => r.Name).
                Select(r => new GetRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsLoggedOutType = r.IsLoggedOutType ?? false,
                    NoOfUsers = r.Users.Count
                }).ToList();
        }
        public async Task<ServiceResponse<GetRoleDto?>> GetRoleByIdAsync(ulong id)
        {
            Data.SQLCipher.Role result = await _unitOfWork.IRoleRepository.GetById(id, r => r.Users);
            if (result == null)
            {
                var badRequestResponse = new NotFound(RoleResponseConstants.BadRequest, RoleResponseConstants.InvalidRoleId);
                return new ServiceResponse<GetRoleDto?>
                {
                    Message = badRequestResponse.Detail,
                    StatusCode = badRequestResponse.Status,
                    Data = null
                };
            }
            GetRoleDto returnRole = new GetRoleDto
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                IsLoggedOutType = result.IsLoggedOutType ?? false,
                NoOfUsers = result.Users.Count
            };

            return new ServiceResponse<GetRoleDto?>(ResponseConstants.Success, HttpStatusCode.OK, returnRole);
        }
        public async Task<ServiceResponse<GetRoleDto>> AddRoleAsync(CreateRole addRoleDto, bool saveToDb = false)
        {
            bool isRoleNameExists = await _unitOfWork.IRoleRepository.IsRoleNameExistsForCreationAsync(addRoleDto.Name);
            if (isRoleNameExists)
            {
                var badRequestResponse = new BadRequest(RoleResponseConstants.BadRequest, RoleResponseConstants.RoleAlreadyExists);
                return new ServiceResponse<GetRoleDto>
                {
                    Message = badRequestResponse.Detail,
                    StatusCode = badRequestResponse.Status,
                    Data = null
                };
            }
            if (addRoleDto.LinkUsers != null)
            {
                bool validateLinkUsers = await _unitOfWork.IRoleRepository.ValidateLinkUsersAsync(addRoleDto.LinkUsers);
                if (!validateLinkUsers)
                {
                    var badRequestResponse = new BadRequest(RoleResponseConstants.BadRequest, RoleResponseConstants.InvalidUserIds);
                    return new ServiceResponse<GetRoleDto>
                    {
                        Message = badRequestResponse.Detail,
                        StatusCode = badRequestResponse.Status,
                        Data = null
                    };
                }
            }

            if (saveToDb)
            {
                Data.SQLCipher.Role result = await _unitOfWork.IRoleRepository.AddRoleAsync(addRoleDto);
                GetRoleDto retGetRoleDto = new GetRoleDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description,
                    IsLoggedOutType = result.IsLoggedOutType ?? false,
                    NoOfUsers = addRoleDto.LinkUsers?.Count ?? 0
                };
                return new ServiceResponse<GetRoleDto>(ResponseConstants.Success, HttpStatusCode.OK, retGetRoleDto);
            }
            else
            {
                var response = await _pendingChangesManager.AddToSessionJsonAsync(addRoleDto, Common.JsonEntities.Role);
                return new(response.Message, response.StatusCode);
            }
        }


        public async Task<ServiceResponse<GetRoleDto?>> UpdateRoleAsync(ulong id, UpdateRole addRoleDto, bool saveToDb = false)
        {
            bool isRoleNameExists = await _unitOfWork.IRoleRepository.IsRoleNameExistsAsync(id, addRoleDto.Name);
            if (isRoleNameExists)
            {
                var badRequestResponse = new BadRequest(RoleResponseConstants.BadRequest, RoleResponseConstants.RoleAlreadyExists);
                return new ServiceResponse<GetRoleDto?>
                {
                    Message = badRequestResponse.Detail,
                    StatusCode = badRequestResponse.Status,
                    Data = null
                };
            }
            if (addRoleDto.LinkUsers != null)
            {
                bool validateLinkUsers = await _unitOfWork.IRoleRepository.ValidateLinkUsersAsync(addRoleDto.LinkUsers);
                if (!validateLinkUsers)
                {
                    var badRequestResponse = new BadRequest(RoleResponseConstants.BadRequest, RoleResponseConstants.InvalidUserIds);
                    return new ServiceResponse<GetRoleDto?>
                    {
                        Message = badRequestResponse.Detail,
                        StatusCode = badRequestResponse.Status,
                        Data = null
                    };
                }
            }
            if (saveToDb)
            {
                GetRoleDto result = await _unitOfWork.IRoleRepository.UpdateRoleFromJsonAsync(id, addRoleDto);
                return new ServiceResponse<GetRoleDto?>(ResponseConstants.Success, HttpStatusCode.OK, result);
            }
            else
            {
                var role = await _unitOfWork.IRoleRepository.GetById(id, x => x.Users);
                var originalObject = new UpdateRole
                {
                    Description = role.Description,
                    IsLoggedOutType = role.IsLoggedOutType,
                    LinkUsers = role.Users.Select(x => x.Id).ToList(),
                    Name = role.Name
                };
                var response = await _pendingChangesManager.UpdateToSessionJsonAsync(addRoleDto, JsonEntities.Role, originalObject, role.Id, role.Name);
                return new ServiceResponse<GetRoleDto?>(response.Message, response.StatusCode);
            }
        }
        public async Task<ApiResponse> DeleteRoleAsync(ulong id, bool saveToDb = false)
        {
            var role = await _unitOfWork.IRoleRepository.GetById(id);
            if (role == null)
                return new ApiResponse(ResponseConstants.RoleDoesNotExists, HttpStatusCode.NotFound);
            if (saveToDb)
            {
                _ = await _unitOfWork.IRoleRepository.Delete(id);
            }
            else
            {
                await _pendingChangesManager.DeleteToSessionJsonAsync(JsonEntities.Role, role.Id, role.Name);
            }
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }


        public async Task<ServiceResponse<string>> ValidateRoleNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                var badRequestResponse = new BadRequest(RoleResponseConstants.BadRequest, RoleResponseConstants.RoleNameIsNullOrEmpty);
                return new ServiceResponse<string>
                {
                    Message = badRequestResponse.Detail,
                    StatusCode = badRequestResponse.Status,
                    Data = null
                };
            }
            return new ServiceResponse<string>(ResponseConstants.Success, HttpStatusCode.OK);

        }

        public async Task<ServiceResponse<string>> ExistingRoleName(string roleName)
        {
            await _unitOfWork.BeginTransactionAsync();
            bool isRoleNameExists = await _unitOfWork.IRoleRepository.IsRoleNameExistsForCreationAsync(roleName);
            if (isRoleNameExists)
            {
                return new ServiceResponse<string>
                {
                    Message = RoleResponseConstants.RoleAlreadyExists,
                    StatusCode = HttpStatusCode.Conflict,
                    Data = null
                };
            }
            return new ServiceResponse<string>(ResponseConstants.Success, HttpStatusCode.OK);
        }

        public async Task<ServiceResponse<string>> ExistingRoleNameWhileUpdation(string roleName, ulong id)
        {
            await _unitOfWork.BeginTransactionAsync();
            bool isRoleNameExists = await _unitOfWork.IRoleRepository.IsRoleNameExistsAsync(id, roleName);
            if (isRoleNameExists)
            {
                return new ServiceResponse<string>
                {
                    Message = RoleResponseConstants.RoleAlreadyExists,
                    StatusCode = HttpStatusCode.Conflict,
                    Data = null
                };
            }
            return new ServiceResponse<string>(ResponseConstants.Success, HttpStatusCode.OK);
        }
    }
}
