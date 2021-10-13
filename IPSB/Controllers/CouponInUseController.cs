﻿using AutoMapper;
using IPSB.AuthorizationHandler;
using IPSB.Core.Services;
using IPSB.ExternalServices;
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
    [Route("api/v1.0/coupon-in-uses")]
    [ApiController]
    // [Authorize(Roles = "Building Manager, Visitor, Store Owner")]
    public class CouponInUseController : Controller
    {
        private readonly ICouponInUseService _service;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<CouponInUse> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPushNotificationService _pushNotificationService;

        public CouponInUseController(ICouponInUseService service, IMapper mapper, IPagingSupport<CouponInUse> pagingSupport, 
            IUploadFileService uploadFileService, IAuthorizationService authorizationService, IPushNotificationService pushNotificationService)
        {
            _service = service;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _pushNotificationService = pushNotificationService;
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

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, couponInUse, Operations.Read);
            if (!authorizedResult.Succeeded)
            {
                return Forbid($"Not authorized to access coupon in use with id: {id}");
            }*/

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
                if (model.Status != Constants.Status.USED && model.Status != Constants.Status.NOT_USED && model.Status != Constants.Status.DELETED)
                {
                    return BadRequest();
                }

                else
                {
                    if (model.Status == Constants.Status.USED)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.USED);
                    }

                    if (model.Status == Constants.Status.NOT_USED)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.NOT_USED);
                    }

                    if (model.Status == Constants.Status.DELETED)
                    {
                        list = list.Where(_ => _.Status == Constants.Status.DELETED);
                    }
                }
            }
            if (model.StoreId != 0)
            {
                list = list.Where(_ => _.Coupon.StoreId == model.StoreId);
            }

            if (model.FeedbackExist)
            {
                list = list.Where(_ => _.RateScore != null);
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

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, couponInUse, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create coupon in use") { StatusCode = 403 };
            }*/

            CouponInUse crtCouponInUse = _mapper.Map<CouponInUse>(model);

            // Default POST Status = "New"
            crtCouponInUse.Status = Constants.Status.NOT_USED;
            crtCouponInUse.RedeemDate = DateTime.Now;

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
        public async Task<ActionResult> PutCouponInUse(int id, [FromForm] CouponInUseUM model)
        {

            CouponInUse updCouponInUse = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Coupon);


            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updCouponInUse, Operations.Update);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update coupon in use with id: {id}") { StatusCode = 403 };
            // }

            try
            {
                bool needUpdate = false;
                if (updCouponInUse.ApplyDate == null)
                {
                    updCouponInUse.ApplyDate = model.ApplyDate;
                    updCouponInUse.Status = Constants.Status.USED;
                    needUpdate = true;
                }
                else if (updCouponInUse.RateScore == null && model.RateScore != null)
                {
                    if (model.ImageUrl != null)
                    {
                        string imageURL = await _uploadFileService.UploadFile("123456798", model.ImageUrl, "coupon-in-use", "feedback-image");
                        updCouponInUse.FeedbackImage = imageURL;
                    }
                    updCouponInUse.FeedbackContent = model.FeedbackContent;
                    updCouponInUse.RateScore = model.RateScore;
                    updCouponInUse.FeedbackDate = DateTime.Now;
                    needUpdate = true;
                }
                else if (updCouponInUse.FeedbackReply == null && model.FeedbackReply != null)
                {
                    updCouponInUse.FeedbackReply = model.FeedbackReply;
                    needUpdate = true;
                }
                if (needUpdate)
                {
                    _service.Update(updCouponInUse);
                    if (await _service.Save() > 0)
                    {
                        if (!string.IsNullOrEmpty(updCouponInUse.FeedbackContent))
                        {
                            var data = new Dictionary<String, String>();
                            data.Add("notiType", "feedback_changed");
                            _ = _pushNotificationService.SendMessage(
                                "Feedback on coupon",
                                "Coupon " + updCouponInUse.Coupon.Name + " has just received feedback from customer",
                                "store_id_" + updCouponInUse.Coupon.StoreId,
                                data
                                );
                        }
                        else if (updCouponInUse.Status.Equals(Constants.Status.USED))
                        {
                            var data = new Dictionary<String, String>();
                            data.Add("notiType", "coupon_in_use_changed");
                            data.Add("couponInUseStatus", updCouponInUse.Status);
                            _ = _pushNotificationService.SendMessage(
                                "Apply discount code successfully",
                                "You have successfully applied the discount code " + updCouponInUse.Coupon.Name,
                                "coupon_in_use_id_" + updCouponInUse.Id,
                                data
                                );
                        }
                    }
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Change the status of coupon in use to deleted
        /// </summary>
        /// <param name="id">Coupon in use's id</param>
        /// <response code="204">Update coupon in use's status successfully</response>
        /// <response code="400">Coupon in use's id does not exist</response>
        /// <response code="500">Failed to update</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult> Delete(int id)
        {
            CouponInUse couponInUse = await _service.GetByIdAsync(_ => _.Id == id);

            var authorizedResult = await _authorizationService.AuthorizeAsync(User, couponInUse, Operations.Delete);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to delete coupon in use with id: {id}") { StatusCode = 403 };
            }

            if (couponInUse is not null)
            {
                return BadRequest();
            }

            if (couponInUse.Status.Equals(Constants.Status.DELETED))
            {
                return BadRequest();
            }

            couponInUse.Status = Constants.Status.INACTIVE;
            try
            {
                _service.Update(couponInUse);
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
