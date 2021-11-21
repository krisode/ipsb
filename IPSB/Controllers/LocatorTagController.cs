using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/locator-tags")]
    [ApiController]
    /*[Authorize(Roles = "Building Manager")]*/
    public class LocatorTagController : ControllerBase
    {
        private readonly ILocatorTagService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<LocatorTag> _pagingSupport;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocationService _locationService;

        public LocatorTagController(ILocatorTagService service, IMapper mapper, IPagingSupport<LocatorTag> pagingSupport, IAuthorizationService authorizationService, ILocationService locationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _authorizationService = authorizationService;
            _locationService = locationService;
        }


        /// <summary>
        /// Get a specific locator tag by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the locator tag with the corresponding id</returns>
        /// <response code="200">Returns the locator tag with the specified id</response>
        /// <response code="404">No locator tags found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<LocatorTagVM> GetLocatorTagById(int id)
        {
            ResponseModel responseModel = new();

            var locatorTag = _service.GetByIdAsync(_ => _.Id == id, _ => _.FloorPlan, _ => _.Location).Result;

            if (locatorTag == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(LocatorTag));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, locatorTag, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to access locator tag with id: {id}");
            }*/

            var rtnLocatorTag = _mapper.Map<LocatorTagVM>(locatorTag);

            return Ok(rtnLocatorTag);
        }

        /// <summary>
        /// Get all locator tags
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
        /// <returns>All locator tags</returns>
        /// <response code="200">Returns all locator tags</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<LocatorTagVM>> GetAllLocatorTags([FromQuery] LocatorTagSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            IQueryable<LocatorTag> list = _service.GetAll(_ => _.FloorPlan, _ => _.Location, _ => _.LocatorTagGroup);

            if (model.Id != null)
            {
                list = list.Where(_ => model.Id.Contains(_.Id));
            }

            if (!string.IsNullOrEmpty(model.Uuid))
            {
                list = list.Where(_ => _.Uuid.Equals(model.Uuid));
            }

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (model.FloorPlanId != 0)
            {
                list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
            }

            if (model.LocationId != 0)
            {
                list = list.Where(_ => _.LocationId == model.LocationId);
            }

            if (model.LowerUpdateTime.HasValue)
            {
                list = list.Where(_ => _.UpdateTime >= model.LowerUpdateTime);
            }

            if (model.UpperUpdateTime.HasValue)
            {
                list = list.Where(_ => _.UpdateTime <= model.UpperUpdateTime);
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


            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<LocatorTagVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Count locator tags
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
        /// <returns>Number of locator tags</returns>
        /// <response code="200">Returns number of locator tags</response>
        [HttpGet]
        [Route("count")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<LocatorTagVM>> CountLocatorTags([FromQuery] LocatorTagSM model)
        {
            ResponseModel responseModel = new();

            IQueryable<LocatorTag> list = _service.GetAll(_ => _.FloorPlan, _ => _.Location, _ => _.LocatorTagGroup);

            if (model.Id != null)
            {
                list = list.Where(_ => model.Id.Contains(_.Id));
            }

            if (!string.IsNullOrEmpty(model.Uuid))
            {
                list = list.Where(_ => _.Uuid.Equals(model.Uuid));
            }

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (model.FloorPlanId != 0)
            {
                list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
            }

            if (model.LocationId != 0)
            {
                list = list.Where(_ => _.LocationId == model.LocationId);
            }

            if (model.LowerUpdateTime.HasValue)
            {
                list = list.Where(_ => _.UpdateTime >= model.LowerUpdateTime);
            }

            if (model.UpperUpdateTime.HasValue)
            {
                list = list.Where(_ => _.UpdateTime <= model.UpperUpdateTime);
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
        /// Create a new locator tag
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "MacAddress": "Mac Adress of the beacon",   
        ///         "Status": "Status of the beacon",   
        ///         "UpdateTime": "The date time the beacon is updated",   
        ///         "FloorPlanId": "Id of the floor plan that the beacon belongs to",   
        ///         "LocationId": "Id of the location that the beacon is located",   
        ///         "LastSeen": "The date time when the beacon was last scanned by vistor ",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new locator tag</response>
        /// <response code="409">Locator tag already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LocatorTagCM>> CreateLocatorTag([FromBody] LocatorTagCM model)
        {
            ResponseModel responseModel = new();

            LocatorTag locatorTag = _service.GetAllWhere(_ => _.Uuid == model.Uuid).FirstOrDefault();

            if (locatorTag is not null)
            {
                responseModel.Code = StatusCodes.Status409Conflict;
                responseModel.Message = ResponseMessage.DUPLICATED.Replace("Object", model.Uuid);
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return Conflict(responseModel);
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, locatorTag, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create locator tag") { StatusCode = 403 };
            }*/


            LocatorTag crtLocatorTag = _mapper.Map<LocatorTag>(model);
            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            crtLocatorTag.UpdateTime = localTime.DateTime;

            // Default POST Status = "New"
            crtLocatorTag.Status = Status.NEW;

            try
            {
                await _service.AddAsync(crtLocatorTag);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return CreatedAtAction("GetLocatorTagById", new { id = crtLocatorTag.Id }, crtLocatorTag);
        }

        /// <summary>
        /// Update locator tag with specified id
        /// </summary>
        /// <param name="id">Locator tag's id</param>
        /// <param name="model">Information applied to updated locator tag</param>
        /// <response code="204">Update locator tag successfully</response>
        /// <response code="400">Locator tag's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutLocatorTag(int id, [FromBody] LocatorTagUM model)
        {
            ResponseModel responseModel = new();

            LocatorTag updLocatorTag = await _service.GetByIdAsync(_ => _.Id == id);

            if (updLocatorTag is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(LocatorTag));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocatorTag, Operations.Read);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update locator tag with id: {id}") { StatusCode = 403 };
            // }

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            try
            {
                if (updLocatorTag.Status == Status.NEW && updLocatorTag.LocationId == null)
                {
                    updLocatorTag.Status = Status.ACTIVE;
                }
                updLocatorTag.LocationId = await _locationService.UpdateLocationJson(updLocatorTag.LocationId, model.LocationJson);
                updLocatorTag.TxPower = model.TxPower;
                updLocatorTag.UpdateTime = localTime.DateTime;
                updLocatorTag.FloorPlanId = model.FloorPlanId;
                updLocatorTag.LocatorTagGroupId = model.LocatorTagGroupId;

                _service.Update(updLocatorTag);
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
        /// Update locator tag's tx power with specified uuid
        /// </summary>
        /// <param name="uuid">Locator tag's uuid</param>
        /// <param name="model">Locator tag model used to update tx power</param>
        /// <response code="204">Update tx power successfully</response>
        /// <response code="400">Locator tag's uuid does not exist</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("tx-power/{uuid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutLocatorTagTxPower(string uuid, [FromBody] LocatorTagTxPowerUM model)
        {
            ResponseModel responseModel = new();

            LocatorTag updLocatorTag = await _service.GetByIdAsync(_ => _.Uuid == uuid);

            if (updLocatorTag is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(LocatorTag));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocatorTag, Operations.Read);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update locator tag with id: {id}") { StatusCode = 403 };
            // }


            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);


            try
            {
                updLocatorTag.TxPower = model.TxPower;
                updLocatorTag.UpdateTime = localTime.DateTime;

                _service.Update(updLocatorTag);
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
        /// Delete locator tag
        /// </summary>
        /// <param name="id">Locator tag's id</param>
        /// <response code="204">Delete locator tag's status successfully</response>
        /// <response code="400">Locator tag's id does not exist</response>
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

            var locatorTag = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (locatorTag is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(LocatorTag));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            if (locatorTag.Status.Equals(Status.INACTIVE))
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.DELETED.Replace("Object", nameof(LocatorTag));
                responseModel.Type = ResponseType.INVALID_REQUEST;
                return BadRequest(responseModel);
            }

            locatorTag.Status = Status.INACTIVE;
            try
            {
                _service.Update(locatorTag);
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
