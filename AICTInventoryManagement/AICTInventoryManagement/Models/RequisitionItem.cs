namespace AICTInventoryManagement.Models
{
    public class RequisitionItem
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Department { get; set; }
        public DateTime? RequisitionDate { get; set; }
        public DateTime? RequisitionDeadline { get; set; } 
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? Quantity { get; set; }
        public string? MeasurementUnit { get; set; }
        public string? Purpose { get; set; }
        public string? RequisitionCode { get; set; }
        public string? Status { get; set; }
        public string? PendingBy { get; set; }

        public string? Comments { get; set; }
    }
}
