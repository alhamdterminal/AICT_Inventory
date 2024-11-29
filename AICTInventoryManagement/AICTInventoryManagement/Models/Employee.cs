using Microsoft.EntityFrameworkCore;

namespace AICTInventoryManagement.Models
{
    [Keyless]
    public class Employee
    {
        public String EmpCode { get; set; }
        public String Empname { get; set; }
        public String Department { get; set; }
        public string Password { get; set; }
        public string role { get; set; }
    }
}
