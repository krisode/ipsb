using System;

namespace IPSB.ViewModels
{
    public class CouponRefModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; }
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
}
