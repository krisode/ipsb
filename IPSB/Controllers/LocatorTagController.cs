using AutoMapper;
using IPSB.AuthorizationHandler;
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
    [Route("api/v1.0/locator-tags")]
    [ApiController]
    [Authorize(Roles = "Building Manager")]
    public class LocatorTagController : AuthorizeController
    {
        private readonly ILocatorTagService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<LocatorTag> _pagingSupport;
        private readonly IAuthorizationService _authorizationService;
        public LocatorTagController(ILocatorTagService service, IMapper mapper, IPagingSupport<LocatorTag> pagingSupport, IAuthorizationService authorizationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _authorizationService = authorizationService;
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
            IQueryable<LocatorTag> list = _service.GetAll(_ => _.FloorPlan, _ => _.Location);

            IQueryable<LocatorTag> rtnlist = list;
            if (model.Id is not null && model.Id.Length > 0)
            {
                List<LocatorTag> locatorTags = new List<LocatorTag>();
                foreach (var id in model.Id)
                {
                    if (list.Where(_ => _.Id == id).FirstOrDefault() is not null)
                    {
                        locatorTags.Add(list.Where(_ => _.Id == id).FirstOrDefault());
                    }
                }
                rtnlist = locatorTags.AsQueryable();
            }

            if (!string.IsNullOrEmpty(model.MacAddress))
            {
                rtnlist = rtnlist.Where(_ => _.MacAddress.Contains(model.MacAddress));
            }

            if (model.FloorPlanId != 0)
            {
                rtnlist = rtnlist.Where(_ => _.FloorPlanId == model.FloorPlanId);
            }

            if (model.LocationId != 0)
            {
                rtnlist = rtnlist.Where(_ => _.LocationId == model.LocationId);
            }

            if (model.LowerUpdateTime.HasValue)
            {
                rtnlist = rtnlist.Where(_ => _.UpdateTime >= model.LowerUpdateTime);
            }

            if (model.UpperUpdateTime.HasValue)
            {
                rtnlist = rtnlist.Where(_ => _.UpdateTime <= model.UpperUpdateTime);
            }

            if (model.LowerLastSeen.HasValue)
            {
                rtnlist = rtnlist.Where(_ => _.LastSeen >= model.LowerLastSeen);
            }

            if (model.UpperLastSeen.HasValue)
            {
                rtnlist = rtnlist.Where(_ => _.LastSeen <= model.UpperLastSeen);
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }

                else
                {
                    if (model.Status == Constants.Status.ACTIVE)
                    {
                        rtnlist = rtnlist.Where(_ => _.Status == Constants.Status.ACTIVE);
                    }

                    if (model.Status == Constants.Status.INACTIVE)
                    {
                        rtnlist = rtnlist.Where(_ => _.Status == Constants.Status.INACTIVE);
                    }
                }
            }


            var pagedModel = _pagingSupport.From(rtnlist)
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
            LocatorTag locatorTag = _service.GetAllWhere(_ => _.MacAddress == model.MacAddress).FirstOrDefault();

            if (locatorTag is not null)
            {
                return Conflict();
            }

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, locatorTag, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create locator tag") { StatusCode = 403 };
            }*/

            LocatorTag crtLocatorTag = _mapper.Map<LocatorTag>(model);
            DateTime currentDateTime = DateTime.Now;
            crtLocatorTag.UpdateTime = currentDateTime;
            crtLocatorTag.LastSeen = currentDateTime;

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

            if (updLocatorTag == null || id != model.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.MISSING && model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updLocatorTag, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update locator tag with id: {id}") { StatusCode = 403 };
            }

            DateTime currentDateTime = DateTime.Now;

            try
            {
                updLocatorTag.Id = model.Id;
                updLocatorTag.MacAddress = model.MacAddress;
                updLocatorTag.Status = model.Status;
                updLocatorTag.UpdateTime = currentDateTime;
                updLocatorTag.FloorPlanId = model.FloorPlanId;
                updLocatorTag.LocationId = model.LocationId;
                updLocatorTag.LastSeen = model.LastSeen.Value;

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
            LocatorTag locatorTag = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, locatorTag, Operations.Delete);
            
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete locator tag with id: {id}") { StatusCode = 403 };
            }

            if (locatorTag is not null)
            {
                return BadRequest();
            }

            /*if (locatorTag.Status.Equals(Constants.Status.INACTIVE))
            {
                return BadRequest();
            }

            locatorTag.Status = Constants.Status.INACTIVE;*/

            try
            {
                _service.Delete(locatorTag);
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
