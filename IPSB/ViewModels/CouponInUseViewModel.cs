using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class CouponInUseVM
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VisitorId { get; set; }
        public DateTime RedeemDate { get; set; }
        public DateTime ApplyDate { get; set; }
        public string Status { get; set; }
        public string FeedbackContent { get; set; }
        public string FeedbackReply { get; set; }
        public string FeedbackImage { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public double? RateScore { get; set; }
        public CouponRefModel Coupon { get; set; }
        public AccountRefModel Visitor { get; set; }
    }
    public class CouponInUseRefModel
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VisitorId { get; set; }
        public DateTime RedeemDate { get; set; }
        public DateTime ApplyDate { get; set; }
        public string Status { get; set; }
    }
    public class CouponInUseSM
    {
        public int CouponId { get; set; }
        public int StoreId { get; set; }
        public bool FeedbackExist { get; set; }
        public int VisitorId { get; set; }
        public DateTime? LowerRedeemDate { get; set; }
        public DateTime? UpperRedeemDate { get; set; }
        public DateTime? LowerApplyDate { get; set; }
        public DateTime? UpperApplyDate { get; set; }
        public string Status { get; set; }
    }
    public class CouponInUseCM
    {
        [Required]
        public int CouponId { get; set; }
        [Required]
        public int VisitorId { get; set; }
        [Required]
        public DateTime RedeemDate { get; set; }
    }
    public class CouponInUseUM
    {
        public DateTime? ApplyDate { get; set; }
        public string FeedbackContent { get; set; }
        public string FeedbackReply { get; set; }
        public IFormFile ImageUrl { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public double? RateScore { get; set; }
    }
}
