using System.ComponentModel.DataAnnotations;

namespace Admin3.Models
{
    public class CustomerModel
    {
        public int? CustomerID { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string HomeAddress { get; set; }
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [Phone]
        [MinLength(10)]
        [MaxLength(10)]
        public string MobileNo { get; set; }
        [Required]
        public string GST_NO { get; set; }
        [Required]
        public string CityName { get; set; }
        [Required]
        [MinLength (6)]
        public string Pincode { get; set; }
        [Required]
        public decimal NetAmount { get; set; }
        [Required]
        public int UserID { get; set; }
    }
    public class CustomerDropDownModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
    }
}
