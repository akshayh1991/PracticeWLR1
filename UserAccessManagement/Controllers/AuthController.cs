using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using UserAccessManagement.Handler;

namespace UserAccessManagement.Controllers
{
    /// <summary>
    /// This controller hold all the API's Related to 
    /// user authentication and password related features
    /// </summary>
    [Route("v1/auth")]
    [ApiController]
    [ProducesResponseType(typeof(ServerError), 500)]
    [ProducesResponseType(typeof(Unauthorized), 401)]
    [ProducesResponseType(typeof(Forbidden), 403)]
    public class AuthController : ControllerBase
    {
        private readonly IPasswordBl _passwordBL;
        private readonly IConfiguration _configuration;
        private readonly IAuthBL _authBL;
        private readonly IMediator _mediator;

        /// <summary>
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="authBL"></param>
        public AuthController(IPasswordBl passwordBL,
                              IConfiguration configuration,
                              IHttpContextAccessor httpContext,
                              IAuthBL authBL,
                              IMediator mediator)
        {
            _passwordBL = passwordBL;
            _configuration = configuration;
            _authBL = authBL;
            _mediator = mediator;
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePassword)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "ChangePassword method called with request: {@ChangePasswordDto}",
                Properties = new object[] { changePassword }
            });
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (changePassword == null)
            {
                return BadRequest(PasswordExceptionConstants.PasswordBadRequest);
            }

            ServiceResponse<string>? result = await _passwordBL.UpdatePasswordAsync(changePassword.userName, changePassword.oldPassword, changePassword.newPassword,ModelState);

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "ChangePassword method called with response: {@ChangePasswordDto}",
                Properties = new object[] { result }
            });

            if (result != null && result.StatusCode == HttpStatusCode.OK)
            {
                return NoContent();
            }


            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, result.Message));
            }
            else if (result.StatusCode == HttpStatusCode.Conflict)
            {
                return Conflict(new Conflict(ResponseConstants.Conflict, HttpStatusCode.Conflict, result.Message));
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                ServerError serverErrorResponse = new ServerError(
                ResponseConstants.InvalidRequest,
                HttpStatusCode.InternalServerError,
                result.Message
                );

                return StatusCode((int)HttpStatusCode.InternalServerError, serverErrorResponse);
            }

            return BadRequest(PasswordExceptionConstants.PasswordChangeFailed);
        }

        /// <summary>
        /// Forget Password
        /// </summary>
        /// <param name="forgetPasswordDto"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "ForgotPassword method called with request: {@ForgetPasswordDto}",
                Properties = new object[] { forgetPasswordDto }
            });
            try
            {
                ServiceResponse<GetForgetPasswordDto> response = await _passwordBL.ForgetPasswordAsync(forgetPasswordDto.userName);

                await _mediator.Send(new InfoLogCommand
                {
                    Message = "ForgotPassword method called with response: {@ForgetPasswordDto}",
                    Properties = new object[] { response }
                });

                if (response.StatusCode == HttpStatusCode.OK && response.Data?.link != null)
                {
                    return NoContent();
                }
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, response.Message));
                }

                return BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, PasswordExceptionConstants.PasswordInternalServerError);
            }
        }

        /// <summary>
        /// Reset Password Link Validation
        /// </summary>
        /// <param name="token" example="abc123def456">Hashed Token with username and password for the given email</param>
        /// <param name="email" example="john.doe@acme.com">Email ID of the user</param>
        /// <returns></returns>
        [HttpGet("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromQuery][Required] string token, [FromQuery][Required] string email)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "ResetPassword method called with token and email: {@token,@email}",
                Properties = new object[] { token, email }
            });

            string? jwtToken = null;
            if (Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeader) &&
                AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue? parsedHeader) &&
                parsedHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                jwtToken = parsedHeader.Parameter;
            }


            ServiceResponse<bool> response = await _passwordBL.GetUserNamePasswordAsync(email, token);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "ResetPassword method called with response: {@ResetPassword}",
                Properties = new object[] { response }
            });

            if (response.StatusCode == HttpStatusCode.OK && response.Data)
            {
                string? baseURL = _configuration["ResetPassword:RedirectURL"];
                string redirectURL = $"{baseURL}?token={jwtToken}";
                return Redirect(redirectURL);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, response.Message));
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest(new BadRequest(ResponseConstants.InvalidRequest, response.Message));
            }

            return BadRequest(response.Message);
        }

        /// <summary>
        /// Reset Password
        /// </summary>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, [FromHeader] string authorization)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "ResetPassword method called with ResetPassword object and authorization request: {@ResetPasswordDto,@authorization}",
                Properties = new object[] { resetPasswordDto, authorization }
            });
            if (resetPasswordDto == null)
            {
                return BadRequest(new ServiceResponse<bool>
                {
                    Message = PasswordExceptionConstants.PasswordBadRequest,
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = false
                });
            }

            if (string.IsNullOrWhiteSpace(resetPasswordDto.newPassword))
            {
                return BadRequest(new ServiceResponse<bool>
                {
                    Message = PasswordExceptionConstants.PasswordNewPasswordRequiredError,
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = false
                });
            }

            ServiceResponse<bool> response = await _passwordBL.CheckForHashedToken(authorization, resetPasswordDto.newPassword);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "ResetPassword method called with response: {@ResetPassword}",
                Properties = new object[] { response }
            });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return NoContent();
            }

            return BadRequest(new ServiceResponse<bool>
            {
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = false
            });
        }


        /// <summary>
        /// User Authentication and Token Generation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<IActionResult> LoginAsync(SecMan.Model.LoginRequest model)
        {
            await _mediator.Send(new InfoLogCommand
            {
                Message = "LoginAsync method called with request: {@LoginRequest}",
                Properties = new object[] { model }
            });


            ServiceResponse<LoginServiceResponse> res = await _authBL.LoginAsync(model);

            await _mediator.Send(new InfoLogCommand
            {
                Message = "LoginAsync method response: {@LoginResponse}",
                Properties = new object[] { res.Data! }
            });

            if (res.StatusCode == HttpStatusCode.OK && res.Data != null)
            {
                if (res.Data.SSOSessionId != null)
                {
                    List<Claim> claims = new List<Claim>
                        {
                            new Claim(ResponseHeaders.SSOSessionId, res.Data.SSOSessionId),
                            new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(res.Data.ExpiresIn).ToString("o"))
                        };

                    ClaimsIdentity ClaimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


                    AuthenticationProperties authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(res.Data.ExpiresIn)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                  new ClaimsPrincipal(ClaimsIdentity),
                                                  authProperties);
                }

                return Ok(new LoginResponse
                {
                    Token = res.Data.Token,
                    ExpiresIn = Convert.ToInt32(res.Data.ExpiresIn * 60)
                });
            }
            return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, res.Message));
        }


        /// <summary>
        /// Validate Session
        /// </summary>
        /// <param name="ssoSessionId"></param>
        /// <returns></returns>
        [HttpGet("validate-session")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [Authorize]
        public async Task<IActionResult> ValidateSessionAsync()
        {
            ClaimsPrincipal user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                string ssoSessionId = user.FindFirst(ResponseHeaders.SSOSessionId).Value;
                await _mediator.Send(new InfoLogCommand
                {
                    Message = "ValidateSessionAsync method called with request: {@SSOSessionId}",
                    Properties = new object[] { ssoSessionId }
                });

                if (string.IsNullOrWhiteSpace(ssoSessionId))
                {
                    return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.InvalidSessionId));
                }
                ServiceResponse<LoginServiceResponse> res = await _authBL.ValidateSessionAsync(ssoSessionId);
                await _mediator.Send(new InfoLogCommand
                {
                    Message = "ValidateSessionAsync method response: {@Response}",
                    Properties = new object[] { res }
                });


                if (res.StatusCode == HttpStatusCode.OK && res.Data != null)
                {
                    if (res.Data.IsPasswordExpired != true && res.Data.SSOSessionId != null)
                    {
                        List<Claim> claims = new List<Claim>
                        {
                            new Claim(ResponseHeaders.SSOSessionId, res.Data.SSOSessionId),
                            new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(res.Data.ExpiresIn).ToString("o"))
                        };

                        ClaimsIdentity ClaimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


                        AuthenticationProperties authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(res.Data.ExpiresIn)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                      new ClaimsPrincipal(ClaimsIdentity),
                                                      authProperties);
                    }
                    return Ok(new LoginResponse
                    {
                        Token = res.Data.Token,
                        ExpiresIn = Convert.ToInt32(res.Data.ExpiresIn * 60)
                    });
                }
                return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, res.Message));
            }
            return Unauthorized(new Unauthorized(nameof(Unauthorized), HttpStatusCode.Unauthorized, ResponseConstants.InvalidSessionId));
        }

        /// <summary>
        /// Logout User
        /// </summary>
        /// <param name="sessionId">The sessionId of the user to log out.</param>
        /// <returns>Returns an HTTP 204 (No Content) status code if the logout is successful, or 404 (Not Found) if the session does not exist</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            ClaimsPrincipal user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                string sessionId = user.FindFirst(ResponseHeaders.SSOSessionId) == null ? null : user.FindFirst(ResponseHeaders.SSOSessionId).Value;
                await _mediator.Send(new InfoLogCommand
                {
                    Message = "Logout method response: {@sessionId}",
                    Properties = new object[] { sessionId }
                });


                if (string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest("SessionID cannot be null or empty.");
                }

                bool result = await _authBL.ClearUserSessionAsync(sessionId);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
                }
            }
            return NoContent();

        }
    }
}
