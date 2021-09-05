using AutoMapper;
using IPSB.Core.Services;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
