using Microsoft.EntityFrameworkCore;

namespace AICTInventoryManagement.Models
{
    [Keyless]
    public class Approver
    {
        public string Department { get; set; } // Department name or code
        public string ApproverRole { get; set; } // Name or role of the approver
    }
}
