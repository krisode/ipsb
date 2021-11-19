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
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/facilities")]
    [ApiController]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityService _service;
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Facility> _pagingSupport;

        public FacilityController(IFacilityService service, ILocationService locationService, IMapper mapper, IPagingSupport<Facility> pagingSupport)
        {
            _service = service;
            _locationService = locationService;
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
            ResponseModel responseModel = new();

            var result = _service.GetAll(_ => _.Location)
                                .FirstOrDefault(_ => _.Id == id);

            if (result == null)
            {
                responseModel.Code = StatusCodes.Status404NotFound;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Facility));
                responseModel.Type = ResponseType.NOT_FOUND;
                return NotFound(responseModel);
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
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetAllFacilities([FromQuery] FacilitySM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var facilityList = _service.GetAll(_ => _.Location.LocationType, _ => _.FloorPlan);

            if (!string.IsNullOrEmpty(model.Name))
            {
                facilityList = facilityList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                facilityList = facilityList.Where(_ => _.Description.Contains(model.Description));
            }
            if (model.BuildingId >= 0)
            {
                facilityList = facilityList.Where(_ => _.BuildingId == model.BuildingId);
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
        /// Count facilities
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
        /// <returns>Number of facilities</returns>
        /// <response code="200">Returns number of facilities</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult CountFacilities([FromQuery] FacilitySM model)
        {
            var facilityList = _service.GetAll(_ => _.Location.LocationType, _ => _.FloorPlan);

            if (!string.IsNullOrEmpty(model.Name))
            {
                facilityList = facilityList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                facilityList = facilityList.Where(_ => _.Description.Contains(model.Description));
            }
            if (model.BuildingId >= 0)
            {
                facilityList = facilityList.Where(_ => _.BuildingId == model.BuildingId);
            }
            if (!string.IsNullOrEmpty(model.LocationType))
            {
                facilityList = facilityList.Where(_ => _.Location.LocationType.Name.Contains(model.LocationType));
            }

            
            return Ok(facilityList.Count());
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
            ResponseModel responseModel = new();

            var createdFacility = _mapper.Map<Facility>(model);

            try
            {
                createdFacility.Status = Status.ACTIVE;
                createdFacility.LocationId = await _locationService.CreateLocationJson(model.LocationJson);
                await _service.AddAsync(createdFacility);
                await _service.Save();
            }
            catch (Exception)
            {
                responseModel.Code = StatusCodes.Status500InternalServerError;
                responseModel.Message = ResponseMessage.CAN_NOT_CREATE;
                responseModel.Type = ResponseType.CAN_NOT_CREATE;
                return new ObjectResult(responseModel) { StatusCode = StatusCodes.Status500InternalServerError };
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
            ResponseModel responseModel = new();

            var updateFacility = await _service.GetByIdAsync(_ => _.Id == id);

            if (updateFacility is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Facility));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }
            try
            {
                updateFacility.LocationId =  await _locationService.UpdateLocationJson(updateFacility.LocationId, model.LocationJson);
                updateFacility.Name = model.Name;
                updateFacility.Description = model.Description;
                updateFacility.FloorPlanId = model.FloorPlanId;
                _service.Update(updateFacility);
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
            ResponseModel responseModel = new();

            var deleteFacility = await _service.GetByIdAsync(_ => _.Id == id);

            if (deleteFacility is null)
            {
                responseModel.Code = StatusCodes.Status400BadRequest;
                responseModel.Message = ResponseMessage.NOT_FOUND.Replace("Object", nameof(Facility));
                responseModel.Type = ResponseType.NOT_FOUND;
                return BadRequest(responseModel);
            }

            try
            {
                deleteFacility.Status = Status.INACTIVE;
                deleteFacility.LocationId = null;
                await _locationService.DisableLocation(deleteFacility.LocationId);
                _service.Update(deleteFacility);
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