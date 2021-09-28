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

namespace IPSB.Controllers
{
    [Route("api/v1.0/floor-plans")]
    [ApiController]
    [Authorize(Roles = "Building Manager, Visitor")]
    public class FloorPlanController : AuthorizeController
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
            var cacheId = new CacheKey<FloorPlan>(id);
            var cacheObjectType = new FloorPlan();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var floorPlan = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var floorPlan = _service.GetByIdAsync(_ => _.Id == id,
                        _ => _.Building,
                        _ => _.LocatorTags,
                        _ => _.Stores).Result;


                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(floorPlan);

                }, ifModifiedSince);

                if (floorPlan == null)
                {
                    return NotFound();
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
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
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
        /// <response code="404">No floor plans found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FloorPlanVM>>> GetAllFloorPlans([FromQuery] FloorPlanSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            if (model.FloorNumber < 0)
            {
                return BadRequest();
            }

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

                if (model.BuildingId != 0)
                {
                    list = list.Where(_ => _.BuildingId == model.BuildingId);
                }

                if (!string.IsNullOrEmpty(model.FloorCode))
                {
                    list = list.Where(_ => _.FloorCode.Contains(model.FloorCode));
                }

                if (model.FloorNumber > 0)
                {
                    list = list.Where(_ => _.FloorNumber == model.FloorNumber);
                }

                var pagedModel = _pagingSupport.From(list)
                    .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                    .Paginate<FloorPlanVM>();

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

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, floorPlan, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create floor plan") { StatusCode = 403 };
            }*/

            string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map");
            FloorPlan crtFloorPlan = _mapper.Map<FloorPlan>(model);
            crtFloorPlan.ImageUrl = imageURL;
            crtFloorPlan.Status = "Inactive";
            crtFloorPlan.CreateDate = DateTime.Now;

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

            FloorPlan updLocationType = await _service.GetByIdAsync(_ => _.Id == id);
            if (updLocationType == null || id != model.Id)
            {
                return BadRequest();
            }

            if (model.FloorNumber < 0)
            {
                return BadRequest();
            }

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocationType, Operations.Update);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update floor plan with id: {id}") { StatusCode = 403 };
            }

            string imageURL = updLocationType.ImageUrl;

            if (model.ImageUrl is not null && model.ImageUrl.Length > 0)
            {
                imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "floor-plan", "floor-plan-map");
            }

            try
            {
                updLocationType.Id = model.Id;
                updLocationType.ImageUrl = imageURL;
                updLocationType.BuildingId = model.BuildingId;
                updLocationType.FloorCode = model.FloorCode;
                updLocationType.FloorNumber = model.FloorNumber;
                _service.Update(updLocationType);
                if (await _service.Save() > 0)
                {
                    #region Updating cache
                    var cacheId = new CacheKey<FloorPlan>(id);
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

        /// <summary>
        /// Change the status of floor plan to inactive
        /// </summary>
        /// <param name="id">Floor plan's id</param>
        /// <response code="204">Update floor plan's status successfully</response>
        /// <response code="400">Floor plan's id does not exist</response>
        /// <response code="500">Failed to update</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            FloorPlan floorPlan = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, floorPlan, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete floor plan with id: {id}") { StatusCode = 403 };
            }

            if (floorPlan is not null)
            {
                return BadRequest();
            }

            if (floorPlan.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            floorPlan.Status = Constants.Status.INACTIVE;
            try
            {
                _service.Update(floorPlan);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        protected override bool IsAuthorize()
        {
            throw new NotImplementedException();
        }
    }
}
