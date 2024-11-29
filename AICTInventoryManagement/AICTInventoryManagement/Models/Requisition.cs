namespace AICTInventoryManagement.Models
{
    public class Requisition
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public DateTime RequisitionDate { get; set; }
        public DateTime Deadline { get; set; }
        public string ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string Supplier { get; set; }
        public string Category { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string LeadTime { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string Location { get; set; }
    }
}
