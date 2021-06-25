using AutoMapper;
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

namespace IPSB.Controllers
{
    [Route("api/v1.0/locations")]
    [ApiController]
    public class LocationController : AuthorizeController
    {
        private readonly ILocationService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Location> _pagingSupport;
       
        public LocationController(ILocationService service, IMapper mapper, IPagingSupport<Location> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
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
        public ActionResult<LocationVM> GetLocationById(int id)
        {
            var location = _service.GetByIdAsync(_ => _.Id == id, _ => _.FloorPlan, _ => _.LocationType, _ => _.Store,
                _ => _.Store.Products,
                _ => _.EdgeFromLocations, _ => _.EdgeToLocations, _ => _.LocatorTags, _ => _.VisitPoints).Result;

            if (location == null)
            {
                return NotFound();
            }

            var rtnLocation = _mapper.Map<LocationVM>(location);

            return Ok(rtnLocation);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<LocationVM>> GetAllLocations([FromQuery] LocationSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Location> list = _service.GetAll(_ => _.FloorPlan, _ => _.LocationType, _ => _.Store,
                _ => _.Store.Products, _ => _.EdgeFromLocations, _ => _.EdgeToLocations, _ => _.LocatorTags, _ => _.VisitPoints);

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
        public async Task<ActionResult<LocationCM>> CreateLocation([FromBody] LocationCM model)
        {
            Location crtLocation = _mapper.Map<Location>(model);

            try
            {
                await _service.AddAsync(crtLocation);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetLocationById", new { id = crtLocation.Id }, crtLocation);
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
        public async Task<ActionResult> PutEdge(int id, [FromBody] LocationUM model)
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
