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
        [Required]
        public string Status { get; set; }
    }
    public class CouponInUseUM
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VisitorId { get; set; }
        public DateTime? RedeemDate { get; set; }
        public DateTime? ApplyDate { get; set; }
        public string Status { get; set; }
    }
}
