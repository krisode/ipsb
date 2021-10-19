using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Notification
    {
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
