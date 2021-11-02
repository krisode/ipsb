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
            var account = _accountService.CheckLogin(authAccount.Email, authAccount.Password, _ => _.Store, _ => _.Building);
            
            if (account == null)
            {
                return Unauthorized();
            }

            if(account.Role.Equals(Role.VISITOR)){
                return BadRequest();
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
            var account = await _accountService.GetByIdAsync(_ => _.Phone == authAccount.Phone && _.Password == authAccount.Password);
            if (account == null)
            {
                return Unauthorized();
            }

            if(!account.Role.Equals(Role.VISITOR)){
                return BadRequest();
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
                return Unauthorized("Invalid login, please try again!");
            }
            Account accountCreate = null;
            if (phone != null)
            {
                accountCreate = _accountService.GetAll()
                    .Where(_ => _.Phone == phone)
                    .FirstOrDefault();
                accountCreate ??= new Account()
                {
                    Phone = phone,
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
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            else if (accountCreate.Status != Status.ACTIVE)
            {
                return Unauthorized();
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
            string tokenFromReqBody = authAccount.RefreshToken;
            string tokenFromCookie = Request.Cookies[CookieConfig.REFRESH_TOKEN];
            if (tokenFromReqBody != null && tokenFromCookie != null)
            {
                return BadRequest("Refresh Token appeared in both cookie and request body!");
            }
            if (tokenFromReqBody == null && tokenFromCookie == null)
            {
                return BadRequest("Require Token in cookie or in request body!");
            }
            string refreshToken = tokenFromReqBody ?? tokenFromCookie;
            int accountId;
            try
            {
                accountId = _jwtTokenProvider.GetIdFromToken(refreshToken);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            Account accountFound = await _accountService.GetByIdAsync(_ => _.Id == accountId, _ => _.Store);
            if (accountFound.Status.Equals(Status.INACTIVE))
            {
                return Unauthorized();
            }

            var rtnAccount = await _jwtTokenProvider.GetUserAuth<AuthLoginSuccess>(accountFound, Response);
            return Ok(rtnAccount);
        }

        /// <summary>
        /// Change password of an account
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
            var updAccount = await _accountService.GetByIdAsync(_ => _.Id == authAccount.AccountId);

            if (updAccount == null)
            {
                return BadRequest();
            }

            if (updAccount.Status.Equals(Status.INACTIVE))
            {
                return Unauthorized();
            }

            try
            {
                updAccount.Id = authAccount.AccountId;
                updAccount.Password = authAccount.Password;
                updAccount.Status = Constants.Status.ACTIVE;

                _accountService.Update(updAccount);
                await _accountService.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
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
            int accountId;
            try
            {
                accountId = _jwtTokenProvider.GetIdFromToken(token);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            Account updAccount = await _accountService.GetByIdAsync(_ => _.Id == accountId);
            if (updAccount.Status.Equals(Status.INACTIVE))
            {
                return Unauthorized();
            }

            if (updAccount == null)
            {
                return BadRequest();
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
            var account = _accountService.CheckEmail(authAccount.Email);

            if (account == null)
            {
                return NotFound();
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
