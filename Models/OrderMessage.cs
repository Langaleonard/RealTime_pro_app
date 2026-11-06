using System.ComponentModel.DataAnnotations;

namespace ABCRetail.Models
{
    public class OrderMessage
    {
        [Required]
        [Display(Name = "Order ID")]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        public string Status { get; set; } = "Processing";
    }
}