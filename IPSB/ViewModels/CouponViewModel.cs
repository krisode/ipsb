using IPSB.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class CouponVM
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public CouponType CouponType { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public double Amount { get; set; }
        public double? MaxDiscount { get; set; }
        public double? MinSpend { get; set; }
        public string ProductInclude { get; set; }
        public string ProductExclude { get; set; }
        public int? Limit { get; set; }
        public string Status { get; set; }

        public StoreRefModel Store { get; set; }
        public ICollection<CouponInUseRefModel> CouponInUses { get; set; }
    }
    public class CouponRefModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public CouponType CouponType { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public double Amount { get; set; }
        public double MaxDiscount { get; set; }
        public double MinSpend { get; set; }
        public string ProductInclude { get; set; }
        public string ProductExclude { get; set; }
        public int Limit { get; set; }
        public string Status { get; set; }
    }
    public class CouponSM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public int CouponTypeId { get; set; }
        public DateTime? LowerPublishDate { get; set; }
        public DateTime? UpperPublishDate { get; set; }
        public DateTime? LowerExpireDate { get; set; }
        public DateTime? UpperExpireDate { get; set; }
        public double LowerAmount { get; set; }
        public double UpperAmount { get; set; }
        public double MaxDiscount { get; set; }
        public double MinSpend { get; set; }
        public int BuildingId { get; set; }
        public string[] ProductInclude { get; set; }
        public string[] ProductExclude { get; set; }
        public int LowerLimit { get; set; }
        public int UpperLimit { get; set; }
        public string Status { get; set; }
    }
    public class CouponCM
    {
        public IFormFileCollection ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int StoreId { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public int CouponTypeId { get; set; }
        [Required]
        public DateTime? PublishDate { get; set; }
        [Required]
        public DateTime? ExpireDate { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public double MaxDiscount { get; set; }
        [Required]
        public double MinSpend { get; set; }
        public string[] ProductInclude { get; set; }
        public string[] ProductExclude { get; set; }
        [Required]
        public int Limit { get; set; }
    }
    public class CouponUM
    {
        public int Id { get; set; }
        public IFormFileCollection ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public int CouponTypeId { get; set; }
        //public DateTime ExpireDate { get; set; }
        public double Amount { get; set; }
        public double MaxDiscount { get; set; }
        public double MinSpend { get; set; }
        public int[] ProductInclude { get; set; }
        public int[] ProductExclude { get; set; }
        public int Limit { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
