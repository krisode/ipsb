using AutoMapper;
using FirebaseAdmin.Auth;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly IMapper _mapper;

        public AuthController(IAccountService accountService, IJwtTokenProvider jwtTokenProvider, IMapper mapper)
        {
            _accountService = accountService;
            _jwtTokenProvider = jwtTokenProvider;
            _mapper = mapper;
        }

        /// <summary>
        /// Check email and password of an account
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "Email" : "abcdef@gmail.com"
        ///         "Password" : "123456"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">No accounts found with the given username and password</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<ActionResult<AccountVM>> CheckLogin(AuthWebLogin authAccount)
        {
            ResponseModel responseModel = new();

            var account = _accountService.CheckLogin(authAccount.Email, authAccount.Password, _ => _.Store, _ => _.Building);
            
            if (account == null)
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            if(account.Role.Equals(Role.VISITOR)){
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(authAccount.Email) + " or " + nameof(authAccount.Password));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            var rtnAccount = await _jwtTokenProvider.GetUserAuth<AuthLoginSuccess>(account, Response);
            return Ok(rtnAccount);
        }

        /// <summary>
        /// Check phone and password of an account
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "Phone" : "0918938432"
        ///         "Password" : "123456"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">No accounts found with the given username and password</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login-phone")]
        public async Task<ActionResult<AccountVM>> LoginPhone(AuthPhoneLogin authAccount)
        {
            ResponseModel responseModel = new();

            var account = await _accountService.GetByIdAsync(_ => _.Phone == authAccount.Phone && _.Password == authAccount.Password);
            if (account == null)
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            if(!account.Role.Equals(Role.VISITOR)){
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            var rtnAccount = await _jwtTokenProvider.GetUserAuth<AuthLoginSuccess>(account, Response);
            return Ok(rtnAccount);
        }

        /// <summary>
        /// Log user out, remove the refresh token cookie
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///        
        ///     }
        /// </remarks>
        /// <returns>Remove the refresh token</returns>
        /// <response code="204">Successfully log user out, removed the refresh token cookie</response>
        /// <response code="500">Internal server errors</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("logout")]
        public ActionResult<AccountVM> Logout()
        {
            Response.Cookies.Delete(CookieConfig.REFRESH_TOKEN);
            return NoContent();
        }

        /// <summary>
        /// Check valid of user login via firebase
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "IdToken": "ddasdadad"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">Invalid login with firebase</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login-firebase")]
        public async Task<ActionResult<AccountVM>> CheckLoginFirebase(AuthFirebaseLogin authAccount)
        {
            ResponseModel responseModel = new();

            var auth = FirebaseAuth.DefaultInstance;

            string phone = null;
            string email = null;
            FirebaseToken decodedToken = null;
            try
            {
                decodedToken = await auth.VerifyIdTokenAsync(authAccount.IdToken);
                decodedToken.Claims.TryGetValue(TokenClaims.PHONE_NUMBER, out var phoneVar);
                decodedToken.Claims.TryGetValue(TokenClaims.EMAIL, out var emailvar);
                phone = (string)phoneVar;
                email = (string)emailvar;
            } 
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }
            Account accountCreate = null;
            if (phone != null)
            {
                accountCreate = _accountService.GetAll()
                    .Where(_ => _.Phone == phone)
                    .FirstOrDefault();
                accountCreate ??= new Account()
                {
                    Phone = phone.Replace("+84", "0"),
                    Status = Status.NEW,
                };
            }
            if (email != null)
            {
                accountCreate = _accountService.GetAll()
                    .Where(_ => _.Email == email)
                    .FirstOrDefault();
                decodedToken.Claims.TryGetValue(TokenClaims.PICTURE, out var picture);
                decodedToken.Claims.TryGetValue(TokenClaims.NAME, out var name);
                accountCreate ??= new Account()
                {
                    Email = email,
                    Name = (string)name,
                    ImageUrl = (string)picture,
                    Status = Status.ACTIVE
                };
            }

            if (accountCreate.Id == 0)
            {
                try
                {
                    accountCreate.Role = Constants.Role.VISITOR;
                    await _accountService.AddAsync(accountCreate);
                    await _accountService.Save();
                }
                catch (Exception)
                {
                    responseModel.Code = StatusCodes.Status500InternalServerError;
                    responseModel.Message = ResponseMessage.CAN_NOT_READ;
                    responseModel.Type = ResponseType.CAN_NOT_READ;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
                }
            }
            else if (accountCreate.Status != Status.ACTIVE || !accountCreate.Role.Equals(Role.VISITOR))
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            var rtnAccount = await _jwtTokenProvider.GetUserAuth<AuthLoginSuccess>(accountCreate, Response);
            return Ok(rtnAccount);
        }

        /// <summary>
        /// Return new access token for user if refreshToken is valid
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "RefreshToken": "sadajsdasjdasjd"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Refresh token is valid</response>
        /// <response code="401">Refresh token is invalid or expired!</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken(AuthRefreshToken authAccount)
        {
            ResponseModel responseModel = new();

            string tokenFromReqBody = authAccount.RefreshToken;
            string tokenFromCookie = Request.Cookies[CookieConfig.REFRESH_TOKEN];
            if (tokenFromReqBody != null && tokenFromCookie != null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.REFRESH_TOKEN;
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }
            if (tokenFromReqBody == null && tokenFromCookie == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.REQUIRE_TOKEN;
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }
            string refreshToken = tokenFromReqBody ?? tokenFromCookie;
            int accountId;
            try
            {
                accountId = _jwtTokenProvider.GetIdFromToken(refreshToken);
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            Account accountFound = await _accountService.GetByIdAsync(_ => _.Id == accountId, _ => _.Store);
            if (accountFound.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            var rtnAccount = await _jwtTokenProvider.GetUserAuth<AuthLoginSuccess>(accountFound, Response);
            return Ok(rtnAccount);
        }

        /// <summary>
        /// Change password of an account on web
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "AccountId": "1"
        ///         "Password" : "123456"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">No accounts found with the given username and password</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword(AuthWebChangePassword authAccount)
        {
            ResponseModel responseModel = new();
            
            var updAccount = await _accountService.GetByIdAsync(_ => _.Id == authAccount.AccountId);
            
            if (updAccount == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            
            if (!authAccount.AccountId.ToString().Equals(User.Identity.Name))
            {
                responseModel.Code = StatusCodes.Status403Forbidden;
                responseModel.Message = ResponseMessage.UNAUTHORIZE_UPDATE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Forbid(responseModel.ToString());
            }

            if (updAccount.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            try
            {
                updAccount.Id = (int) authAccount.AccountId;
                updAccount.Password = authAccount.Password;
                updAccount.Status = Constants.Status.ACTIVE;

                _accountService.Update(updAccount);
                await _accountService.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return NoContent();
        }


        /// <summary>
        /// Change password of an account on mobile
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "AccountId": "1"
        ///         "Password" : "123456"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">No accounts found with the given username and password</response>
        [HttpPut("change-password-mobile/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<ActionResult> ChangePasswordMobile(int id, AuthMobileChangePassword authAccount)
        {
            ResponseModel responseModel = new();

            if (id != authAccount.AccountId)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(authAccount.AccountId));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            var updAccount = await _accountService.GetByIdAsync(_ => _.Id == id);

            if (updAccount == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (updAccount.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            if (!updAccount.Password.Equals(authAccount.OldPassword))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(authAccount.OldPassword));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            try
            {
                updAccount.Id = authAccount.AccountId;
                updAccount.Password = authAccount.NewPassword;

                _accountService.Update(updAccount);
                await _accountService.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return NoContent();
        }


        /// <summary>
        /// Authorize token sent from request
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "token": "abc"
        ///     }
        /// </remarks>
        /// <returns>Return status whether token is valid</returns>
        /// <response code="200">Valid token</response>
        /// <response code="401">Invalid token</response>
        /// <response code="403">Invalid token</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("authorize-token")]
        public async Task<ActionResult> AuthorizeToken(string token)
        {
            ResponseModel responseModel = new();

            int accountId;
            try
            {
                accountId = _jwtTokenProvider.GetIdFromToken(token);
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            Account updAccount = await _accountService.GetByIdAsync(_ => _.Id == accountId);

            if (updAccount == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (updAccount.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status401Unauthorized;
                responseModel.Message = ResponseMessage.UNAUTHORIZE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Unauthorized(responseModel);
            }

            return Ok(updAccount);
        }

        /// <summary>
        /// Send an forgot password to an email
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "Email": "exampleemail@email.com"
        ///     }
        /// </remarks>
        /// <returns>Response after executing request</returns>
        /// <response code="200">Send email successfully</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(AuthWebForgotPassword authAccount)
        {
            ResponseModel responseModel = new();

            var account = _accountService.CheckEmail(authAccount.Email);

            if (account == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(authAccount.Email));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            var additionalClaims = _jwtTokenProvider.GetAdditionalClaims(account);

            string accessToken = await _jwtTokenProvider.GetAccessToken(additionalClaims);

            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                    "key-c8f20ea48751d2eaa564f4ee68e22914");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "sandbox063d4a6203534601a25434de0bce380b.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "IPSB Team <trantrinhdanghuy1406@gmail.com>");
            request.AddParameter("to", authAccount.Email);
            AuthResponseForgotPassword auth = new();
            auth.Url = "http://localhost:3000/change-password/" + accessToken;
            #region other accept string to parse as JSON
            /*string str = "{ 'context_name': { 'lower_bound': 'value', 'pper_bound': 'value', 'values': [ 'value1', 'valueN' ] } }";*/
            /*string str = "{ 'url': 'http://localhost:3000/change-password'}";*/
            /*JObject json = JObject.Parse(str);*/
            #endregion

            string json = JsonConvert.SerializeObject(auth);
            request.AddParameter("h:X-Mailgun-Variables", json);
            request.AddParameter("template", "forgot_password_template");
            request.AddParameter("subject", "Reset your IPSB account password");
            request.Method = Method.POST;

            client.Execute(request);

            return Ok(accessToken);
        }
    }
}
