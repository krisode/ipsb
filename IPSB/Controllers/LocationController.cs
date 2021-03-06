using AutoMapper;
using IPSB.Cache;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/locations")]
    [ApiController]
    [Authorize(Roles = "Building Manager")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Location> _pagingSupport;
        // private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStore _cacheStore;

        public LocationController(ILocationService service, IMapper mapper, IPagingSupport<Location> pagingSupport,
            ICacheStore cacheStore)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _cacheStore = cacheStore;
        }

        /// <summary>
        /// Get a specific location by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the location with the corresponding id</returns>
        /// <response code="200">Returns the location with the specified id</response>
        /// <response code="404">No locations found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationVM>> GetLocationById(int id)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Location>(id);
            var cacheObjectType = new Location();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var location = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var location = _service.GetByIdAsync(_ => _.Id == id,
                        _ => _.FloorPlan,
                        _ => _.LocationType,
                        _ => _.Store,
                        _ => _.Store.Products,
                        _ => _.EdgeFromLocations,
                        _ => _.EdgeToLocations,
                        _ => _.LocatorTag).Result;

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(location);

                }, ifModifiedSince);

                if (location == null)
                {
                    responseModel.Code = StatusCodes.Status404NotFound;
                    responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Location));
                    responseModel.Type = ResponseType.NOT_FOUND;
                    return NotFound(responseModel);
                }

                var rtnLocation = _mapper.Map<LocationVM>(location);

                return Ok(rtnLocation);
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
        /// Get all locations
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
        /// <returns>All locations</returns>
        /// <response code="200">Returns all locations</response>
        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LocationVM>>> GetAllLocations([FromQuery] LocationSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            ResponseModel responseModel = new();

            var cacheId = new CacheKey<Location>(Utils.Constants.DefaultValue.INTEGER);
            var cacheObjectType = new Location();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];
            try
            {
                var cacheResponse = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var list = _service.GetAll(_ => _.FloorPlan, _ => _.LocationType, _ => _.Store, _ => _.LocatorTag, _ => _.Facility);

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(list);

                }, setLastModified: (cachedTime) =>
                 {
                     Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedTime);
                     return cachedTime;
                 }, ifModifiedSince);

                var list = cacheResponse.Result;
                if (model.BuildingId != 0)
                {
                    list = list.ToList().Where(_ => _.FloorPlan?.BuildingId == model.BuildingId).AsQueryable();
                }

                if (!string.IsNullOrEmpty(model.Status))
                {
                    if (Status.ACTIVE.Equals(model.Status) || Status.INACTIVE.Equals(model.Status))
                    {
                        list = list.Where(_ => _.Status == model.Status);
                    }
                    else
                    {
                        responseModel.Code = StatusCodes.Status400BadRequest;
                        responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                        responseModel.Type = ResponseType.INVALID_REQUEST;
                        return BadRequest(responseModel);
                    }
                }

                if (model.FloorPlanId != 0)
                {
                    list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
                }

                if (model.StoreId != 0)
                {
                    list = list.ToList().Where(_ => _.Store?.Id == model.StoreId).AsQueryable();
                }

                if (model.LocationTypeId != 0)
                {
                    list = list.Where(_ => _.LocationTypeId == model.LocationTypeId);
                }

                if (model.NotLocationTypeIds != null && model.NotLocationTypeIds.Count() > 0)
                {
                    list = list.Where(_ => !model.NotLocationTypeIds.Contains(_.LocationTypeId));
                }
                if (model.LocationTypeIds != null && model.LocationTypeIds.Length > 0)
                {
                    list = list.Where(_ => model.LocationTypeIds.Contains(_.LocationTypeId));
                }

                if (!string.IsNullOrEmpty(model.SearchKey))
                {
                    list = list.ToList().Where(_ =>
                    (_.LocationType?.Name?.ToLower().Contains(model.SearchKey.ToLower()) ?? false)
                    || (_.Store?.Name?.ToLower().Contains(model.SearchKey.ToLower()) ?? false)
                    || (_.Facility?.Name?.ToLower().Contains(model.SearchKey.ToLower()) ?? false)).AsQueryable();
                }

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                    .Paginate<LocationVM>();
                if (cacheResponse.NotModified)
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }

                return Ok(pagedModel);

            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };

            }

        }

        /// <summary>
        /// Count locations
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
        /// <returns>Number of locations</returns>
        /// <response code="200">Returns number of locations</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> CountLocations([FromQuery] LocationSM model)
        {
            ResponseModel responseModel = new();
            try
            {
                var list = _service.GetAll();

                if (model.X != 0)
                {
                    list = list.Where(_ => _.X == model.X);
                }

                if (model.Y != 0)
                {
                    list = list.Where(_ => _.Y == model.Y);
                }
                if (!string.IsNullOrEmpty(model.Status))
                {
                    if (Status.ACTIVE.Equals(model.Status) || Status.INACTIVE.Equals(model.Status))
                    {
                        list = list.Where(_ => _.Status == model.Status);
                    }
                    else
                    {
                        responseModel.Code = StatusCodes.Status400BadRequest;
                        responseModel.Message = ResponseMessage.INVALID_PARAMETER.Replace("Object", nameof(model.Status));
                        responseModel.Type = ResponseType.INVALID_REQUEST;
                        return BadRequest(responseModel);
                    }
                }

                if (model.FloorPlanId != 0)
                {
                    list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
                }

                if (model.StoreId != 0)
                {
                    list = list.Where(_ => _.Store.Id == model.StoreId);
                }

                if (model.LocationTypeId != 0)
                {
                    list = list.Where(_ => _.LocationTypeId == model.LocationTypeId);
                }

                if (model.NotLocationTypeIds != null && model.NotLocationTypeIds.Count() > 0)
                {
                    list = list.Where(_ => !model.NotLocationTypeIds.Contains(_.LocationTypeId));
                }
                if (model.LocationTypeIds != null && model.LocationTypeIds.Length > 0)
                {
                    list = list.Where(_ => model.LocationTypeIds.Contains(_.LocationTypeId));
                }


                return Ok(list.Count());
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

        }

        /// <summary>
        /// Create a new location
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "X": "Location's longitude",   
        ///         "Y": "Location's latitude",   
        ///         "FloorPlanId": "Id of the floor plan",
        ///         "StoreId": "Id of the store",
        ///         "LocationTypeId": "Id of the location type"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new location</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateLocation([FromBody] List<LocationCM> listModel)
        {
            ResponseModel responseModel = new();

            List<Location> list = listModel.Select(model => _mapper.Map<Location>(model))
                .Select(_ =>
                {
                    _.Status = "Active";
                    return _;
                }).ToList();
            try
            {
                await _service.AddRangeAsync(list);
                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Location>(DefaultValue.INTEGER),
                        _cacheStore.Remove<LocatorTag>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Store>(DefaultValue.INTEGER)
                    );
                }
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            var listRefModel = list.Select(_ => _mapper.Map<LocationRefModel>(_));
            return CreatedAtAction("CreateLocation", listRefModel);
        }

        /// <summary>
        /// Update location with specified id
        /// </summary>
        /// <param name="id">Location's id</param>
        /// <param name="model">Information applied to updated location</param>
        /// <response code="204">Update location successfully</response>
        /// <response code="400">Location's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutLocation(int id, [FromBody] LocationUM model)
        {
            ResponseModel responseModel = new();

            Location updLocation = await _service.GetByIdAsync(_ => _.Id == id);

            try
            {
                updLocation.X = model.X;
                updLocation.Y = model.Y;
                updLocation.FloorPlanId = model.FloorPlanId;
                updLocation.LocationTypeId = model.LocationTypeId;
                _service.Update(updLocation);
                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Location>(id),
                        _cacheStore.Remove<LocatorTag>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Store>(DefaultValue.INTEGER)
                    );
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
        /// Change status of locations based on a specified list of ids to Inactive
        /// </summary>
        /// <param name="model">List of locations's id used to delete</param>
        /// <response code="204">Delete locations status successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Authorize(Roles = "Building Manager")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteRange([FromBody] LocationDM model)
        {
            ResponseModel responseModel = new();

            try
            {
                if (model.Ids != null && model.Ids.Count > 0)
                {
                    // Delete location if location is point on route
                    _service.DeleteRange(model.Ids);
                }

                if (await _service.Save() > 0)
                {
                    await Task.WhenAll(
                        _cacheStore.Remove<Location>(DefaultValue.INTEGER),
                        _cacheStore.Remove<LocatorTag>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Edge>(DefaultValue.INTEGER),
                        _cacheStore.Remove<Store>(DefaultValue.INTEGER)
                    );
                }
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
