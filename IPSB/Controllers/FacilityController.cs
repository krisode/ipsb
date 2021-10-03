using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IPSB.Core.Services;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPSB.Controllers
{
    [Route("api/v1.0/facilities")]
    [ApiController]
    public class FacilityController : Controller
    {
        private readonly IFacilityService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Facility> _pagingSupport;

        public FacilityController(IFacilityService service, IMapper mapper, IPagingSupport<Facility> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific facility by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the facility with the corresponding id</returns>
        /// <response code="200">Returns the facility with the specified id</response>
        /// <response code="404">No facility found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public ActionResult GetFacilityById(int id)
        {
            var result = _service.GetAll(_ => _.Location)
                                .FirstOrDefault(_ => _.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            var rtnFacility = _mapper.Map<FacilityVM>(result);
            return Ok(rtnFacility);
        }

        /// <summary>
        /// Get all facilities
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         "name": "Name of facility",
        ///         "description": "Description of facility",
        ///         "locationId": "Location of facility"
        ///     }
        ///
        /// </remarks>
        /// <returns>All facilities</returns>
        /// <response code="200">Returns all facilities</response>
        /// <response code="404">No facilities found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult GetAllFacilities([FromQuery] FacilitySM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var facilityList = _service.GetAll(_ => _.Location);

            if (!string.IsNullOrEmpty(model.Name))
            {
                facilityList = facilityList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                facilityList = facilityList.Where(_ => _.Description.Contains(model.Description));
            }
            if (model.LocationId > 0)
            {
                facilityList = facilityList.Where(_ => _.LocationId == model.LocationId);
            }
            if (!string.IsNullOrEmpty(model.LocationType))
            {
                facilityList = facilityList.Where(_ => _.Location.LocationType.Name.Contains(model.LocationType));
            }

            var pagedModel = _pagingSupport.From(facilityList)
                                            .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                                            .Paginate<FacilityVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new facility
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "name": "Facility's name",   
        ///         "description": "Facility's description",  
        ///         "locationId": "Facility's description",  
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created new facility</response>
        /// <response code="400">Required create data is missing</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        // [Authorize(Roles = "Building Manager")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateFacility([FromBody] FacilityCM model)
        {
            var createdFacility = _mapper.Map<Facility>(model);

            try
            {
                createdFacility.Status = Constants.Status.ACTIVE;
                await _service.AddAsync(createdFacility);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("CreateFacility", new { id = createdFacility.Id }, createdFacility);
        }

        /// <summary>
        /// Update facility with specified id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT 
        ///     {
        ///         "name": "Facility's name",   
        ///         "description": "Facility's description",  
        ///         "locationId": "Facility's description",  
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Facility's id</param>
        /// <param name="model">Information applied to update facility</param>
        /// <response code="204">Update facility successfully</response>
        /// <response code="400">Required update data is missing</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        // [Authorize(Roles = "Building Manager")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateFacility(int id, [FromBody] FacilityUM model)
        {
            var updateFacility = await _service.GetByIdAsync(_ => _.Id == id);

            try
            {
                updateFacility.Name = model.Name;
                updateFacility.Description = model.Description;
                updateFacility.LocationId = model.LocationId;
                _service.Update(updateFacility);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete facility with specified id
        /// </summary>
        /// <param name="id">Facility's id</param>
        /// <response code="204">Delete facility successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        // [Authorize(Roles = "Building Manager")]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var deleteFacility = await _service.GetByIdAsync(_ => _.Id == id);
            try
            {
                deleteFacility.Status = Constants.Status.INACTIVE;
                _service.Update(deleteFacility);
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