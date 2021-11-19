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
    [Route("api/v1.0/floor-plans")]
    [ApiController]
    [Authorize(Roles = "Building Manager, Visitor")]
    public class FloorPlanController : ControllerBase
    {
        private readonly IFloorPlanService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<FloorPlan> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStore _cacheStore;

        public FloorPlanController(IFloorPlanService service, IMapper mapper, IPagingSupport<FloorPlan> pagingSupport,
            IUploadFileService uploadFileService, IAuthorizationService authorizationService, ICacheStore cacheStore)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _cacheStore = cacheStore;
        }

        /// <summary>
        /// Get a specific floor plan by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the floor plan with the corresponding id</returns>
        /// <response code="200">Returns the floor plan with the specified id</response>
        /// <response code="404">No floor plans found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<FloorPlanVM>> GetFloorPlanById(int id)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<FloorPlan>(id);
            var cacheObjectType = new FloorPlan();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var floorPlan = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var floorPlan = _service.GetByIdAsync(_ => _.Id == id).Result;


                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(floorPlan);

                }, ifModifiedSince);

                if (floorPlan == null)
                {
                    responseModel.Code = StatusCodes.Status404NotFound;
                    responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(FloorPlan));
                    responseModel.Type = ResponseType.NOT_FOUND;
                    return NotFound(responseModel);
                }

                /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, floorPlan, Operations.Read);
                if (!authorizedResult.Succeeded)
                {
                    return Forbid($"Not authorized to access floor plan with id: {id}");
                }*/

                var rtnFloorPlan = _mapper.Map<FloorPlanVM>(floorPlan);

                return Ok(rtnFloorPlan);
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
        /// Get all floor plans
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
        /// <returns>All floor plans</returns>
        /// <response code="200">Returns all floor plans</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FloorPlanVM>>> GetAllFloorPlans([FromQuery] FloorPlanSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<FloorPlan>(DefaultValue.INTEGER);
            var cacheObjectType = new FloorPlan();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

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

                if (model.NotFloorPlanId >= 0)
                {
                    list = list.Where(_ => _.Id != model.NotFloorPlanId);
                }

                if (model.BuildingId >= 0)
                {
                    list = list.Where(_ => _.BuildingId == model.BuildingId);
                }

                if (!string.IsNullOrEmpty(model.FloorCode))
                {
                    list = list.Where(_ => _.FloorCode.Contains(model.FloorCode));
                }

                if (Status.ACTIVE.Equals(model.Status) || Status.INACTIVE.Equals(model.Status))
                {
                    list = list.Where(_ => _.Status == model.Status);
                }

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.FloorNumber, isAll, isAscending)
                    .Paginate<FloorPlanVM>();

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
        /// Count floor plans
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
        /// <returns>Number of floor plans</returns>
        /// <response code="200">Returns number of floor plans</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FloorPlanVM>>> CountFloorPlans([FromQuery] FloorPlanSM model)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<FloorPlan>(Utils.Constants.DefaultValue.INTEGER);
            var cacheObjectType = new FloorPlan();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

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

                if (model.NotFloorPlanId >= 0)
                {
                    list = list.Where(_ => _.Id != model.NotFloorPlanId);
                }

                if (model.BuildingId >= 0)
                {
                    list = list.Where(_ => _.BuildingId == model.BuildingId);
                }

                if (!string.IsNullOrEmpty(model.FloorCode))
                {
                    list = list.Where(_ => _.FloorCode.Contains(model.FloorCode));
                }

                if (Status.ACTIVE.Equals(model.Status) || Status.INACTIVE.Equals(model.Status))
                {
                    list = list.Where(_ => _.Status == model.Status);
                }

                return Ok(list.Count());
            }
            catch (Exception e)
            {
                if (e.Message.Equals(ExceptionMessage.NOT_MODIFIED))
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
        /// Create a new floor plan
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "ImageURL": "The 2D image of the floor plan",   
        ///         "BuildingId": "The building's id that the floor plan belongs to",   
        ///         "FloorCode": "Code of the floor plan",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new floor plan</response>
        /// <response code="409">Location type already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FloorPlanCM>> CreateFloorPlan([FromForm] FloorPlanCM model)
        {
            ResponseModel responseModel = new();

            int existed = _service.GetAll().Where(
                _ => _.FloorCode.ToUpper() == model.FloorCode.ToUpper()
                && _.BuildingId == model.BuildingId
                ).Count();
            if (existed >= 1)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.FloorCode);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, floorPlan, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create floor plan") { StatusCode = 403 };
            }*/

            FloorPlan crtFloorPlan = _mapper.Map<FloorPlan>(model);
            crtFloorPlan.ImageUrl = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map"); ;
            crtFloorPlan.Status = Status.ACTIVE;

            try
            {
                await _service.AddAsync(crtFloorPlan);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetFloorPlanById", new { id = crtFloorPlan.Id }, crtFloorPlan);
        }

        /// <summary>
        /// Update floor plan with specified id
        /// </summary>
        /// <param name="id">Floor plan's id</param>
        /// <param name="model">Information applied to updated floor plan</param>
        /// <response code="204">Update floor plan successfully</response>
        /// <response code="400">Floor plan's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Floor plan already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutFloorPlan(int id, [FromForm] FloorPlanUM model)
        {
            ResponseModel responseModel = new();

            int managerId = -1;
            int.TryParse(User.Identity.Name, out managerId);
            int existed = _service.GetAll().Where(
                            _ => _.FloorCode.ToUpper() == model.FloorCode.ToUpper()
                            && _.Id != id
                            && _.Building.ManagerId == managerId
                            ).Count();
            if (existed >= 1)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.FloorCode);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            FloorPlan updFloorPlan = await _service.GetByIdAsync(_ => _.Id == id);



            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocationType, Operations.Update);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update floor plan with id: {id}") { StatusCode = 403 };
            // }

            string imageURL = updFloorPlan.ImageUrl;

            if (model.ImageUrl is not null)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map");
            }

            try
            {
                updFloorPlan.ImageUrl = imageURL;
                updFloorPlan.FloorCode = model.FloorCode;
                updFloorPlan.FloorNumber = model.FloorNumber;
                updFloorPlan.RotationAngle = model.RotationAngle;
                updFloorPlan.MapScale = model.MapScale;
                // updLocationType.Status = model.Status;
                _service.Update(updFloorPlan);
                if (await _service.Save() > 0)
                {
                    // #region Updating cache
                    // var cacheId = new CacheKey<FloorPlan>(id);
                    // await _cacheStore.Remove(cacheId);
                    // #endregion
                }

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
        /// Change the status of floor plan to inactive
        /// </summary>
        /// <param name="id">Floor plan's id</param>
        /// <response code="204">Delete floor plan's status successfully</response>
        /// <response code="400">Floor plan's id does not exist</response>
        /// <response code="500">Failed to delete</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            ResponseModel responseModel = new();

            FloorPlan floorPlan = await _service.GetByIdAsync(_ => _.Id == id);

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, floorPlan, Operations.Delete);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to delete floor plan with id: {id}") { StatusCode = 403 };
            // }

            if (floorPlan is not null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(FloorPlan));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (floorPlan.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(FloorPlan));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            floorPlan.Status = Status.INACTIVE;

            try
            {
                _service.Update(floorPlan);
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
