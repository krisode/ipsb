using AutoMapper;
using IPSB.Core.Services;
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
        private readonly IMapper _mapper;

        public AuthController(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        /// <summary>
        /// Check username and password of an account
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST {
        ///         "Email" : "1"
        ///         "Password" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Account exists in the system</response>
        /// <response code="401">No accounts found with the given username and password</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public ActionResult<AccountVM> CheckLogin(AuthWebLogin authAccount)
        {
            var account = _accountService.CheckLogin(authAccount.Email, authAccount.Password);

            if (account == null)
            {
                return Unauthorized();
            }

            var rtnAccount = _mapper.Map<AccountVM>(account);

            return Ok(rtnAccount);
        }


    }
}
