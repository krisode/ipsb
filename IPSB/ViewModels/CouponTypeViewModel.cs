using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class CouponTypeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class CouponTypeSM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class CouponTypeCM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }

    public class CouponTypeUM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}