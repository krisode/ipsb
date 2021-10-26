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
    [Authorize(Roles = "Building Manager")]
    public class LocatorTagController : Controller
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
            var locatorTag = _service.GetByIdAsync(_ => _.Id == id, _ => _.FloorPlan, _ => _.Location).Result;

            if (locatorTag == null)
            {
                return NotFound();
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
        /// <response code="404">No locator tags found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<LocatorTagVM>> GetAllLocatorTags([FromQuery] LocatorTagSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
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
                list = list.Where(_ => _.FloorPlan.BuildingId == model.BuildingId);
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
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE && model.Status != Constants.Status.NEW)
                {
                    return BadRequest();
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
            LocatorTag locatorTag = _service.GetAllWhere(_ => _.Uuid == model.Uuid).FirstOrDefault();

            if (locatorTag is not null)
            {
                return Conflict();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, locatorTag, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create locator tag") { StatusCode = 403 };
            }*/
            int locationId = 0;
            if (!string.IsNullOrEmpty(model.LocationJson))
            {
                var json = JsonConvert.DeserializeObject<Location>(model.LocationJson);
                if (json != null && json.Id == 0)
                {
                    json.Status = Status.ACTIVE;
                    var locationToCreate = await _locationService.AddAsync(json);
                    await _service.Save();
                    locationId = locationToCreate.Id;
                }
            }
            LocatorTag crtLocatorTag = _mapper.Map<LocatorTag>(model);
            crtLocatorTag.UpdateTime = DateTime.Now;
            crtLocatorTag.LocationId = locationId;

            // Default POST Status = "Active"
            crtLocatorTag.Status = Constants.Status.ACTIVE;

            try
            {
                await _service.AddAsync(crtLocatorTag);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
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
            LocatorTag updLocatorTag = await _service.GetByIdAsync(_ => _.Id == id);

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocatorTag, Operations.Read);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update locator tag with id: {id}") { StatusCode = 403 };
            // }

            

            try
            {
                updLocatorTag.LocationId = await _locationService.UpdateLocationJson(updLocatorTag.LocationId, model.LocationJson);
                updLocatorTag.TxPower = model.TxPower;
                updLocatorTag.UpdateTime = DateTime.Now;
                updLocatorTag.FloorPlanId = model.FloorPlanId;
                updLocatorTag.LocatorTagGroupId = model.LocatorTagGroupId;

                _service.Update(updLocatorTag);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete locator tag
        /// </summary>
        /// <param name="id">Locator tag's id</param>
        /// <response code="204">Update locator tag's status successfully</response>
        /// <response code="400">Locator tag's id does not exist</response>
        /// <response code="500">Failed to update</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            var locatorTag = await _service.GetByIdAsync(_ => _.Id == id);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete building with id: {id}") { StatusCode = 403 };
            }*/

            if (locatorTag is null)
            {
                return BadRequest();
            }

            if (locatorTag.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            locatorTag.Status = Status.INACTIVE;
            try
            {
                _service.Update(locatorTag);
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
