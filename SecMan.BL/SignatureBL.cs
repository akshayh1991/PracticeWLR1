using MediatR;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.Net;

namespace SecMan.BL
{
    public class SignatureBL : ISignatureBL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryption;
        private readonly ICurrentUserService _currentUserServices;

        public SignatureBL(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryption, ICurrentUserService currentUserServices)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryption = encryptionDecryption;
            _currentUserServices = currentUserServices;
        }
        public async Task<ApiResponse> VerifySignatureAsync(string password, string note)
        {
            await _unitOfWork.BeginTransactionAsync();
            var details = _currentUserServices.UserDetails;
            var userCredentials = await _unitOfWork.ISignatureRepository.GetUserCredentials(details.Username.Trim());

            if (userCredentials == null)
            {
                return new ApiResponse(ResponseConstants.InvalidUsername, HttpStatusCode.BadRequest);
            }

            var parts = userCredentials.Password.Split('$');
            if (parts.Length < 2)
            {
                return new ApiResponse(ResponseConstants.InvalidPasswordFormat, HttpStatusCode.BadRequest);
            }

            bool isCheckPassword;

            isCheckPassword = parts[1] switch
            {
                "2" => _encryptionDecryption.VerifyHashPassword(userCredentials.Password, password),
                "1" =>  VerifyAESPassword(userCredentials, password),
                _ => throw new NotSupportedException(ResponseConstants.UnsupportedPasswordFormat)
            };

            if (isCheckPassword)
            {
                await _unitOfWork.ISignatureRepository.SignatureVerifyAsync(details.Id, note);
                await _unitOfWork.CommitTransactionAsync();
                return new ApiResponse(ResponseConstants.SignatureVerified, HttpStatusCode.OK);

            }
            return new ApiResponse(ResponseConstants.InvalidRequest, HttpStatusCode.BadRequest);
        }
        public async Task<ApiResponse> SignatureAuthorizeAsync(Authorize request)
        {
            await _unitOfWork.BeginTransactionAsync();
            var details = _currentUserServices.UserDetails;
            if (request.UserName==details.Username)
            {
                return new ApiResponse(ResponseConstants.UserDetailNotFound, HttpStatusCode.Forbidden);
            }
            if (details == null)
            {
                return new ApiResponse(ResponseConstants.UserDetailNotFound, HttpStatusCode.BadRequest);
            }

            if (request.IsSigned)
            {
                var userCredentials = await _unitOfWork.ISignatureRepository.GetUserCredentials(details.Username.Trim());
                if (userCredentials == null)
                {
                    return new ApiResponse(ResponseConstants.InvalidUsername, HttpStatusCode.BadRequest);
                }
            }

            Data.SQLCipher.User? authorizeUser = null;

            if (request.IsAuthorize)
            {
                authorizeUser = await _unitOfWork.IUserRepository.GetUserByUsername(request.UserName);
                if (authorizeUser == null)
                    return new(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound);

                var authorizeUserDetails = await _unitOfWork.IUserRepository.GetUserDetails(authorizeUser.Id);

                if(!authorizeUserDetails.Item2.SelectMany(x => x.Permissions).Any(x => x.Name == "CAN_AUTHORIZE"))
                {
                    return new(ResponseConstants.InvalidPermissions, HttpStatusCode.Forbidden);
                }

                var userCredentials = await _unitOfWork.ISignatureRepository.GetUserCredentials(request.UserName.Trim());
                if (userCredentials == null)
                {
                    return new ApiResponse(ResponseConstants.InvalidUsername, HttpStatusCode.BadRequest);
                }

                var parts = userCredentials.Password.Split('$');
                if (parts.Length < 2)
                {
                    return new ApiResponse(ResponseConstants.InvalidPasswordFormat, HttpStatusCode.BadRequest);
                }

                bool isCheckPassword;
                isCheckPassword = parts[1] switch
                {
                    "2" => _encryptionDecryption.VerifyHashPassword(userCredentials.Password, request.Password),
                    "1" => VerifyAESPassword(userCredentials, request.Password),
                    _ => throw new NotSupportedException(ResponseConstants.UnsupportedPasswordFormat)
                };

                if (!isCheckPassword)
                {
                    return new ApiResponse(ResponseConstants.InvalidPassword, HttpStatusCode.BadRequest);
                }
            }

            await _unitOfWork.ISignatureRepository.SignatureAuthorizeAsync(details.Id, authorizeUser.Id, request);
            await _unitOfWork.CommitTransactionAsync();
            return new ApiResponse(ResponseConstants.SignatureVerified, HttpStatusCode.OK);
        }

        private bool VerifyAESPassword(GetUserCredentialsDto userCredentials, string password)
        {
            return _encryptionDecryption.DecryptPasswordAES256(userCredentials.Password) == password;
        }
    }
}
