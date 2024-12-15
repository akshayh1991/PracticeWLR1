using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.Model.Common;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using User = SecMan.Model.User;


namespace SecMan.BL
{
    public class UserBL : IUserBL, IAuthBL
    {
        private readonly IEncryptionDecryption _encryptionDecryption;
        private readonly JwtTokenOptions _jwt;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRsaKeysBL _rsaKeysBL;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPendingChangesManager _pendingChangesManager;

        public UserBL(IEncryptionDecryption encryptionDecryption,
                      IOptionsSnapshot<JwtTokenOptions> tokenOptions,
                      IUnitOfWork unitOfWork,
                      IRsaKeysBL rsaKeysBL,
                      ICurrentUserService currentUserService,
                      IPendingChangesManager pendingChangesManager)
        {
            _unitOfWork = unitOfWork;
            _rsaKeysBL = rsaKeysBL;
            _currentUserService = currentUserService;
            _pendingChangesManager = pendingChangesManager;
            _encryptionDecryption = encryptionDecryption;
            _jwt = tokenOptions.Value ?? new JwtTokenOptions();
        }

        public async Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model)
        {
            IEnumerable<Data.SQLCipher.User> usersQuery = await _unitOfWork.IUserRepository.GetAll(r => r.Roles);

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                usersQuery = usersQuery.Where(x => (x.UserName != null &&
                                                   x.UserName
                                                  .ToLower()
                                                  .Replace(" ", string.Empty)
                                                  .Contains(model.Username
                                                                 .ToLower()
                                                                 .Replace(" ", string.Empty))) ||
                                                  (x.FirstName != null &&
                                                   x.FirstName
                                                  .ToLower()
                                                  .Replace(" ", string.Empty)
                                                  .Contains(model.Username
                                                                 .ToLower()
                                                                 .Replace(" ", string.Empty))) ||
                                                  (x.LastName != null &&
                                                   x.LastName
                                                  .ToLower()
                                                  .Replace(" ", string.Empty)
                                                  .Contains(model.Username
                                                                 .ToLower()
                                                                 .Replace(" ", string.Empty))));
            }
            if (model.Role?.Count > 0)
            {
                usersQuery = usersQuery.Where(x => x.Roles.Exists(x => x.Name != null && model.Role.Contains(x.Name.ToLower().Replace(" ", string.Empty))));
            }
            if (model.Status?.Count > 0)
            {
                usersQuery = usersQuery.Where(x =>
                                             (model.Status.Contains("active") && x.IsActive) ||
                                             (model.Status.Contains("inactive") && !x.IsActive) ||
                                             (model.Status.Contains("retired") && x.Retired) ||
                                             (model.Status.Contains("locked") && x.Locked)
                                             );
            }
            if (model.IsLegacy != null)
            {
                usersQuery = usersQuery.Where(x => x.IsLegacy == model.IsLegacy);
            }

            int userCount = usersQuery.Count();

            List<Model.User> users = usersQuery
                .OrderBy(x => x.UserName).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                .Skip(model.Offset ?? 0)
                .Take(model.Limit ?? 500)
                .Select(user => new Model.User
                {
                    IsActive = user.IsActive,
                    Description = user.Description,
                    Domain = user.Domain,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Id = user.Id,
                    InactiveDate = user.InActiveDate,
                    IsLegacy = user.IsLegacy,
                    IsLocked = user.Locked,
                    IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                    IsRetired = user.Retired,
                    Language = user.Language,
                    LastLogin = user.LastLoginDate,
                    LastName = user.LastName,
                    LockedDate = user.LockedDate,
                    LockedReason = user.LockedReason,
                    PasswordExpiryDate = user.PasswordExpiryDate,
                    ResetPassword = user.ResetPassword,
                    RetiredDate = user.RetiredDate,
                    Username = user.UserName,
                    Roles = user.Roles.Select(r => new Model.RoleModel
                    {
                        Description = r.Description,
                        Id = r.Id,
                        IsLoggedOutType = r.IsLoggedOutType,
                        Name = r.Name,
                        NoOfUsers = r.Users.Count
                    }).ToList()
                }).ToList();

