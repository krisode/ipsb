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
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/coupon-in-uses")]
    [ApiController]
    // [Authorize(Roles = "Building Manager, Visitor, Store Owner")]
    public class CouponInUseController : ControllerBase
    {
        private readonly ICouponInUseService _service;
        private readonly ICouponService _couponService;
        private readonly IMapper _mapper;
        private readonly IPagingSupport<CouponInUse> _pagingSupport;
        private readonly IUploadFileService _uploadFileService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotificationService _notificationService;

        public CouponInUseController(ICouponInUseService service, ICouponService couponService, IMapper mapper, IPagingSupport<CouponInUse> pagingSupport,
            IUploadFileService uploadFileService, IAuthorizationService authorizationService, IPushNotificationService pushNotificationService,
            INotificationService notificationService)
        {
            _service = service;
            _couponService = couponService;
            _mapper = mapper;
            _pagingSupport = pagingSupport;
            _uploadFileService = uploadFileService;
            _authorizationService = authorizationService;
            _pushNotificationService = pushNotificationService;
            _notificationService = notificationService;
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
            var couponInUse = _service.GetByIdAsync(_ => _.Id == id, _ => _.Coupon.Store, _ => _.Visitor).Result;

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
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CouponInUseVM>> GetAllCouponInUses([FromQuery] CouponInUseSM model, int pageSize = 20, int pageIndex = 1, bool isAll = false, bool isAscending = true)
        {
            IQueryable<CouponInUse> list = _service.GetAll(_ => _.Coupon.Store, _ => _.Visitor);

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

            if (model.VisitorId != 0 && model.CouponId != 0 && model.CheckLimit)
            {
                pagedModel.Content = pagedModel.Content.ToList().Select(_ =>
                {
                    _.OverLimit = _service.GetAll().Where(
                        _ => _.CouponId == model.CouponId && _.Status.Equals(Status.ACTIVE)
                        ).Count() >= _.Coupon.Limit;
                    return _;
                }).AsQueryable();
            }

            return Ok(pagedModel);
        }


        /// <summary>
        /// Count coupon in uses
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
        /// <returns>Number of coupon in uses</returns>
        /// <response code="200">Returns number of coupon in uses</response>
        [HttpGet]
        [Route("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CouponInUseVM>> CountCouponInUses([FromQuery] CouponInUseSM model)
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


            return Ok(list.Count());
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

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            /*var authorizedResult = await _authorizationService.AuthorizeAsync(User, couponInUse, Operations.Create);
            if (!authorizedResult.Succeeded)
            {
                return new ObjectResult($"Not authorize to create coupon in use") { StatusCode = 403 };
            }*/

            CouponInUse crtCouponInUse = _mapper.Map<CouponInUse>(model);

            // Default POST Status = "New"
            crtCouponInUse.Status = Constants.Status.NOT_USED;
            crtCouponInUse.RedeemDate = localTime.DateTime;

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

            CouponInUse updCouponInUse = await _service.GetByIdAsync(_ => _.Id == id, _ => _.Coupon.Store.Account);

            if (updCouponInUse is null)
            {
                return BadRequest();
            }

            // var authorizedResult = await _authorizationService.AuthorizeAsync(User, updCouponInUse, Operations.Update);
            // if (!authorizedResult.Succeeded)
            // {
            //     return new ObjectResult($"Not authorize to update coupon in use with id: {id}") { StatusCode = 403 };
            // }

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

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
                    updCouponInUse.FeedbackDate = localTime.DateTime;
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
                        if (!string.IsNullOrEmpty(updCouponInUse.FeedbackContent) && string.IsNullOrEmpty(updCouponInUse.FeedbackReply))
                        {

                            var notification = new Notification();
                            notification.Title = "Feedback on coupon";
                            notification.Body = "Coupon " + updCouponInUse.Coupon.Name + " has just received feedback from customer";
                            notification.ImageUrl = updCouponInUse.Coupon.ImageUrl;
                            notification.Screen = Constants.Route.FEEDBACK;
                            notification.Parameter = "couponId:" + updCouponInUse.CouponId;
                            if (updCouponInUse.Coupon.Store.AccountId != null)
                            {
                                notification.AccountId = (int)updCouponInUse.Coupon.Store.AccountId;
                            }
                            notification.Status = Constants.Status.UNREAD;
                            notification.Date = localTime.DateTime;
                            Notification crtNotification = await _notificationService.AddAsync(notification);
                            if (await _notificationService.Save() > 0)
                            {
                                var data = new Dictionary<String, String>();
                                data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                                data.Add("notificationType", "feedback_changed");
                                data.Add("notificationId", crtNotification.Id.ToString());
                                data.Add("couponId", updCouponInUse.Coupon.Id.ToString());
                                _ = _pushNotificationService.SendMessage(
                                    "Feedback on coupon",
                                    "Coupon " + updCouponInUse.Coupon.Name + " has just received feedback from customer",
                                    "store_id_" + updCouponInUse.Coupon.StoreId,
                                    data
                                    );
                            }

                        }
                        else if (updCouponInUse.Status.Equals(Constants.Status.USED) && string.IsNullOrEmpty(updCouponInUse.FeedbackContent) && string.IsNullOrEmpty(updCouponInUse.FeedbackReply))
                        {
                            var notification = new Notification();
                            notification.Title = "Apply coupon successfully";
                            notification.Body = "You have successfully applied the coupon " + updCouponInUse.Coupon.Name;
                            notification.ImageUrl = updCouponInUse.Coupon.ImageUrl;
                            notification.Screen = Constants.Route.COUPON_DETAIL;
                            notification.Parameter = "couponId:" + updCouponInUse.CouponId;
                            notification.AccountId = updCouponInUse.VisitorId;
                            notification.Status = Constants.Status.UNREAD;
                            notification.Date = localTime.DateTime;
                            var crtNotification = await _notificationService.AddAsync(notification);
                            if (await _notificationService.Save() > 0)
                            {
                                var data = new Dictionary<String, String>();
                                data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                                data.Add("notificationType", "coupon_in_use_changed");
                                data.Add("notificationId", crtNotification.Id.ToString());
                                data.Add("couponInUseId", updCouponInUse.Id.ToString());
                                _ = _pushNotificationService.SendMessage(
                                    "Apply coupon successfully",
                                    "You have successfully applied the coupon " + updCouponInUse.Coupon.Name,
                                    "coupon_in_use_id_" + updCouponInUse.Id,
                                    data
                                    );
                            }
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
        /// <response code="204">Delete coupon in use's status successfully</response>
        /// <response code="400">Coupon in use's id does not exist</response>
        /// <response code="500">Failed to delete</response>
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
