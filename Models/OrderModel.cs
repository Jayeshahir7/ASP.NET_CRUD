﻿using System.ComponentModel.DataAnnotations;

namespace Admin3.Models
{
    public class OrderModel
    {
        public int? OrderID {  get; set; }
        [Required]
        public int  Ordernumber { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public int CustomerID{ get; set; }
        [Required]
        public string PaymentMode { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public string ShippingAddress { get; set; }
        [Required]
        public int UserID { get; set; }
    }
    public class OrderDropDownModel
    {
        public int OrderID { get; set; }
       
    }
}
