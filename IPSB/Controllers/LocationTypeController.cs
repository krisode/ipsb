using AutoMapper;
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
    [Route("api/v1.0/location-types")]
    [ApiController]
    public class LocationTypeController : AuthorizeController
    {
        private readonly ILocationTypeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<LocationType> _pagingSupport;
        // private readonly IAuthorizationService _authorizationService;

        public LocationTypeController(ILocationTypeService service, IMapper mapper, IPagingSupport<LocationType> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific location type by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the location type with the corresponding id</returns>
        /// <response code="200">Returns the location type with the specified id</response>
        /// <response code="404">No location types found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<LocationTypeVM> GetLocationTypeById(int id)
        {
            var locationType = _service.GetByIdAsync(_ => _.Id == id, _ => _.Locations).Result;

            if (locationType == null)
            {
                return NotFound();
            }

            var rtnLocationType = _mapper.Map<LocationTypeRefModel>(locationType);

            return Ok(rtnLocationType);
        }

        /// <summary>
        /// Get all location types
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
        /// <returns>All location types</returns>
        /// <response code="200">Returns all location types</response>
        /// <response code="404">No location types found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<LocationTypeVM>> GetAllLocationTypes([FromQuery] LocationTypeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<LocationType> list = _service.GetAll(_ => _.Locations);

            if (!string.IsNullOrEmpty(model.Name))
            {
                list = list.Where(_ => _.Name.Contains(model.Name));
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                list = list.Where(_ => _.Description.Contains(model.Description));
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<LocationTypeRefModel>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new location type
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "Name": "Name of the location type",   
        ///         "Description": "Description of location type",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new location type</response>
        /// <response code="400">Invalid parameters</response>
        /// <response code="409">Location type already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LocationTypeCM>> CreateLocationType([FromBody] LocationTypeCM model)
        {
            LocationType locationType = _service.GetByIdAsync(_ => _.Name.ToUpper() == model.Name.ToUpper()).Result;
            if (locationType is not null)
            {
                return Conflict();
            }

            LocationType crtLocationType = _mapper.Map<LocationType>(model);

            try
            {
                await _service.AddAsync(crtLocationType);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetLocationTypeById", new { id = crtLocationType.Id }, crtLocationType);
        }

        /// <summary>
        /// Update location type with specified id
        /// </summary>
        /// <param name="id">Location type's id</param>
        /// <param name="model">Information applied to updated location type</param>
        /// <response code="204">Update location type successfully</response>
        /// <response code="400">Location type's id does not exist or does not match with the id in parameter</response>
        /// <response code="409">Location type already exists</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutLocationType(int id, [FromBody] LocationTypeUM model)
        {
            LocationType updLocationType = await _service.GetByIdAsync(_ => _.Id == id);
            if (updLocationType == null || id != model.Id)
            {
                return BadRequest();
            }

            if (updLocationType.Name.ToUpper() == model.Name.ToUpper())
            {
                return Conflict();
            }

            try
            {
                updLocationType.Id = model.Id;
                updLocationType.Name = model.Name;
                updLocationType.Description = model.Description;
                _service.Update(updLocationType);
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
        // Future PLan
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
