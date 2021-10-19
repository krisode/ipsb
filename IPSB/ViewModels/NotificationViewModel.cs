using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class NotificationVM
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

    }
    public class NotificationRefModel
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
    }

    public class NotificationSM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string Screen { get; set; }
        public string Parameter { get; set; }
        public int AccountId { get; set; }
        public string Status { get; set; }
        public DateTime? LowerDate { get; set; }
        public DateTime? UpperDate { get; set; }
    }
    public class NotificationCM
    {
        [Required]
        public string Content { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string Parameter { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
    public class NotificationUM
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
    }
}
