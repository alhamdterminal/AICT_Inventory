using Microsoft.EntityFrameworkCore;

namespace AICTInventoryManagement.Models
{
    [Keyless]
    public class SelectedItem
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public int Quantity { get; set; }
    }
    [Keyless]
    public class InventoryUpdate
    {
        public int? ItemCode { get; set; }
        public int NewQuantity { get; set; }
    }
    
}
