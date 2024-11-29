using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AICTInventoryManagement.Models
{
    
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public String ProductCode { get; set; }
        public String ProductName { get; set; }
        public String MeasurementUnit { get; set; }
    }
}
