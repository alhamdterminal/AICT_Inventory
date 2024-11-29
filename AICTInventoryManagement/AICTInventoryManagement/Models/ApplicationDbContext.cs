using Microsoft.EntityFrameworkCore;

namespace AICTInventoryManagement.Models
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor that takes DbContextOptions and passes it to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet representing the 'Requisitions' table in the database
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<SelectedItem> SelectedItems { get; set; }
        public DbSet<Approver> Approvers { get; set; }
        public DbSet<InventoryUpdate> InventoryUpdate { get; set; }
        public DbSet<InventoryHistories> InventoryHistories { get; set; }
        
    }
}
