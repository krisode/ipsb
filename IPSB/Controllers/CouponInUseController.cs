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
    [Route("api/v1.0/coupon-in-uses")]
    [ApiController]
    public class CouponInUseController : AuthorizeController
    {
        private readonly ICouponInUseService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<CouponInUse> _pagingSupport;

        public CouponInUseController(ICouponInUseService service, IMapper mapper, IPagingSupport<CouponInUse> pagingSupport)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
        }

        /// <summary>
        /// Get a specific coupon in use by id
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     GET {
        ///         "id" : "1"
        ///     }
        /// </remarks>
        /// <returns>Return the coupon in use with the corresponding id</returns>
        /// <response code="200">Returns the coupon in use with the specified id</response>
        /// <response code="404">No coupon in uses found with the specified id</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public ActionResult<CouponInUseVM> GetCouponInUseById(int id)
        {
            var couponInUse = _service.GetByIdAsync(_ => _.Id == id, _ => _.Coupon, _ => _.Visitor).Result;

            if (couponInUse == null)
            {
                return NotFound();
            }

            var rtnCouponInUse = _mapper.Map<CouponInUseVM>(couponInUse);

            return Ok(rtnCouponInUse);
        }

        /// <summary>
        /// Get all coupon in uses
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
        /// <returns>All coupon in uses</returns>
        /// <response code="200">Returns all coupon in uses</response>
        /// <response code="404">No coupon in uses found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CouponInUseVM>> GetAllCouponInUses([FromQuery] CouponInUseSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<CouponInUse> list = _service.GetAll(_ => _.Coupon, _ => _.Visitor);

            if (model.CouponId != 0)
            {
                list = list.Where(_ => _.CouponId == model.CouponId);
            }

            if (model.VisitorId != 0)
            {
                list = list.Where(_ => _.VisitorId == model.VisitorId);
            }

            if (model.LowerRedeemDate.HasValue)
            {
                list = list.Where(_ => _.RedeemDate >= model.LowerRedeemDate);
            }

            if (model.UpperRedeemDate.HasValue)
            {
                list = list.Where(_ => _.RedeemDate <= model.UpperRedeemDate);
            }

            if (model.LowerApplyDate.HasValue)
            {
                list = list.Where(_ => _.ApplyDate >= model.LowerApplyDate);
            }

            if (model.UpperApplyDate.HasValue)
            {
                list = list.Where(_ => _.ApplyDate <= model.UpperApplyDate);
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
                        list = list.Where(_ => _.Status == Constants.Status.ACTIVE);
                    }

                    if (model.Status == Constants.Status.INACTIVE)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.INACTIVE);
                    }
                }
            }

            var pagedModel = _pagingSupport.From(list)
                .GetRange(pageIndex, pageSize, _ => _.Id, isAll, isAscending)
                .Paginate<CouponInUseVM>();

            return Ok(pagedModel);
        }

        /// <summary>
        /// Create a new coupon in use
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST 
        ///     {
        ///         "CouponId": "Id of the coupon",   
        ///         "VisitorId": "Id of the visitor",   
        ///         "RedeemDate": "The date time that user saved",
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Created a new coupon in use</response>
        /// <response code="409">Coupon in use already exists</response>
        /// <response code="500">Failed to save request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CouponInUseCM>> CreateCouponInUse([FromBody] CouponInUseCM model)
        {
            CouponInUse couponInUse = _service.GetAllWhere(_ => _.CouponId == model.CouponId, _ => _.VisitorId == model.VisitorId).FirstOrDefault();
            if (couponInUse is not null)
            {
                return Conflict();
            }

            //if (!string.IsNullOrEmpty(model.Status))
            //{
            //    if (model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
            //    {
            //        return BadRequest();
            //    }
            //}

            CouponInUse crtCouponInUse = _mapper.Map<CouponInUse>(model);

            // Default POST Status = "New"
            crtCouponInUse.Status = Constants.Status.NEW;

            try
            {
                await _service.AddAsync(crtCouponInUse);
                await _service.Save();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetCouponInUseById", new { id = crtCouponInUse.Id }, crtCouponInUse);
        }

        /// <summary>
        /// Update coupon in use with specified id
        /// </summary>
        /// <param name="id">Coupon in use's id</param>
        /// <param name="model">Information applied to updated coupon in use</param>
        /// <response code="204">Update coupon in use successfully</response>
        /// <response code="400">Coupon in use's id does not exist or does not match with the id in parameter</response>
        /// <response code="500">Failed to update</response>
        [HttpPut]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PutCouponInUse(int id, [FromBody] CouponInUseUM model)
        {

            CouponInUse updCouponInUse = await _service.GetByIdAsync(_ => _.Id == id);

            if (updCouponInUse == null || id != model.Id)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(model.Status))
            {
                if (model.Status != Constants.Status.NEW && model.Status != Constants.Status.ACTIVE && model.Status != Constants.Status.INACTIVE)
                {
                    return BadRequest();
                }
            }

            if (updCouponInUse.CouponId == model.CouponId && updCouponInUse.VisitorId == updCouponInUse.VisitorId)
            {
                try
                {
                    updCouponInUse.Id = model.Id;
                    updCouponInUse.CouponId = model.CouponId;
                    updCouponInUse.VisitorId = model.VisitorId;
                    updCouponInUse.RedeemDate = model.RedeemDate.Value;
                    updCouponInUse.ApplyDate = model.ApplyDate.Value;
                    updCouponInUse.Status = model.Status;

                    _service.Update(updCouponInUse);
                    await _service.Save();
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            } else
            {
                    return BadRequest();
           
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
