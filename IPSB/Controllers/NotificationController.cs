using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.ExternalServices;
using IPSB.Cache;
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
    [Route("api/v1.0/notifications")]
    [ApiController]
    /*[Authorize(Roles = "Admin, Building Manager")]*/
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Notification> _pagingSupport;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStore _cacheStore;
        public NotificationController(INotificationService service, IMapper mapper, IPagingSupport<Notification> pagingSupport,
            IAuthorizationService authorizationService, ICacheStore cacheStore)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _authorizationService = authorizationService;
            _cacheStore = cacheStore;
        }

        /// <summary>
        /// Get a specific notification by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the notification with the corresponding id</returns>
        /// <response code="200">Returns the notification with the specified id</response>
        /// <response code="404">No notifications found with the specified id</response>
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationVM>> GetNotificationById(int id)
        {
            ResponseModel responseModel = new();
            var cacheId = new CacheKey<Notification>(id);
            var cacheObjectType = new Notification();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var notification = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var notification = _service.GetByIdAsync(_ => _.Id == id).Result;

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(notification);

                }, ifModifiedSince);


                if (notification == null)
                {
                    responseModel.Code = StatusCodes.Status404NotFound;
                    responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Notification));
                    responseModel.Type = ResponseType.NOT_FOUND;
                    return NotFound(responseModel);
                }

                /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Read);
                if (!authorizedResult.Succeeded)
                {
                    return new ObjectResult($"Not authorize to access building with id: {id}") { StatusCode = 403 };
                }*/

                var rtnNotification = _mapper.Map<NotificationVM>(notification);

                return Ok(rtnNotification);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    responseModel.Code = StatusCodes.Status304NotModified;
                    responseModel.Message = ResponseMessage.NOT_MODIFIED;
                    responseModel.Type = ResponseType.NOT_MODIFIED;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status304NotModified };
                }
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_READ;
                responseModel.Type = ResponseType.CAN_NOT_READ;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        /// <summary>
        /// Get all notifications
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
        /// <returns>All notifications</returns>
        /// <response code="200">Returns all notifications</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationVM>>> GetAllNotifications([FromQuery] NotificationSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Notification>(Constants.DefaultValue.INTEGER);
            var cacheObjectType = new Notification();
            string ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll();

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                {
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                    return cachedTime;
                }, ifModifiedSince);


                if (!string.IsNullOrEmpty(model.Status))
                {
                    if (model.Status != Status.READ && model.Status != Status.UNREAD)
                    {
                        responseModel.Code = StatusCodes.Status400BadRequest;
                        responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                        responseModel.Type = ResponseType.INVALID_REQUEST;
                        return BadRequest(responseModel);
                    }

                    else
                    {
                        if (model.Status == Status.READ)
                        {
                            list = list.Where(_ => _.Status == Status.READ);
                        }

                        if (model.Status == Status.UNREAD)
                        {
                            list = list.Where(_ => _.Status == Status.UNREAD);
                        }
                    }
                }

                if (model.AccountId >= 0)
                {
                    list = list.Where(_ => _.AccountId == model.AccountId);
                }

                if (!string.IsNullOrEmpty(model.Title))
                {
                    list = list.Where(_ => _.Title.Contains(model.Title));
                }

                if (!string.IsNullOrEmpty(model.Body))
                {
                    list = list.Where(_ => _.Body.Contains(model.Body));
                }

                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    list = list.Where(_ => _.ImageUrl.Contains(model.ImageUrl));
                }

                if (!string.IsNullOrEmpty(model.Screen))
                {
                    list = list.Where(_ => _.Screen.Contains(model.Screen));
                }

                if (!string.IsNullOrEmpty(model.Parameter))
                {
                    list = list.Where(_ => _.Parameter.Contains(model.Parameter));
                }

                if (model.LowerDate.HasValue)
                {
                    list = list.Where(_ => _.Date >= model.LowerDate);
                }

                if (model.UpperDate.HasValue)
                {
                    list = list.Where(_ => _.Date <= model.UpperDate);
                }

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                    .Paginate<NotificationVM>();

                return Ok(pagedModel);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    responseModel.Code = StatusCodes.Status304NotModified;
                    responseModel.Message = ResponseMessage.NOT_MODIFIED;
                    responseModel.Type = ResponseType.NOT_MODIFIED;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status304NotModified };
                }
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_READ;
                responseModel.Type = ResponseType.CAN_NOT_READ;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }
        
        /// <summary>
        /// Count notifications
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
        /// <returns>All notifications</returns>
        /// <response code="200">Returns all notifications</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<int>>> CountNotification([FromQuery] NotificationSM model)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Notification>(DefaultValue.INTEGER);
            var cacheObjectType = new Notification();
            string ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll();

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                {
                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                    return cachedTime;
                }, ifModifiedSince);


                if (!string.IsNullOrEmpty(model.Status))
                {
                    if (model.Status != Status.READ && model.Status != Status.UNREAD)
                    {
                        responseModel.Code = StatusCodes.Status400BadRequest;
                        responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                        responseModel.Type = ResponseType.INVALID_REQUEST;
                        return BadRequest(responseModel);
                    }

                    else
                    {
                        if (model.Status == Status.READ)
                        {
                            list = list.Where(_ => _.Status == Status.READ);
                        }

                        if (model.Status == Status.UNREAD)
                        {
                            list = list.Where(_ => _.Status == Status.UNREAD);
                        }
                    }
                }

                if (model.AccountId >= 0)
                {
                    list = list.Where(_ => _.AccountId == model.AccountId);
                }

                if (!string.IsNullOrEmpty(model.Title))
                {
                    list = list.Where(_ => _.Title.Contains(model.Title));
                }

                if (!string.IsNullOrEmpty(model.Body))
                {
                    list = list.Where(_ => _.Body.Contains(model.Body));
                }

                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    list = list.Where(_ => _.ImageUrl.Contains(model.ImageUrl));
                }

                if (!string.IsNullOrEmpty(model.Screen))
                {
                    list = list.Where(_ => _.Screen.Contains(model.Screen));
                }

                if (!string.IsNullOrEmpty(model.Parameter))
                {
                    list = list.Where(_ => _.Parameter.Contains(model.Parameter));
                }

                if (model.LowerDate.HasValue)
                {
                    list = list.Where(_ => _.Date >= model.LowerDate);
                }

                if (model.UpperDate.HasValue)
                {
                    list = list.Where(_ => _.Date <= model.UpperDate);
                }

                return Ok(list.Count());
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    responseModel.Code = StatusCodes.Status304NotModified;
                    responseModel.Message = ResponseMessage.NOT_MODIFIED;
                    responseModel.Type = ResponseType.NOT_MODIFIED;
                    return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status304NotModified };
                }
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_READ;
                responseModel.Type = ResponseType.CAN_NOT_READ;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Content": "Content of notification",   
        ///         "Screen": "Screen used to navigate to when the user clicks on the notification",   
        ///         "Parameter": "Parameters passed to the screen used for navigation",   
        ///         "AccountId": "Id of account received notification",   
        ///         "Status": "Status of notification",   
        ///         "Date": "Notification creation date",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new notification</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationCM>> CreateNotification(NotificationCM model)
        {
            ResponseModel responseModel = new();

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create building") { StatusCode = 403 };
            }*/

            Notification crtNotification = _mapper.Map<Notification>(model);

            // Default POST Status = "Unread"
            crtNotification.Status = Status.UNREAD;

            try
            {
                await _service.AddAsync(crtNotification);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetNotificationById", new { id = crtNotification.Id }, crtNotification);
        }

        /// <summary>
        /// Update notification with specified id
        /// </summary>
        /// <param name="id">Notification's id</param>
        /// <param name="model">Information applied to updated notification</param>
        /// <response code="204">Update notification successfully</response>
        /// <response code="400">Notification's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutNotification(int id, NotificationUM model)
        {
            ResponseModel responseModel = new();

            if (id != model.Id)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Id));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            #region Get notification by ID
            Notification updNotification = await _service.GetByIdAsync(_ => _.Id == id);
            #endregion

            /*#region Authorization(Role = "Building Manager, Admin")
            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updBuilding, Operations.Update);

            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update building with id: {id}") { StatusCode = 403 };
            }
            #endregion*/

            #region Checking whether request is valid
            if (updNotification == null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Notification));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Status.READ && model.Status != Status.UNREAD)
                {
                    responseModel.Code = StatusCodes.Status400BadRequest;
                    responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                    responseModel.Type = ResponseType.INVALID_REQUEST;
                    return BadRequest(responseModel);
                }
            }
            #endregion

            #region Updating building
            try
            {
                if (model.Id > 0)
                {
                    updNotification.Id = model.Id;
                }
                
                if(!string.IsNullOrEmpty(model.Title))
                {
                    updNotification.Title = model.Title;
                }
                
                if(!string.IsNullOrEmpty(model.Body))
                {
                    updNotification.Body = model.Body;
                }
                
                if(!string.IsNullOrEmpty(model.ImageUrl))
                {
                    updNotification.ImageUrl = model.ImageUrl;
                }
                
                
                if(!string.IsNullOrEmpty(model.Screen))
                {
                    updNotification.Screen = model.Screen;
                }
                
                
                if(!string.IsNullOrEmpty(model.Parameter))
                {
                    updNotification.Parameter = model.Parameter;
                }
                
                
                if(model.AccountId >= 0)
                {
                    updNotification.AccountId = model.AccountId;
                }
                
                
                if(!string.IsNullOrEmpty(model.Status))
                {
                    updNotification.Status = model.Status;
                }
                
                
                if(model.Date.HasValue)
                {
                    updNotification.Date = model.Date.Value;
                }

                _service.Update(updNotification);
                if (await _service.Save() > 0)
                {
                    #region Updating cache
                    var cacheId = new CacheKey<Notification>(id);
                    await _cacheStore.Remove(cacheId);
                    #endregion
                }

            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_UPDATE;
                responseModel.Type = ResponseType.CAN_NOT_UPDATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            #endregion

            // Success
            return NoContent();
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        /// <param name="id">Notification's id</param>
        /// <response code="204">Delete notification's status successfully</response>
        /// <response code="400">Notification's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            Notification notification = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (notification is not null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Notification));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            try
            {
                _service.Delete(notification);
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
