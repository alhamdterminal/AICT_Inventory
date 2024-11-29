using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AICTInventoryManagement.Models
{
   
    public class InventoryHistories
    {
        [Key]
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int RequiredQuantity { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string? Action { get; set; }
        public string? UserRole { get; set; }
        public string? RequisitionCode { get; set; }
       
        public int? AvailableStock { get; set; }
        public int? PreviousStock { get; set; }
        public string? ProductCode { get; set; }
    }
}
