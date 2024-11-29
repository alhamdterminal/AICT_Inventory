using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AICTInventoryManagement.Models
{
    
    public class Inventory
    {
        [Key]
        public int Id { get; set; }
        public string ItemCode { get; set; }
        public string Items { get; set; }
        public int Quantity { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
