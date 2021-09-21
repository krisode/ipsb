using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
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
    [Route("api/v1.0/edges")]
    [ApiController]
    [Authorize(Roles = "Building Manager, Visitor")]
    public class EdgeController : Controller
    {
        private readonly IEdgeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Edge> _pagingSupport;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStore _cacheStore;

        public EdgeController(IEdgeService service, IMapper mapper, IPagingSupport<Edge> pagingSupport,
            IAuthorizationService authorizationService, ICacheStore cacheStore)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _authorizationService = authorizationService;
            _cacheStore = cacheStore;
        }

        /// <summary>
        /// Get a specific edge by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the edge with the corresponding id</returns>
        /// <response code="200">Returns the edge with the specified id</response>
        /// <response code="404">No edges found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<EdgeVM>> GetEdgeById(int id)
        {
            var cacheId = new CacheKey<Edge>(id);
            var cacheObjectType = new Edge();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];

            try
            {
                var edge = await _cacheStore.GetOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var edge = _service.GetByIdAsync(_ => _.Id == id,
                        _ => _.FromLocation,
                        _ => _.ToLocation,
                        _ => _.FromLocation.Store,
                        _ => _.ToLocation.Store).Result;


                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(edge);

                }, ifModifiedSince);


                if (edge == null)
                {
                    return NotFound();
                }

                /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, building, Operations.Read);
                if (!authorizedResult.Succeeded)
                {
                    return new ObjectResult($"Not authorize to access building with id: {id}") { StatusCode = 403 };
                }*/

                var rtnEdge = _mapper.Map<EdgeVM>(edge);

                return Ok(rtnEdge);
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
        /// Get all edges
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
        /// <returns>All edges</returns>
        /// <response code="200">Returns all edges</response>
        /// <response code="404">No edges found</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EdgeVM>>> GetAllEdges([FromQuery] EdgeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var cacheId = new CacheKey<EdgeVM>(Utils.Constants.DefaultValue.INTEGER);
            var cacheObjectType = new EdgeVM();
            var ifModifiedSince = Request.Headers[Constants.Request.IF_MODIFIED_SINCE];
            try
            {
                // var list = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                // {
                //     var list = _service.GetAll(_ => _.FromLocation.FloorPlan,
                //         _ => _.ToLocation.FloorPlan,
                //         _ => _.FromLocation.Store,
                //         _ => _.ToLocation.Store);

                //     Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                //     return Task.FromResult(list);

                // }, ifModifiedSince);

                var list = _service.GetAll(_ => _.FromLocation.FloorPlan,
                        _ => _.ToLocation.FloorPlan,
                        _ => _.FromLocation.Store,
                        _ => _.ToLocation.Store);

                if (model.FromLocationId != 0)
                {
                    list = list.Where(_ => _.FromLocationId == model.FromLocationId);
                }

                if (model.ToLocationId != 0)
                {
                    list = list.Where(_ => _.ToLocationId == model.ToLocationId);
                }

                if (model.LowerDistance != 0)
                {
                    list = list.Where(_ => _.Distance >= model.LowerDistance);
                }

                if (model.UpperDistance != 0)
                {
                    list = list.Where(_ => _.Distance <= model.UpperDistance);
                }

                if (model.FloorPlanId != 0)
                {
                    list = list.Where(_ => _.FromLocation.FloorPlanId == model.FloorPlanId || _.ToLocation.FloorPlanId == model.FloorPlanId);
                }

                if (model.BuildingId != 0)
                {
                    list = list.Where(_ => _.FromLocation.FloorPlan.BuildingId == model.BuildingId);

                }

                var data = await _cacheStore.GetAllOrSetAsync(cacheObjectType, cacheId, func: (cachedItemTime) =>
                {
                    var pagedModel = _pagingSupport.From(list)
                            .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                            .Paginate<EdgeVM>();

                    Response.Headers.Add(Constants.Response.LAST_MODIFIED, cachedItemTime);

                    return Task.FromResult(pagedModel);

                }, ifModifiedSince);


                return Ok(data);
            }
            catch (Exception e)
            {
                if (e.Message.Equals(Constants.ExceptionMessage.NOT_MODIFIED))
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            /*
                        IQueryable<Edge> list = _service.GetAll(_ => _.FromLocation.FloorPlan, _ => _.ToLocation.FloorPlan, _ => _.FromLocation.Store, _ => _.ToLocation.Store);
            */
        }

        /// <summary>
        /// Create a new edge
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "FromLocationId": "The location's id that is the beginning of the edge",   
        ///         "ToLocationId": "The location's id that is the end of the edge",   
        ///         "Distance": "The distance between two locations",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new edge</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateEdge([FromBody] List<EdgeCM> listModel)
        {
            List<Edge> list = listModel.Select(model => _mapper.Map<Edge>(model)).ToList();
            try
            {
                await _service.AddRangeAsync(list);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var rtnEdgeIds = list.Select(_ => _.Id);
            return CreatedAtAction("CreateEdge", rtnEdgeIds);
        }

        /// <summary>
        /// Update edge with specified id
        /// </summary>
        /// <param name="id">Edge's id</param>
        /// <param name="model">Information applied to updated edge</param>
        /// <response code="204">Update edge successfully</response>
        /// <response code="400">Edge's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutEdge(int id, [FromBody] EdgeUM model)
        {
            Edge updEdge = await _service.GetByIdAsync(_ => _.Id == id);

            if (updEdge == null || id != model.Id)
            {
                return BadRequest();
            }

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, updEdge, Operations.Update);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to update edge with id: {id}") { StatusCode = 403 };
            }

            try
            {
                updEdge.Id = model.Id;
                updEdge.FromLocationId = model.FromLocationId;
                updEdge.ToLocationId = model.ToLocationId;
                updEdge.Distance = model.Distance;
                _service.Update(updEdge);
                if (await _service.Save() > 0)
                {
                    #region Updating cache
                    var cacheId = new CacheKey<Edge>("");
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



        // DELETE api/<EdgeController>?id=1&id=3
        // Change Status to Inactive
        [HttpDelete]
        public async Task<ActionResult> DeleteRange([FromBody] EdgeDM model)
        {
            try
            {
                _service.DeleteRange(model.Ids);
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
