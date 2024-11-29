namespace AICTInventoryManagement.Models
{
    public class RequisitionForm
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public DateTime RequisitionDate { get; set; }
        public DateTime RequisitionDeadline { get; set; }
        public List<RequisitionItemViewModel> Items { get; set; }
    }

    public class RequisitionItemViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string MeasurementUnit { get; set; }
        public string Purpose { get; set; }
    }

  
}
