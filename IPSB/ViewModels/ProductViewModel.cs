﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class ProductVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StoreId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string Status { get; set; }
    }
}