namespace AICTInventoryManagement.Models
{
    public class ApprovalRequisitionViewModel
    {
        public RequisitionItem Requisition { get; set; }
        public List<RequisitionItem> RequisitionItems { get; set; }
        public List<InventoryItem> InventoryItems { get; set; }
    }
}
 