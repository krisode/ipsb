using AutoMapper;
using IPSB.Core.Services;
using IPSB.ExternalServices;
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
    [Route("api/v1.0/floor-plans")]
    [ApiController]
    public class FloorPlanController : AuthorizeController
    {
        private readonly IFloorPlanService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<FloorPlan> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;

        public FloorPlanController(IFloorPlanService service, IMapper mapper, IPagingSupport<FloorPlan> pagingSupport, IUploadFileService uploadFileService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
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
        [HttpGet("{id}")]
        public ActionResult<FloorPlanVM> GetFloorPlanById(int id)
        {
            var floorPlan = _service.GetByIdAsync(_ => _.Id == id, _ => _.Building, _ => _.Locations, _ => _.LocatorTags, _ => _.Stores);

            if (floorPlan == null)
            {
                return NotFound();
            }

            var rtnEdge = _mapper.Map<FloorPlanVM>(floorPlan.Result);

            return Ok(rtnEdge);
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
        /// <response code="404">No floor plans found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<FloorPlanVM>> GetAllFloorPlans([FromQuery] FloorPlanSM model, int pageSize = 20, int pageIndex = 1, bool isAscending = true)
        {
            IQueryable<FloorPlan> list = _service.GetAll(_ => _.Building, _ => _.Locations, _ => _.LocatorTags, _ => _.Stores);

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (!string.IsNullOrEmpty(model.FloorCode))
            {
                list = list.Where(_ => _.FloorCode.Contains(model.FloorCode));
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAscending)
                .Paginate<FloorPlanVM>();

            return Ok(pagedModel);
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
            FloorPlan floorPlan = _service.GetByIdAsync(_ => _.FloorCode.ToUpper() == model.FloorCode).Result;
            if (floorPlan is not null)
            {
                return Conflict();
            }

            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map");
            FloorPlan crtFloorPlan = _mapper.Map<FloorPlan>(model);
            crtFloorPlan.ImageUrl = imageURL;

            try
            {
                await _service.AddAsync(crtFloorPlan);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetFloorPlanById", new { id = crtFloorPlan.Id }, crtFloorPlan);
        }

        /// <summary>
        /// Update floor plan with specified id
        /// </summary>
        /// <param name="id">Floor plan's id</param>
        /// <param name="model">Information applied to updated floor plan</param>
        /// <response code="204">Update floor plan successfully</response>
        /// <response code="400">>Floor plan's id does not exist or does not match with the id in parameter</response>
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

            FloorPlan updLocationType = await _service.GetByIdAsync(_ => _.Id == id);
            if (updLocationType == null || id != model.Id)
            {
                return BadRequest();
            }

            if (updLocationType.FloorCode.ToUpper() == model.FloorCode)
            {
                return Conflict();
            }

            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map");

            try
            {
                updLocationType.Id = model.Id;
                updLocationType.ImageUrl = imageURL;
                updLocationType.BuildingId = model.BuildingId;
                updLocationType.FloorCode = model.FloorCode;
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
