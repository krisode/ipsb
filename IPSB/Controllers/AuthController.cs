using AutoMapper;
using FirebaseAdmin.Auth;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            var account = _accountService.CheckLogin(authAccount.Email, authAccount.Password);

            if (account == null)
            {
                return Unauthorized();
            }

            var rtnAccount = _mapper.Map<AuthLoginSuccess>(account);

            // Claims for generating JWT
            var additionalClaims = _jwtTokenProvider.GetAdditionalClaims(account);
           
            string accessToken = await _jwtTokenProvider.GetAccessToken(additionalClaims);
           
            string refreshToken = await _jwtTokenProvider.GetRefreshToken(additionalClaims);

            rtnAccount.AccessToken = accessToken;
            rtnAccount.RefreshToken = refreshToken;

            return Ok(rtnAccount);
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
            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;

            string phone = null;
            string email = null;
            FirebaseToken decodedToken = null; 
            try
            {
                decodedToken = await auth.VerifyIdTokenAsync(authAccount.IdToken);
                phone = decodedToken.Claims[TokenClaims.PHONE_NUMBER].ToString();
                email = decodedToken.Claims[TokenClaims.EMAIL].ToString();
            }
            catch (Exception e)
            {
                return Unauthorized("Invalid login, please try again!");
            }
            Account accountCreate = null;
            if(phone != null)
            {
                accountCreate = _accountService.GetAll()
                    .Where(_ => _.Phone == phone)
                    .First();
                accountCreate ??= new Account() { Phone = phone };
            }
            if(email != null)
            {
                accountCreate = _accountService.GetAll().Where(_ => _.Email == email).First();
                string picture = decodedToken.Claims[TokenClaims.PICTURE].ToString();
                string name = decodedToken.Claims[TokenClaims.NAME].ToString();
                accountCreate ??= new Account() { Email = email, Name = name, ImageUrl = picture };
            }
            
            if(accountCreate.Id == 0)
            {
                try
                {
                    accountCreate.Role = Constants.Role.VISITOR;
                    accountCreate.Status = Constants.Status.ACTIVE;
                    await _accountService.AddAsync(accountCreate);
                    await _accountService.Save();
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }                
            }

            var rtnAccount = _mapper.Map<AuthLoginSuccess>(accountCreate);

            var additionalClaims = _jwtTokenProvider.GetAdditionalClaims(accountCreate);
            string accessToken = await _jwtTokenProvider.GetAccessToken(additionalClaims);
            string refreshToken = await _jwtTokenProvider.GetRefreshToken(additionalClaims);

            rtnAccount.AccessToken = accessToken;
            rtnAccount.RefreshToken = refreshToken;
            
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
            int accountId;
            try
            {
                accountId = _jwtTokenProvider.GetIdFromToken(authAccount.RefreshToken);
            }
            catch (Exception)
            {
                return Unauthorized();
            }

            Account accountFound = await _accountService.GetByIdAsync(_ => _.Id == accountId);
            if (accountFound.Status.Equals(Status.INACTIVE))
            {
                return Unauthorized();
            }

            var rtnAccount = _mapper.Map<AuthLoginSuccess>(accountFound);

            var additionalClaims = _jwtTokenProvider.GetAdditionalClaims(accountFound);
            string accessToken = await _jwtTokenProvider.GetAccessToken(additionalClaims);
            string refreshToken = await _jwtTokenProvider.GetRefreshToken(additionalClaims);

            rtnAccount.AccessToken = accessToken;
            rtnAccount.RefreshToken = refreshToken;
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
    }

}
