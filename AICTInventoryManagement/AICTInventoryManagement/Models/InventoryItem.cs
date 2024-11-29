using Microsoft.EntityFrameworkCore;

namespace AICTInventoryManagement.Models
{
    [Keyless]
    public class InventoryItem
    {
        public string ItemCode { get; set; }
        public string Items { get; set; }
        public int Quantity { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
