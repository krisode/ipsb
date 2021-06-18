﻿using AutoMapper;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
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
    [Route("api/v1.0/accounts")]
    [ApiController]
    public class AccountController : AuthorizeController
    {
        private readonly IAccountService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Account> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public AccountController(IAccountService service, IMapper mapper, IPagingSupport<Account> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
        }


        /// <summary>
        /// Get a specific account by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the account with the corresponding id</returns>
        /// <response code="200">Returns the account with the specified id</response>
        /// <response code="404">No accounts found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<AccountVM> GetAccountById(int id)
        {
            var account = _service.GetByIdAsync(_ => _.Id == id, _ => _.BuildingAdmins, _ => _.BuildingManagers, 
                _ => _.CouponInUses, _ => _.Stores, _ => _.VisitRoutes);

            if (account == null)
            {
                return NotFound();
            }

            var rtnAccount = _mapper.Map<AccountVM>(account.Result);

            return Ok(rtnAccount);
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         
        ///     }
        ///
        /// </remarks>
        /// <returns>All accounts</returns>
        /// <response code="200">Returns all accounts</response>
        /// <response code="404">No accounts found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<AccountVM>> GetAllAccounts([FromQuery] AccountSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Account> list = _service.GetAll(_ => _.BuildingAdmins, _ => _.BuildingManagers,
                _ => _.CouponInUses, _ => _.Stores, _ => _.VisitRoutes);

            if (!string.IsNullOrEmpty(model.Role))
            {
                list = list.Where(_ => _.Role.ToUpper() == model.Role.ToUpper());
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }
            
            if (!string.IsNullOrEmpty(model.Phone))
            {
                list = list.Where(_ => _.Phone.Contains(model.Phone));
            }
            
            if (!string.IsNullOrEmpty(model.Email))
            {
                list = list.Where(_ => _.Email.Contains(model.Email));
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }

                else
                {
                    if (model.Status == Constants.Status.ACTIVE)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.ACTIVE);
                    }

                    if (model.Status == Constants.Status.INACTIVE)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.INACTIVE);
                    }
                }
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<AccountVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Email": "Email of the account",   
        ///         "Name": "Name of the account",   
        ///         "Phone": "Account phone number",   
        ///         "Image": "Profile picture of the account",   
        ///         "Role": "Account role", 
        ///         "Status": "Status of the account",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new account</response>
        /// <response code="409">Account already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AccountCM>> CreateAccount([FromForm] AccountCM model)
        {
            Account account = _service.GetByIdAsync(_ => _.Email == model.Email).Result;
            if (account is not null)
            {
                return Conflict();
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!Constants.Role.ROLE_LIST.Contains(model.Role))
                {
                    return BadRequest();
                }
            }

            Account crtAccount = _mapper.Map<Account>(model);

            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "account", "account-profile");
            crtAccount.ImageUrl = imageURL;

            try
            {
                await _service.AddAsync(crtAccount);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetAccountById", new { id = crtAccount.Id }, crtAccount);
        }

        /// <summary>
        /// Update account with specified id
        /// </summary>
        /// <param name="id">Account's id</param>
        /// <param name="model">Information applied to updated account</param>
        /// <response code="204">Update account successfully</response>
        /// <response code="400">Account's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Account already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutAccount(int id, [FromForm] AccountUM model)
        {

            Account updAccount = await _service.GetByIdAsync(_ => _.Id == id);

            if (updAccount == null || id != model.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!Constants.Role.ROLE_LIST.Contains(model.Role))
                {
                    return BadRequest();
                }
            }
            
            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            string imageURL = updAccount.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "account", "account-profile");
            }

            try
            {
                updAccount.Id = model.Id;
                updAccount.Role = model.Role;
                updAccount.Name = model.Name;
                updAccount.ImageUrl = imageURL;
                updAccount.Phone = model.Phone;
                updAccount.Status = model.Status;

                _service.Update(updAccount);
                await _service.Save();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }

        // DELETE api/<ProductCategoryController>/5
        // Change Status to Inactive
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }

        protected override bool IsAuthorize()
        {
            throw new NotImplementedException();
        }
    }
}