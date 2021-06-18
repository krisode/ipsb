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
    [Route("api/v1.0/edges")]
    [ApiController]
    public class EdgeController : AuthorizeController
    {
        private readonly IEdgeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<Edge> _pagingSupport;

        public EdgeController(IEdgeService service, IMapper mapper, IPagingSupport<Edge> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
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
        public ActionResult<EdgeVM> GetEdgeById(int id)
        {
            var edge = _service.GetByIdAsync(_ => _.Id == id, _ => _.FromLocation, _ => _.ToLocation);

            if (edge == null)
            {
                return NotFound();
            }

            var rtnEdge = _mapper.Map<EdgeVM>(edge.Result);

            return Ok(rtnEdge);
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
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<EdgeVM>> GetAllEdges([FromQuery] EdgeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<Edge> list = _service.GetAll(_ => _.FromLocation, _ => _.ToLocation);

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
                list = list.Where(_ => _.FromLocation.FloorPlanId == model.FloorPlanId);
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<EdgeVM>();

            return Ok(pagedModel);
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
        public async Task<ActionResult<EdgeCM>> CreateEdge([FromBody] EdgeCM model)
        {
            Edge crtEdge = _mapper.Map<Edge>(model);

            try
            {
                await _service.AddAsync(crtEdge);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetEdgeById", new { id = crtEdge.Id }, crtEdge);
        }

        /// <summary>
        /// Update edge with specified id
        /// </summary>
        /// <param name="id">Edge's id</param>
        /// <param name="model">Information applied to updated edge</param>
        /// <response code="204">Update edge successfully</response>
        /// <response code="400">>Edge's id does not exist or does not match with the id in parameter</response>
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

            try
            {
                updEdge.Id = model.Id;
                updEdge.FromLocationId = model.FromLocationId;
                updEdge.ToLocationId = model.ToLocationId;
                updEdge.Distance = model.Distance;
                _service.Update(updEdge);
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
