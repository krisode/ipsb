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
    [Route("api/v1.0/visit-points")]
    [ApiController]
    public class VisitPointController : AuthorizeController
    {
        private readonly IVisitPointService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<VisitPoint> _pagingSupport;

        public VisitPointController(IVisitPointService service, IMapper mapper, IPagingSupport<VisitPoint> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific visit point by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the visit point with the corresponding id</returns>
        /// <response code="200">Returns the visit point with the specified id</response>
        /// <response code="404">No visit points found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<VisitPointVM> GetVisitPointById(int id)
        {
            var visitPoint = _service.GetByIdAsync(_ => _.Id == id, _ => _.Location, _ => _.VisitRoute).Result;

            if (visitPoint is null)
            {
                return NotFound();
            }

            var rtnVisitPoint = _mapper.Map<VisitPointVM>(visitPoint);

            return Ok(rtnVisitPoint);
        }

        /// <summary>
        /// Get all visit points
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
        /// <returns>All visit points</returns>
        /// <response code="200">Returns all visit points</response>
        /// <response code="404">No visit points found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<VisitPointVM>> GetAllVisitPoints([FromQuery] VisitPointSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<VisitPoint> list = _service.GetAll(_ => _.Location, _ => _.VisitRoute);

            if (model.LocationId != 0)
            {
                list = list.Where(_ => _.LocationId == model.LocationId);
            }

            if (model.VisitRouteId != 0)
            {
                list = list.Where(_ => _.VisitRouteId == model.VisitRouteId);
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
                .Paginate<VisitPointVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new visit point
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "LocationId": "Id of the location",   
        ///         "VisitRouteId": "Id of the visit route",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new visit point</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VisitPointCM>> CreateVisitPoint([FromBody] VisitPointCM model)
        {
            
            VisitPoint crtVisitPoint = _mapper.Map<VisitPoint>(model);
            DateTime currentDateTime = DateTime.Now;
            crtVisitPoint.RecordTime = currentDateTime;

            try
            {
                await _service.AddAsync(crtVisitPoint);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetVisitPointById", new { id = crtVisitPoint.Id }, crtVisitPoint);
        }

        /// <summary>
        /// Update visit point with specified id
        /// </summary>
        /// <param name="id">Visit point's id</param>
        /// <param name="model">Information applied to updated visit point</param>
        /// <response code="204">Update visit point successfully</response>
        /// <response code="400">Visit point's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutVisitPoint(int id, [FromBody] VisitPointUM model)
        {

            VisitPoint updVisitPoint = await _service.GetByIdAsync(_ => _.Id == id);

            if (updVisitPoint == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                updVisitPoint.Id = model.Id;
                updVisitPoint.LocationId = model.LocationId;
                updVisitPoint.VisitRouteId = model.VisitRouteId;
                updVisitPoint.RecordTime = model.RecordTime.Value;
                
                _service.Update(updVisitPoint);
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
