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
using System.Threading.Tasks;

namespace IPSB.Controllers
{
    [Route("api/v1.0/locations")]
    [ApiController]
    [Authorize(Roles = "Building Manager")]
    public class LocationController : Controller
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
                        _ => _.LocatorTags,
                        _ => _.VisitPoints).Result;

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(location);

                }, ifModifiedSince);

                if (location == null)
                {
                    return NotFound();
                }

                var rtnLocation = _mapper.Map<LocationVM>(location);

                return Ok(rtnLocation);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
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
        /// <response code="404">No locations found</response>
        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LocationVM>>> GetAllLocations([FromQuery] LocationSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var cacheId = new CacheKey<Location>(Utils.Constants.DefaultValue.INTEGER);
            var cacheObjectType = new Location();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];
            try
            {
                var list = _service.GetAll(_ => _.FloorPlan, _ => _.LocationType, _ => _.Store);
                // var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                // {
                //     var list = _service.GetAll(_ => _.FloorPlan, _ => _.LocationType, _ => _.Store);

                //     Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                //     return Task.FromResult(list);

                // }, ifModifiedSince);

                if (model.BuildingId != 0)
                {
                    list = list.Where(_ => _.FloorPlan.BuildingId == model.BuildingId);
                }

                if (model.X != 0)
                {
                    list = list.Where(_ => _.X == model.X);
                }

                if (model.Y != 0)
                {
                    list = list.Where(_ => _.Y == model.Y);
                }

                if (model.FloorPlanId != 0)
                {
                    list = list.Where(_ => _.FloorPlanId == model.FloorPlanId);
                }

                if (model.StoreId != 0)
                {
                    list = list.Where(_ => _.StoreId == model.StoreId);
                }

                if (model.LocationTypeId != 0)
                {
                    list = list.Where(_ => _.LocationTypeId == model.LocationTypeId);
                }

                if (model.NotLocationTypeId != 0)
                {
                    list = list.Where(_ => _.LocationTypeId != model.NotLocationTypeId);
                }
                if (model.LocationTypeIds != null && model.LocationTypeIds.Length > 0)
                {
                    list = list.Where(_ => model.LocationTypeIds.Contains(_.LocationTypeId));
                }

                if (!string.IsNullOrEmpty(model.LocationTypeName))
                {
                    list = list.Where(_ => _.LocationType.Name.Contains(model.LocationTypeName));
                }

                if (!string.IsNullOrEmpty(model.StoreName))
                {
                    list = list.Where(_ => _.Store.Name.Contains(model.StoreName));
                }

                if (!string.IsNullOrEmpty(model.ProductName))
                {

                    list = list.Where(_ => _.Store.Products.Any(_ => _.Name.Contains(model.ProductName)));
                }

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                    .Paginate<LocationVM>();

                return Ok(pagedModel);

            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
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
            List<Location> list = listModel.Select(model => _mapper.Map<Location>(model))
                .ToList()
                .Select(_ =>
                {
                    _.Status = "Active";
                    return _;
                }).ToList();
            try
            {
                await _service.AddRangeAsync(list);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var rtnItemIds = list.Select(_ => _.Id);
            return CreatedAtAction("CreateLocation", rtnItemIds);
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
            Location updLocation = await _service.GetByIdAsync(_ => _.Id == id);
            if (updLocation == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                updLocation.Id = model.Id;
                updLocation.X = model.X;
                updLocation.Y = model.Y;
                updLocation.FloorPlanId = model.FloorPlanId;
                updLocation.StoreId = model.StoreId;
                updLocation.LocationTypeId = model.LocationTypeId;
                _service.Update(updLocation);
                if (await _service.Save() > 0)
                {
                    #region Updating cache
                    var cacheId = new CacheKey<Location>(id);
                    await _cacheStore.Remove(cacheId);
                    #endregion
                }

            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return NoContent();
        }


        // DELETE api/<LocationController>?id=1&id=3
        // Change Status to Inactive
        [HttpDelete]
        public async Task<ActionResult> DeleteRange([FromBody] LocationDM model)
        {
            try
            {
                if (model.Ids != null && model.Ids.Count > 0)
                {
                    // Delete location if location is point on route
                    _service.DeleteRange(model.Ids);
                    // Change location status to "Inactive" if location is not point on route
                    _service.Disable(model.Ids);

                }

                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }


    }
}
