using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Notification
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string Screen { get; set; }
        public string Parameter { get; set; }
        public int AccountId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }

        public virtual Account Account { get; set; }
    }
}
