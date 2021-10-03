using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IPSB.Infrastructure.Contexts;
using IPSB.Utils;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IPSB.Controllers
{
    [Route("api/v1.0/coupon-types")]
    [ApiController]
    public class CouponTypeController : Controller
    {
        private readonly ICouponTypeService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<CouponType> _pagingSupport;

        public CouponTypeController(ICouponTypeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a specific coupon type by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the coupon type with the corresponding id</returns>
        /// <response code="200">Returns the coupon type with the specified id</response>
        /// <response code="404">No coupon type found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCouponTypeById(int id)
        {
            var result = await _service.GetByIdAsync(_ => _.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            var rtnCouponType = _mapper.Map<CouponTypeVM>(result);
            return Ok(rtnCouponType);
        }

        /// <summary>
        /// Get all coupon types
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET 
        ///     {
        ///         "name": "Fixed",
        ///         "description": "Discount total price"
        ///     }
        ///
        /// </remarks>
        /// <returns>All coupon types</returns>
        /// <response code="200">Returns all coupon types</response>
        /// <response code="404">No coupon types found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult GetAllCouponTypes([FromQuery] CouponTypeSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            var couponTypeList = _service.GetAll();

            if (!string.IsNullOrEmpty(model.Name))
            {
                couponTypeList = couponTypeList.Where(_ => _.Name.Contains(model.Name));
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                couponTypeList = couponTypeList.Where(_ => _.Description.Contains(model.Description));
            }

            var pagedModel = _pagingSupport.From(couponTypeList)
                                            .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                                            .Paginate<CouponTypeVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new coupon type
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "name": "Coupon type's name",   
        ///         "description": "Coupon type's description",   
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created new coupon type</response>
        /// <response code="400">Required create data is missing</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CouponTypeVM>> CreateCouponType([FromBody] CouponTypeCM model)
        {
            var createdCouponType = _mapper.Map<CouponType>(model);

            try
            {
                await _service.AddAsync(createdCouponType);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("CreateCouponType", new { id = createdCouponType.Id }, createdCouponType);
        }

        /// <summary>
        /// Update coupon type with specified id
        /// </summary>
        /// <param name="id">Coupon type's id</param>
        /// <param name="model">Information applied to updated coupon type</param>
        /// <response code="204">Update coupon type successfully</response>
        /// <response code="400">Required update data is missing</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateCouponType(int id, [FromBody] CouponTypeUM model)
        {
            var updateCouponType = await _service.GetByIdAsync(_ => _.Id == id);

            try
            {
                updateCouponType.Name = model.Name;
                updateCouponType.Description = model.Description;
                _service.Update(updateCouponType);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Delete coupon type with specified id
        /// </summary>
        /// <param name="id">Coupon type's id</param>
        /// <response code="204">Delete coupon type successfully</response>
        /// <response code="500">Failed to delete</response>
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int id)
        {
            var updateCouponType = await _service.GetByIdAsync(_ => _.Id == id);

            try
            {
                updateCouponType.Status = Constants.Status.INACTIVE;
                _service.Update(updateCouponType);
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