            return new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = userCount, Users = users });
        }


        public async Task<ServiceResponse<User>> AddUserAsync(CreateUser model, bool saveToDb = false)
        {
            if (model.Username == model.Password)
            {
                return new ServiceResponse<User>(ResponseConstants.UserNameAndPasswordAreSame, HttpStatusCode.BadRequest);
            }

            Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetUserByUsername(model.Username);
            if (user is not null)
            {
                return new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict);
            }

            List<RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(model.Roles);
            if (roles.Count != model.Roles.Count)
            {
                return new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }
            if (saveToDb)
            {
                User res = await _unitOfWork.IUserRepository.AddUserAsync(model);
                return new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, res);
            }
            else
            {
                model.Password = _encryptionDecryption.EncryptPassword(model.Password, model.IsLegacy);
                ApiResponse response = await _pendingChangesManager.AddToSessionJsonAsync(model, JsonEntities.User);
                return new(response.Message, response.StatusCode);
            }
        }

        public async Task<ServiceResponse<Model.User>> GetUserByIdAsync(ulong userId)
        {
            Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetById(userId, r => r.Roles);
            if (user is null)
                return new ServiceResponse<Model.User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);

            User newUser = new Model.User
            {
                IsActive = user.IsActive,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLegacy = user.IsLegacy,
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                IsRetired = user.Retired,
                Language = user.Language,
                LastLogin = user.LastLoginDate,
                LastName = user.LastName,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Username = user.UserName,
                Roles = user.Roles.Select(role => new Model.RoleModel
                {
                    Description = role.Description,
                    Id = role.Id,
                    IsLoggedOutType = role.IsLoggedOutType,
                    Name = role.Name,
                    NoOfUsers = role.Users.Count
                }).ToList()
            };

            return new ServiceResponse<Model.User>(ResponseConstants.Success, HttpStatusCode.OK, newUser);
        }

        public async Task<ServiceResponse<User>> UpdateUserAsync(UpdateUser model, ulong userId, bool saveToDb = false)
        {
            DateTime? passwordExpiryDate = null;
            if (model.IsPasswordExpiryEnabled != null &&
                model.IsPasswordExpiryEnabled.Value)
            {
                Data.SQLCipher.SysFeat sysfeat = await _unitOfWork.ISystemFeatureRepository.GetById(3, x => x.SysFeatProps);
                Data.SQLCipher.SysFeatProp? sysProp = sysfeat.SysFeatProps.Where(x => x.Name == SysFeatureConstants.Expiry).FirstOrDefault();
                int passwordExporyPeriod = Convert.ToInt32(sysProp.Val) > 0 ? Convert.ToInt32(sysProp.Val) : 30;
                passwordExpiryDate = DateTime.UtcNow.AddDays(passwordExporyPeriod);
            }

            Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetById(userId, x => x.Roles);

            if (user is null)
                return new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);

            if (user.Retired)
                return new ServiceResponse<User>(ResponseConstants.CantEdit, HttpStatusCode.BadRequest);

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                IEnumerable<Data.SQLCipher.User> existingUser = await _unitOfWork.IUserRepository.GetAll(x => x.UserName == model.Username && x.Id != userId);
                if (existingUser.Any())
                {
                    return new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict);
                }
            }

            if ((!string.IsNullOrWhiteSpace(model.Username) && model.Username == model.Password) || user.UserName == model.Password)
            {
                return new ServiceResponse<User>(ResponseConstants.UserNameAndPasswordAreSame, HttpStatusCode.BadRequest);
            }


            List<RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(model.Roles);
            if (model.Roles != null && roles.Count != model.Roles?.Count)
            {
                return new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }

            if (model.Password != null)
            {
                Tuple<UserDetails?, List<AppPermissions>?> userPermissions = await _unitOfWork.IUserRepository.GetUserDetails(_currentUserService.UserId);
                if (!userPermissions.Item2.SelectMany(x => x.Permissions ?? Enumerable.Empty<Permission>())
                    .Any(x => x.Name == "CAN_EDIT_ALL_PASSWORDS"))
                {
                    return new ServiceResponse<User>(ResponseConstants.InvalidPermissions, HttpStatusCode.Forbidden);
                }
            }

            if (model.IsLegacy != null && model.IsLegacy == false && user.IsLegacy)
            {
                var password = _encryptionDecryption.DecryptPasswordAES256(user.Password);
                model.Password = password;
            }

            if (!saveToDb)
            {
                if (model.Password != null)
                {
                    var recentPasswords = await _unitOfWork.IPasswordRepository.GetRecentPasswordsWithHistoryCountAsync(userId);
                    foreach (var password in recentPasswords.Item1)
                    {
                        if (_encryptionDecryption.VerifyPassword(model.Password, password, user.IsLegacy))
                            return new(PasswordExceptionConstants.DynamicPasswordPreviousPasswordsError.Replace("{@Count}", recentPasswords.Item2), HttpStatusCode.BadRequest);
                    }
                    model.Password = _encryptionDecryption.EncryptPassword(model.Password, model.IsLegacy ?? user.IsLegacy);
                }
                UpdateUser originalObject = new UpdateUser
                {
                    IsActive = user.IsActive,
                    Description = user.Description,
                    Domain = user.Domain,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    IsLegacy = user.IsLegacy,
                    IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                    Language = user.Language,
                    LastName = user.LastName,
                    ResetPassword = user.ResetPassword,
                    Roles = user.Roles.Select(x => x.Id).ToList(),
                    Username = user.UserName,
                    Password = user.Password,
                    FirstLogin = user.FirstLogin
                };
                var res = await _pendingChangesManager.UpdateToSessionJsonAsync(model, JsonEntities.User, originalObject, user.Id, user.UserName);
                return new ServiceResponse<User>(res.Message, res.StatusCode);
            }
            else
            {
                User res = await _unitOfWork.IUserRepository.UpdateUserAsync(model, userId, passwordExpiryDate);
                return new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, res);
            }
        }



        public async Task<ApiResponse> DeleteUserAsync(ulong userId, bool saveToDb = false)
        {
            var user = await _unitOfWork.IUserRepository.GetById(userId);
            if (user == null)
                return new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);
            if (saveToDb)
            {
                _ = await _unitOfWork.IUserRepository.Delete(userId);
            }
            else
            {
                await _pendingChangesManager.DeleteToSessionJsonAsync(JsonEntities.User, user.Id, user.UserName);
            }
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }


        public async Task<ServiceResponse<LoginServiceResponse>> LoginAsync(LoginRequest model)
        {
            Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetUserByUsername(model.Username);
            if (user is null)
            {
                return new(ResponseConstants.UserDoesNotExists, HttpStatusCode.Unauthorized);
            }

            ServiceResponse<LoginServiceResponse>? userStatus = CheckUserStatus(user);

            if (userStatus != default)
            {
                return userStatus;
            }

            bool isPasswordVerified = false;
            if (user.FirstLogin)
            {
                if (VerifyUserPassword(user, model.Password))
                {
                    isPasswordVerified = true;
                }
                if (!isPasswordVerified)
                {
                    user.IsLegacy = !user.IsLegacy;
                    isPasswordVerified = VerifyUserPassword(user, model.Password);
                }
            }
            else
            {
                isPasswordVerified = VerifyUserPassword(user, model.Password);
            }

            if (isPasswordVerified)
            {
                Tuple<UserDetails?, List<AppPermissions>?> userDetails = await _unitOfWork.IUserRepository.GetUserDetails(user.Id);

                string ssoSessionId = RandomStringGenerator.Generate(12);
                string token = GenerateAccessToken(userDetails.Item1, userDetails.Item2);

                await _unitOfWork.IUserRepository.UpdateUserSessionDetails(user.Id, ssoSessionId, _jwt.TokenExpireTime);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.IEventLogRepository.LogLoginAttempts(user.Id, true);
                return new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, new LoginServiceResponse
                {
                    Token = token,
                    SSOSessionId = ssoSessionId,
                    ExpiresIn = _jwt.TokenExpireTime
                });
            }

            await _unitOfWork.IEventLogRepository.LogLoginAttempts(user.Id, false);
            return new(ResponseConstants.InvalidPassword, HttpStatusCode.Unauthorized);
        }


        private static ServiceResponse<LoginServiceResponse>? CheckUserStatus(SecMan.Data.SQLCipher.User user)
        {
            if (user.Retired)
            {
                return new(ResponseConstants.AccountRetired, HttpStatusCode.Unauthorized);
            }
            if (user.Locked)
            {
                return new(ResponseConstants.AccountLocked, HttpStatusCode.Unauthorized);
            }
            if (!user.IsActive)
            {
                return new(ResponseConstants.AccountInActive, HttpStatusCode.Unauthorized);
            }
            if (user.PasswordExpiryEnable && user.PasswordExpiryDate < DateTime.UtcNow)
            {
                return new(ResponseConstants.PasswordExpired, HttpStatusCode.Unauthorized);
            }
            return default;
        }



        private bool VerifyUserPassword(SecMan.Data.SQLCipher.User user, string password)
        {
            string? encryptedPassword = user.Password;

            if (!string.IsNullOrWhiteSpace(encryptedPassword))
            {
                if (user.IsLegacy)
                {
                    string? decryptedPassword = _encryptionDecryption.DecryptPasswordAES256(encryptedPassword);
                    return decryptedPassword == password;
                }
                else
                {
                    return _encryptionDecryption.VerifyHashPassword(encryptedPassword, password);
                }
            }
            return false;
        }


        private string GenerateAccessToken(UserDetails? userDetails, List<AppPermissions>? appPermissions)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            System.Security.Cryptography.RSA rsa = _rsaKeysBL.GetPrivateKey();

            Claim[] claims = new[]
            {
                new Claim("userAttributes", JsonConvert.SerializeObject(userDetails), JsonClaimValueTypes.Json),
                new Claim("apps", JsonConvert.SerializeObject(appPermissions), JsonClaimValueTypes.Json),
                new Claim("sub", userDetails?.Username ?? string.Empty),
            };

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwt.TokenExpireTime),
                Issuer = _jwt.ValidIssuer,
                Audience = _jwt.ValidAudience,
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa)
                {
                    KeyId = "Primary"
                }, SecurityAlgorithms.RsaSha256)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<ServiceResponse<LoginServiceResponse>> ValidateSessionAsync(string ssoSessionId)
        {
            ulong userId = await _unitOfWork.IUserRepository.GetUserBySessionId(ssoSessionId);
            if (userId == 0)
            {
                return new(ResponseConstants.InvalidSessionId, HttpStatusCode.Unauthorized);
            }

            Tuple<UserDetails?, List<AppPermissions>?> userDetails = await _unitOfWork.IUserRepository.GetUserDetails(userId);

            string token = GenerateAccessToken(userDetails.Item1, userDetails.Item2);
            if (userDetails.Item1 != null)
            {
                await _unitOfWork.IUserRepository.UpdateUserSessionDetails(userId, ssoSessionId, _jwt.TokenExpireTime);
            }
            return new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, new LoginServiceResponse
            {
                Token = token,
                SSOSessionId = ssoSessionId,
                ExpiresIn = _jwt.TokenExpireTime,
                IsPasswordExpired = userDetails.Item1?.IsExpired
            });
        }

        public async Task<ApiResponse> RetireUserAsync(ulong userId, bool saveToDb = false)
        {
            Data.SQLCipher.User user = await _unitOfWork.IUserRepository.GetById(userId, x => x.Roles);

            if (user == null)
                return new(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);
            if (user.Retired)
                return new(ResponseConstants.UserAlreadyRetired, HttpStatusCode.Conflict);
            if (saveToDb)
            {
                await _unitOfWork.IUserRepository.RetireUserAsync(user);
            }
            else
            {
                await _pendingChangesManager.RetireToSessionJsonAsync(JsonEntities.User, user.Id, user.UserName);
            }
            return new(ResponseConstants.Success, HttpStatusCode.OK);
        }

        /// <summary>
        /// Removes the session associated with the specified sessionID.
        /// </summary>
        /// <param name="sessionId">The sessionId of the user whose session is to be removed.</param>
        /// <returns>
        /// A Task representing the asynchronous operation. Returns true if the session was successfully removed; otherwise, false.
        /// </returns>
        public async Task<bool> ClearUserSessionAsync(string sessionId)
        {
            await _unitOfWork.BeginTransactionAsync();
            bool isSessionremoved = false;
            ulong userId = await _unitOfWork.IUserRepository.GetUserBySessionId(sessionId);
            isSessionremoved = await _unitOfWork.IUserRepository.ClearUserSessionAsync(sessionId);
            await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, true, EventSubType.Logout);
            await _unitOfWork.CommitTransactionAsync();
            Log.Information("Session removal for user: {SessionID} was {Issessionremoved}", sessionId, isSessionremoved ? "successful" : "unsuccessful");
            return isSessionremoved;
        }

        public async Task<ApiResponse> UpdateUserLanguageAsync(string language)
        {
            await _unitOfWork.BeginTransactionAsync();
            UserDetails userDetail = _currentUserService.UserDetails;
            Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetById(userDetail.Id);
            if (user is null)
                return new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);
            await _unitOfWork.IUserRepository.UpdateUserLanguageAsync(userDetail.Id, language);
            await _unitOfWork.CommitTransactionAsync();
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }
        public async Task<ApiResponse> UnlockUserAsync(ulong userId, bool changePasswordOnLogin, bool saveToDb = false)
        {
            Data.SQLCipher.User user = await _unitOfWork.IUserRepository.GetById(userId);

            if (user == null)
            {
                return new(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);
            }
            if (saveToDb)
            {
                await _unitOfWork.IUserRepository.UnlockUserAsync(user, changePasswordOnLogin);
            }
            else
            {
                await _pendingChangesManager.UnlockToSessionJsonAsync(JsonEntities.User, user.Id, user.UserName, changePasswordOnLogin);
            }

            return new(ResponseConstants.Success, HttpStatusCode.OK);
        }
    }
}
