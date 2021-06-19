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
    [Route("api/v1.0/visit-routes")]
    [ApiController]
    public class VisitRouteController : AuthorizeController
    {
        private readonly IVisitRouteService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<VisitRoute> _pagingSupport;

        public VisitRouteController(IVisitRouteService service, IMapper mapper, IPagingSupport<VisitRoute> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }


        /// <summary>
        /// Get a specific visit route by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the visit route with the corresponding id</returns>
        /// <response code="200">Returns the visit route with the specified id</response>
        /// <response code="404">No visit routes found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<VisitRouteVM> GetVisitRouteById(int id)
        {
            var visitRoute = _service.GetByIdAsync(_ => _.Id == id, _ => _.Account, _ => _.Building, _ => _.VisitPoints).Result;

            if (visitRoute is null)
            {
                return NotFound();
            }

            var rtnVisitRoute = _mapper.Map<VisitRouteVM>(visitRoute);

            return Ok(rtnVisitRoute);
        }

        /// <summary>
        /// Get all visit routes
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
        /// <returns>All visit routes</returns>
        /// <response code="200">Returns all visit routes</response>
        /// <response code="404">No visit routes found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<VisitRouteVM>> GetAllVisitRoutes([FromQuery] VisitRouteSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<VisitRoute> list = _service.GetAll(_ => _.Account, _ => _.Building, _ => _.VisitPoints);

            if (model.AccountId != 0)
            {
                list = list.Where(_ => _.AccountId == model.AccountId);
            }

            if (model.BuildingId != 0)
            {
                list = list.Where(_ => _.BuildingId == model.BuildingId);
            }

            if (model.LowerRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime >= model.LowerRecordTime);
            }

            if (model.UpperRecordTime.HasValue)
            {
                list = list.Where(_ => _.RecordTime <= model.UpperRecordTime);
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<VisitRouteVM>();

            return Ok(pagedModel);
        }


        /// <summary>
        /// Create a new visit route
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "AccountId": "Id of the account visiting the route",   
        ///         "BuildingId": "Id of the visit building that the route belongs to",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new visit route</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VisitRouteCM>> CreateRoute([FromBody] VisitRouteCM model)
        {

            VisitRoute crtVisitRoute = _mapper.Map<VisitRoute>(model);
            DateTime currentDateTime = DateTime.Now;
            crtVisitRoute.RecordTime = currentDateTime;

            try
            {
                await _service.AddAsync(crtVisitRoute);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetVisitRouteById", new { id = crtVisitRoute.Id }, crtVisitRoute);
        }

        /// <summary>
        /// Update visit route with specified id
        /// </summary>
        /// <param name="id">Visit route's id</param>
        /// <param name="model">Information applied to updated visit route</param>
        /// <response code="204">Update visit route successfully</response>
        /// <response code="400">Visit route's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutVisitRoute(int id, [FromBody] VisitRouteUM model)
        {

            VisitRoute updVisitRoute = await _service.GetByIdAsync(_ => _.Id == id);

            if (updVisitRoute == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                updVisitRoute.Id = model.Id;
                updVisitRoute.AccountId = model.AccountId;
                updVisitRoute.BuildingId = model.BuildingId;
                updVisitRoute.RecordTime = model.RecordTime.Value;

                _service.Update(updVisitRoute);
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
