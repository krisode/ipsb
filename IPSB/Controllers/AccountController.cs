using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/accounts")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Account> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;

        public AccountController(IAccountService service, IMapper mapper, IPagingSupport<Account> pagingSupport, IUploadFileService uploadFileService, IAuthorizationService authorizationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
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
        public async Task<ActionResult<AccountVM>> GetAccountById(int id)
        {
            ResponseModel responseModel = new();

            var account = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Store, _ => _.Building);

            if (account == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            // resouce-based imperative authorization
            var authorizedResult = await _authorizationService.AuthorizeAsync(User, account, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                responseModel.Code = StatusCodes.Status403Forbidden;
                responseModel.Message = ResponseMessage.UNAUTHORIZE_READ;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Forbid(responseModel.ToString());
            }

            var rtnAccount = _mapper.Map<AccountVM>(account);

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
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[Authorize(Policy = Policies.QUERY_ACCOUNT)]
        public ActionResult<IEnumerable<AccountVM>> GetAllAccounts([FromQuery] AccountSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {

            ResponseModel responseModel = new();

            var list = _service.GetAll(_ => _.Store, _ => _.Building);

            if (model.NotManageBuilding)
            {
                list = list.Where(_ => _.Building == null);
            }

            if (model.NotManageStore)
            {
                list = list.Where(_ => _.Store == null);
            }

            if (model.BuildingId > 0)
            {
                list = list.Where(_ => _.StoreOwnerBuildingId == model.BuildingId);
            }

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
                if (model.Status != Status.ACTIVE && model.Status != Status.INACTIVE && model.Status != Status.NEW)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
                else
                {
                    if (model.NotManageStore || model.NotManageBuilding)
                    {
                        list = list.Where(_ => _.Status != Status.INACTIVE);
                    }
                    else
                    {
                        list = list.Where(_ => _.Status == model.Status);
                    }

                }
            }


            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<AccountVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count accounts
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
        /// <returns>Number of accounts</returns>
        /// <response code="200">Returns number of accounts</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[Authorize(Policy = Policies.QUERY_ACCOUNT)]
        public ActionResult<IEnumerable<AccountVM>> CountAccounts([FromQuery] AccountSM model)
        {
            ResponseModel responseModel = new();

            var list = _service.GetAll(_ => _.Store, _ => _.Building);

            if (model.NotManageBuilding)
            {
                list = list.Where(_ => _.Building == null);
            }

            if (model.NotManageStore)
            {
                list = list.Where(_ => _.Store == null);
            }

            if (model.BuildingId > 0)
            {
                list = list.Where(_ => _.StoreOwnerBuildingId == model.BuildingId);
            }

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
                if (model.Status != Status.ACTIVE && model.Status != Status.INACTIVE && model.Status != Status.NEW)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
                else
                {
                    list = list.Where(_ => _.Status == model.Status);
                }
            }

            return Ok(list.Count());
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
            ResponseModel responseModel = new();
            Account account = null;
            account = await _service.GetByIdAsync(_ => _.Phone == model.Phone);
            if (account is not null)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Phone);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }


            account = await _service.GetByIdAsync(_ => _.Email == model.Email);
            if (account is not null)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Email);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            //if (!string.IsNullOrEmpty(model.Status))
            //{
            //    if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
            //    {
            //        return BadRequest();
            //    }
            //}

            if (!Role.BUILDING_MANAGER.Equals(model.Role) && !Role.STORE_OWNER.Equals(model.Role))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Role));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            if (Role.STORE_OWNER != model.Role && model.StoreOwnerBuildingId != null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Role));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            if (Role.STORE_OWNER == model.Role && model.StoreOwnerBuildingId == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Role));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            Account crtAccount = _mapper.Map<Account>(model);


            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "account", "account-profile");
            crtAccount.ImageUrl = imageURL;

            // Default POST Status = "NEW"
            crtAccount.Status = Status.NEW;
            crtAccount.Password = DefaultValue.PASSWORD;

            try
            {
                await _service.AddAsync(crtAccount);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
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
            ResponseModel responseModel = new();

            Account account = null;
            account = await _service.GetByIdAsync(_ => _.Phone == model.Phone && _.Id != id);
            if (account is not null)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Phone);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            Account updAccount = await _service.GetByIdAsync(_ => _.Id == id);

            if (updAccount is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updAccount, Operations.Update);

            if (!authorizedResult.Succeeded)
            {
                responseModel.Code = StatusCodes.Status403Forbidden;
                responseModel.Message = ResponseMessage.UNAUTHORIZE_UPDATE;
                responseModel.Type = ResponseType.UNAUTHORIZE;
                return Forbid(responseModel.ToString());
            }

            string imageURL = updAccount.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "account", "account-profile");
            }

            try
            {
                if (model.FirstUpdateProfile && updAccount.Status.Equals(Status.NEW))
                {
                    updAccount.Status = Status.ACTIVE;
                    updAccount.Password = model.Password;
                }
                updAccount.Name = model.Name;
                updAccount.ImageUrl = imageURL;
                updAccount.Phone = model.Phone;

                _service.Update(updAccount);
                await _service.Save();
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
        /// Delete account with specified id
        /// </summary>
        /// <param name="id">Account's id</param>
        /// <response code="204">Delete account successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            var deleteEntity = await _service.GetByIdAsync(_ => _.Id == id);

            if (deleteEntity is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (deleteEntity.Role.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(Account));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            try
            {
                deleteEntity.Status = Status.INACTIVE;
                _service.Update(deleteEntity);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_DELETE;
                responseModel.Type = ResponseType.CAN_NOT_DELETE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            return NoContent();
        }

    }
}
