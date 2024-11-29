using AICTInventoryManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace AICTInventoryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string empId, string password)
        {
            
            var employee = _context.Employees
                .FirstOrDefault(e => e.EmpCode == empId && e.Password == password);

            if (employee != null)
            {
             
                HttpContext.Session.SetString("EmpName", employee.Empname);

                
                HttpContext.Session.SetString("EmpCode", employee.EmpCode);

                HttpContext.Session.SetString("role", employee.role);

               
                return RedirectToAction("PendingList", "Home");
            }

          
            ViewBag.ErrorMessage = "Invalid Employee ID or Password!";
            return View("Login");
        }
        public IActionResult Logout()
        {
           
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
        public IActionResult Index()
        {

            return View();
        }
    

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult InventoryList()
        {
            var InventoryItems = _context.Inventory.ToList();
            return View(InventoryItems);
        }
        public IActionResult CheckInventoryHistory()
        {
            var products = _context.Products
                .Select(p => new { p.ProductCode, p.ProductName })
                .ToList();

            // Pass the products list to the view
            return View(products);
        }
        public IActionResult SearchInventoryHistory(string productCode)
        {
            // Querying InventoryHistories and joining with Products to get the ProductName
            var history = _context.InventoryHistories
                .Where(e => e.ProductCode == productCode)
                .OrderByDescending(e => e.Id) // Ordering by the latest entry
                .ToList();

            // Getting the ProductName from the Products table based on ProductCode
            var productName = _context.Products
                .Where(p => p.ProductCode == productCode)
                .Select(p => p.ProductName)
                .FirstOrDefault();  // Returns the first match or null if not found

            // Pass the ProductName to the view using ViewBag
            ViewBag.ProductName = productName;

            // Return the view with the inventory history
            return View(history);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
          {
            // Find the item by Id (you should replace this with actual database logic)
            var item = _context.Inventory.FirstOrDefault(i => i.Id == id);
            var stock = item.Quantity;
            if (item != null)
            {
                // Update the quantity
                item.Quantity = quantity;

                // Save changes to the database
                _context.SaveChanges();

                

         

                var reqQuantity = item.Quantity - stock;
                    
                    var inventoryHistory = new InventoryHistories
                    {
                        ItemId = id,  // Same ItemId
                        RequiredQuantity = +reqQuantity, // Negative because quantity is being deducted
                        ChangeDate = DateTime.Now,  // Current time for when the change occurs
                        Action = "Admin Added Inventory Stock",  // Action description
                        UserRole = "Admin",  // User role passed in
                        RequisitionCode = "-",  // Requisition code passed in
                        AvailableStock = item.Quantity,  // New available stock after deduction
                        PreviousStock = stock,  // Previous stock before deduction
                        ProductCode = item.ItemCode  // Use the ProductCode from the latest history record
                    };

                    // Step 3: Save the new InventoryHistory record to the database.
                    _context.InventoryHistories.Add(inventoryHistory);
                    _context.SaveChanges();
                


                return RedirectToAction("InventoryList");
            }

            return Json(new { success = false });
        }
        public IActionResult ManageInventory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProductInventory(ProductInventoryViewModel model)
          {
            if (ModelState.IsValid)
            {
                var lastprod = _context.Products.OrderByDescending(p => p.Id).Select(p => p.ProductCode).FirstOrDefault();
                string newProductCode = GenerateNextCode(lastprod);

                var lastInventory = _context.Inventory.OrderByDescending(p => p.Id).Select(p => p.ItemCode).FirstOrDefault();
                string newItemCode = GenerateNextCode(lastInventory);

                var product = new Product
                {
                    ProductCode = newProductCode,
                    ProductName = model.ProductName,
                    MeasurementUnit = model.MeasurementUnit
                };
                _context.Products.Add(product);

                var inventory = new Inventory
                {
                    ItemCode = newItemCode,
                    Items = model.ProductName,
                    Quantity = model.Quantity,
                    MeasurementUnit = model.MeasurementUnit
                };
                _context.Inventory.Add(inventory);

                await _context.SaveChangesAsync();

                // Return a success response
                return Json(new { success = true, message = "Inventory successfully added." });
            }

            // Return a failure response if model validation fails
            return Json(new { success = false, message = "There was an error adding the inventory." });
        }



        private string GenerateNextCode(string lastCode)
        {
            // If no code exists (first entry), return "0001"
            if (string.IsNullOrEmpty(lastCode))
            {
                return "0001";
            }

            // Convert the numeric part of the last code (assumes it's a string like "0001")
            if (int.TryParse(lastCode, out int number))
            {
                // Increment the number and format it with leading zeros (4 digits)
                number++;
                return number.ToString("D4"); // Format to always have 4 digits (e.g., "0002")
            }

            // In case the last code format is unexpected, return "0001"
            return "0001";
        }
        public IActionResult Requisition()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetEmployeeDetails(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
            {
                return Json(new { success = false, message = "Employee code is required" });
            }

            var employee = _context.Employees
                .Where(e => e.EmpCode == employeeCode)
                .FirstOrDefault();

            if (employee != null)
            {
                return Json(new
                {
                    success = true,
                    empCode = employee.EmpCode,
                    empname = employee.Empname,
                    department = employee.Department
                });
            }
            else
            {
                return Json(new { success = false, message = "Employee not found" });
            }
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _context.Products
                .Select(p => new { p.ProductCode, p.ProductName }) 
                .ToList();

            return Json(products);
        }

        [HttpGet]
        public IActionResult GetProductDetails(string productCode)
        {
            var product = _context.Products
                .Where(p => p.ProductCode == productCode)
                .Select(p => new { p.ProductCode, p.ProductName,p.MeasurementUnit })
                .FirstOrDefault();

            if (product != null)
            {
                return Json(product);
            }

            return NotFound();
        }


        public IActionResult Create()
        {
           
            var products = _context.Products
                                     .Select(p => new { p.ProductCode, p.ProductName })
                                     .ToList();

            if (products != null)
            {
                ViewData["Products"] = products;
            }
            else
            {
                ViewData["Products"] = new List<dynamic>();  
            }

            return View();
        }

        [HttpPost]
        public IActionResult Submit(RequisitionForm model)
        {
            // Get the last requisition code and generate the new one
            var lastRequisitionCode = _context.RequisitionItems
                                               .OrderByDescending(r => r.RequisitionCode)
                                               .Select(r => r.RequisitionCode)
                                               .FirstOrDefault();

            string requisitionCode;
            if (string.IsNullOrEmpty(lastRequisitionCode))
            {
                requisitionCode = "0001";
            }
            else
            {
                int lastCodeNumber = int.Parse(lastRequisitionCode);
                requisitionCode = (lastCodeNumber + 1).ToString("D4");
            }

            string empCode = HttpContext.Session.GetString("EmpCode");

            // Get the role and department of the logged-in user
            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            var userDepartment = _context.Employees
                                         .Where(e => e.EmpCode == empCode)
                                         .Select(e => e.Department)
                                         .FirstOrDefault();

            // Determine the PendingBy field logic
            string pendingBy = string.Empty;

            // If the user is an Admin, PendingBy should be "Complete"
            if (userRole == "Admin")
            {
                pendingBy = "Complete";
            }
            else
            {
                // Get the approver for the department from the Approvers table
                var approver = _context.Approvers
                                       .Where(a => a.Department == userDepartment)
                                       .Select(a => a.ApproverRole) // Assuming 'ApproverName' is the column for the approver's name or role
                                       .FirstOrDefault();

                // If an approver is found for the department, use it; otherwise, fallback to "LineManager"
                pendingBy = approver ?? "LineManager"; // Default to "LineManager" if no approver is found
            }

            // Now, create and add the requisition items to the database
            foreach (var item in model.Items)
            {
                var requisitionItem = new RequisitionItem
                {
                    EmployeeCode = model.EmployeeCode,
                    EmployeeName = model.EmployeeName,
                    Department = model.Department,
                    RequisitionDate = model.RequisitionDate,
                    RequisitionDeadline = model.RequisitionDeadline,
                    ItemCode = item.Code,
                    ItemName = item.Name,
                    Quantity = item.Quantity,
                    MeasurementUnit = item.MeasurementUnit,
                    Purpose = item.Purpose,
                    RequisitionCode = requisitionCode,
                    Status = userRole == "Admin" ? "Complete" : "Pending",
                    PendingBy = pendingBy, // Set based on the department's approver
                    Comments = userRole == "Admin" ? "Approved By Admin" : ""
                };

                _context.RequisitionItems.Add(requisitionItem);
            }

            // Save the changes to the database
            _context.SaveChanges();

            // Redirect to the list page
            return RedirectToAction("PendingList");
        }


        public async Task<ActionResult> Approved(string requisitionCode, string selectedItems, string comment, bool Reject)
        {
            if (string.IsNullOrEmpty(requisitionCode))
            {
                return Json(new { success = false, message = "Requisition code is required." });
            }

            // Reject requisition items
            if (Reject)
            {
                var requisitionItems = _context.RequisitionItems
                    .Where(r => r.RequisitionCode == requisitionCode)
                    .ToList();

                string empCode = HttpContext.Session.GetString("EmpCode");

                if (string.IsNullOrEmpty(empCode))
                {
                    return Json(new { success = false, message = "Employee code is missing from session." });
                }

                var userRole = _context.Employees
                    .Where(e => e.EmpCode == empCode)
                    .Select(e => e.role)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(userRole))
                {
                    return Json(new { success = false, message = "User role not found." });
                }

                if (requisitionItems.Any())
                {
                    foreach (var item in requisitionItems)
                    {
                        // Set status and comment for rejected items
                        item.Status = "Rejected";
                        item.PendingBy = $"Rejected By {userRole}";
                        item.Comments = comment;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("PendingList");
                }

                return Json(new { success = false, message = "No requisition items found for this code." });
            }
            else
            {
                if (string.IsNullOrEmpty(selectedItems))
                {
                    return Json(new { success = false, message = "No items selected for approval." });
                }

                var items = JsonConvert.DeserializeObject<List<SelectedItem>>(selectedItems);

                var requisitionItems = _context.RequisitionItems
                    .Where(r => r.RequisitionCode == requisitionCode)
                    .ToList();

                if (requisitionItems.Count == 0)
                {
                    return Json(new { success = false, message = "No items found for the given requisition code." });
                }

                var selectedItemIds = items.Select(item => item.ItemId).ToList();

                // Remove unselected items from the requisition
                var itemsToDelete = requisitionItems.Where(r => !selectedItemIds.Contains(r.Id)).ToList();
                _context.RequisitionItems.RemoveRange(itemsToDelete);

                string empCode = HttpContext.Session.GetString("EmpCode");

                if (string.IsNullOrEmpty(empCode))
                {
                    return Json(new { success = false, message = "Employee code is missing from session." });
                }

                var userRole = _context.Employees
                    .Where(e => e.EmpCode == empCode)
                    .Select(e => e.role)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(userRole))
                {
                    return Json(new { success = false, message = "User role not found." });
                }

                foreach (var selectedItem in items)
                {
                    var item = requisitionItems.FirstOrDefault(r => r.Id == selectedItem.ItemId);

                    if (item != null)
                    {
                        // Update the quantity for selected items
                        item.Quantity = selectedItem.Quantity;

                        // Check and update inventory (ensure sufficient stock)
                        var inventoryItem = _context.Inventory
                          .FirstOrDefault(i => i.ItemCode == selectedItem.ItemCode);

                        if (inventoryItem != null && inventoryItem.Quantity >= selectedItem.Quantity)
                        {
                            // Deduct from inventory
                            inventoryItem.Quantity -= selectedItem.Quantity;

                            // Update the inventory record
                            _context.Inventory.Update(inventoryItem);

                            // Record inventory history
                            var inventoryHistory = new InventoryHistories
                            {
                                ItemId = selectedItem.ItemId,
                                RequiredQuantity = -selectedItem.Quantity, // Negative because quantity is being deducted
                                ChangeDate = DateTime.Now,
                                Action = "Requisition Approved",
                                UserRole = userRole,
                                RequisitionCode = requisitionCode,
                                AvailableStock = inventoryItem.Quantity, // New available stock after deduction
                                PreviousStock = inventoryItem.Quantity + selectedItem.Quantity, // Previous stock before deduction
                                ProductCode = selectedItem.ItemCode
                            };

                            // Add to InventoryHistories table
                            _context.InventoryHistories.Add(inventoryHistory);

                            // Save changes to both Inventory and InventoryHistories
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Insufficient inventory for item: {item.ItemName}" });
                        }

                        // Set status and comments for approved items
                        if (userRole == "Admin")
                        {
                            item.Status = "Complete";
                            item.PendingBy = "Complete";
                        }
                        else
                        {
                            item.Status = "Approved";
                            item.PendingBy = "Admin";
                        }

                        item.Comments = comment;
                    }
                }

                // Save changes to the database after all updates are made
                await _context.SaveChangesAsync();
                return RedirectToAction("PendingList");
            }
        }


        //public ActionResult Approved(string requisitionCode, string selectedItems, string comment, bool Reject)
        //{
        //    if (string.IsNullOrEmpty(requisitionCode))
        //    {
        //        return Json(new { success = false, message = "Requisition code is required." });
        //    }

        //    // Reject requisition items
        //    if (Reject)
        //    {
        //        var requisitionItems = _context.RequisitionItems
        //                                        .Where(r => r.RequisitionCode == requisitionCode)
        //                                        .ToList();

        //        string empCode = HttpContext.Session.GetString("EmpCode");

        //        if (string.IsNullOrEmpty(empCode))
        //        {
        //            return Json(new { success = false, message = "Employee code is missing from session." });
        //        }

        //        var userRole = _context.Employees
        //                               .Where(e => e.EmpCode == empCode)
        //                               .Select(e => e.role)
        //                               .FirstOrDefault();

        //        if (string.IsNullOrEmpty(userRole))
        //        {
        //            return Json(new { success = false, message = "User role not found." });
        //        }

        //        if (requisitionItems.Any())
        //        {
        //            foreach (var item in requisitionItems)
        //            {
        //                // Set status and comment for rejected items
        //                item.Status = "Rejected";
        //                item.PendingBy = $"Rejected By {userRole}";
        //                item.Comments = comment;
        //            }

        //            _context.SaveChanges();
        //            return RedirectToAction("PendingList");
        //        }

        //        return Json(new { success = false, message = "No requisition items found for this code." });
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(selectedItems))
        //        {
        //            return Json(new { success = false, message = "No items selected for approval." });
        //        }

        //        var items = JsonConvert.DeserializeObject<List<SelectedItem>>(selectedItems);

        //        var requisitionItems = _context.RequisitionItems
        //                                       .Where(r => r.RequisitionCode == requisitionCode)
        //                                       .ToList();

        //        if (requisitionItems.Count == 0)
        //        {
        //            return Json(new { success = false, message = "No items found for the given requisition code." });
        //        }

        //        var selectedItemIds = items.Select(item => item.ItemId).ToList();

        //        // Remove unselected items from the requisition
        //        var itemsToDelete = requisitionItems.Where(r => !selectedItemIds.Contains(r.Id)).ToList();
        //        _context.RequisitionItems.RemoveRange(itemsToDelete);

        //        string empCode = HttpContext.Session.GetString("EmpCode");

        //        if (string.IsNullOrEmpty(empCode))
        //        {
        //            return Json(new { success = false, message = "Employee code is missing from session." });
        //        }

        //        var userRole = _context.Employees
        //                               .Where(e => e.EmpCode == empCode)
        //                               .Select(e => e.role)
        //                               .FirstOrDefault();

        //        if (string.IsNullOrEmpty(userRole))
        //        {
        //            return Json(new { success = false, message = "User role not found." });
        //        }

        //        foreach (var selectedItem in items)
        //        {
        //            var item = requisitionItems.FirstOrDefault(r => r.Id == selectedItem.ItemId);

        //            if (item != null)
        //            {
        //                // Update the quantity for selected items
        //                item.Quantity = selectedItem.Quantity;

        //                // Check and update inventory (ensure sufficient stock)
        //                   var inventoryItem = _context.Inventory
        //                   .FirstOrDefault(i => i.ItemCode == selectedItem.ItemCode);

        //                if (inventoryItem != null && inventoryItem.Quantity >= selectedItem.Quantity)
        //                {
        //                    // Update the inventory quantity
        //                    inventoryItem.Quantity -= selectedItem.Quantity;

        //                    // Record inventory history
        //                    var inventoryHistory = new InventoryHistories
        //                    {
        //                        ItemId = selectedItem.ItemId,
        //                        QuantityChanged = -selectedItem.Quantity, // Negative because quantity is being deducted
        //                        ChangeDate = DateTime.Now,
        //                        Action = "Requisition Approved", // Or "Requisition Rejected" depending on the context
        //                        UserRole = userRole,
        //                        RequisitionCode = requisitionCode
        //                    };

        //                    // Add to InventoryHistories table
        //                    _context.InventoryHistories.Add(inventoryHistory);


        //                     _context.SaveChangesAsync();
        //                }
        //                else
        //                {
        //                    return Json(new { success = false, message = $"Insufficient inventory for item: {item.ItemName}" });
        //                }

        //                // Set status and comments for approved items
        //                if (userRole == "Admin")
        //                {
        //                    item.Status = "Complete";
        //                    item.PendingBy = "Complete";
        //                }
        //                else
        //                {
        //                    item.Status = "Approved";
        //                    item.PendingBy = "Admin";
        //                }

        //                item.Comments = comment;
        //            }
        //        }

        //        _context.SaveChanges();
        //        return RedirectToAction("List");
        //    }
        //}


        [HttpPost]
        public ActionResult Approve(string requisitionCode, string selectedItems, string comment, bool Reject)
        {
            if (string.IsNullOrEmpty(requisitionCode))
            {
               
                return Json(new { success = false, message = "Requisition code is required." });
            }

            if (Reject)
            {
                
                var requisitionItems = _context.RequisitionItems
                                                .Where(r => r.RequisitionCode == requisitionCode)
                                                .ToList();

                string empCode = HttpContext.Session.GetString("EmpCode");


                if (string.IsNullOrEmpty(empCode))
                {
                    return Json(new { success = false, message = "Employee code is missing from session." });
                }

                var userRole = _context.Employees
                                       .Where(e => e.EmpCode == empCode)
                                       .Select(e => e.role)
                                       .FirstOrDefault();


                if (string.IsNullOrEmpty(userRole))
                {
                    return Json(new { success = false, message = "User role not found." });
                }

                if (requisitionItems.Any())  
                {
                    
                    foreach (var item in requisitionItems)
                    {
                        if (userRole == "Admin")
                        {
                            item.Status = "Rejected";
                            item.PendingBy = $"Rejected By {userRole}";
                        }
                        else
                        {
                            item.Status = "Rejected";
                            item.PendingBy = $"Rejected By {userRole}";
                        }

                        item.Comments = comment;
                       
                        
                    }

                    

                    _context.SaveChanges();

                   
                    return RedirectToAction("PendingList");  
                }

                return Json(new { success = false, message = "No requisition items found for this code." });
            }
            else
            {
                if (string.IsNullOrEmpty(selectedItems))
                {
                    return Json(new { success = false, message = "No items selected for approval." });
                }

                var items = JsonConvert.DeserializeObject<List<SelectedItem>>(selectedItems);

               
                var requisitionItems = _context.RequisitionItems
                                               .Where(r => r.RequisitionCode == requisitionCode)
                                               .ToList();

                
                if (requisitionItems.Count == 0)
                {
                    return Json(new { success = false, message = "No items found for the given requisition code." });
                }

                var selectedItemIds = items.Select(item => item.ItemId).ToList();

                
                var itemsToDelete = requisitionItems.Where(r => !selectedItemIds.Contains(r.Id)).ToList();
                _context.RequisitionItems.RemoveRange(itemsToDelete);

                
                string empCode = HttpContext.Session.GetString("EmpCode");

                
                if (string.IsNullOrEmpty(empCode))
                {
                    return Json(new { success = false, message = "Employee code is missing from session." });
                }

                var userRole = _context.Employees
                                       .Where(e => e.EmpCode == empCode)
                                       .Select(e => e.role)
                                       .FirstOrDefault();

                
                if (string.IsNullOrEmpty(userRole))
                {
                    return Json(new { success = false, message = "User role not found." });
                }

                
                foreach (var selectedItem in items)
                {
                    var item = requisitionItems.FirstOrDefault(r => r.Id == selectedItem.ItemId);

                    if (item != null)
                    {
                        item.Quantity = selectedItem.Quantity;

                        if (userRole == "Admin")
                        {
                            item.Status = "Complete";
                            item.PendingBy = "Complete";
                        }
                        else
                        {
                            item.Status = "Approved";
                            item.PendingBy = "Admin";
                        }

                        item.Comments = comment;
                    }
                }
               

                _context.SaveChanges();

                return RedirectToAction("PendingList"); 
            }
            
        }


        //[HttpPost]
        //public IActionResult Approve(string requisitionCode, string selectedItems, string comment)
        //{

        //        if (string.IsNullOrEmpty(selectedItems))
        //        {
        //            return Json(new { success = false, message = "No items selected for approval." });
        //        }


        //        var items = JsonConvert.DeserializeObject<List<SelectedItem>>(selectedItems);


        //        var requisitionItems = _context.RequisitionItems
        //                                       .Where(r => r.RequisitionCode == requisitionCode)
        //                                       .ToList();


        //        var selectedItemIds = items.Select(item => item.ItemId).ToList();


        //        var itemsToDelete = requisitionItems.Where(r => !selectedItemIds.Contains(r.Id)).ToList();

        //        _context.RequisitionItems.RemoveRange(itemsToDelete);

        //        string empCode = HttpContext.Session.GetString("EmpCode");


        //        var userRole = _context.Employees
        //                               .Where(e => e.EmpCode == empCode)
        //                               .Select(e => e.role)
        //                               .FirstOrDefault();

        //        foreach (var selectedItem in items)
        //        {
        //            var item = requisitionItems.FirstOrDefault(r => r.Id == selectedItem.ItemId);

        //            if (item != null)
        //            {

        //                item.Quantity = selectedItem.Quantity;


        //                if (userRole == "Admin")
        //                {
        //                    item.Status = "Complete";
        //                    item.PendingBy = "Complete";
        //                }
        //                else
        //                {

        //                    item.Status = "Approved";
        //                    item.PendingBy = "Admin";
        //                }

        //                item.Comments = comment;
        //            }
        //        }


        //        _context.SaveChanges();

        //        return RedirectToAction("List");
        //    }


     


        //public IActionResult Submit(RequisitionForm model)
        //{
        //    // Fetch the last requisition code from the RequisitionItems table
        //    var lastRequisitionCode = _context.RequisitionItems
        //                                       .OrderByDescending(r => r.RequisitionCode)
        //                                       .Select(r => r.RequisitionCode)
        //                                       .FirstOrDefault();

        //    // If no requisition code exists, start from 0001
        //    string requisitionCode;
        //    if (string.IsNullOrEmpty(lastRequisitionCode))
        //    {
        //        requisitionCode = "0001"; // Start from 0001 if no requisition code exists
        //    }
        //    else
        //    {
        //        // Extract the numeric part of the last requisition code and increment it
        //        int lastCodeNumber = int.Parse(lastRequisitionCode);
        //        requisitionCode = (lastCodeNumber + 1).ToString("D4"); // Format as 4 digits, e.g., 0002, 0003
        //    }

        //    // Create requisition items using the same requisition code
        //    foreach (var item in model.Items)
        //    {
        //        var requisitionItem = new RequisitionItem
        //        {
        //            EmployeeCode = model.EmployeeCode,
        //            EmployeeName = model.EmployeeName,
        //            Department = model.Department,
        //            RequisitionDate = model.RequisitionDate,
        //            RequisitionDeadline = model.RequisitionDeadline,
        //            ItemCode = item.Code,
        //            ItemName = item.Name,
        //            Quantity = item.Quantity,
        //            MeasurementUnit = item.MeasurementUnit,
        //            Purpose = item.Purpose,
        //            RequisitionCode = requisitionCode,
        //            Status = "Pending",         
        //            PendingBy = "LineManager"
        //        };

        //        _context.RequisitionItems.Add(requisitionItem);
        //    }

        //    _context.SaveChanges();


        //    return RedirectToAction("List");
        //}


        public IActionResult ApprovedList()
        {

            string empCode = HttpContext.Session.GetString("EmpCode");


            if (string.IsNullOrEmpty(empCode))
            {

                return RedirectToAction("Login");
            }


            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            List<RequisitionItem> requisitions;


            if (userRole != "Employee")
            {
                if (userRole == "Admin")
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Status == "Approved")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                    return View(requisitions.OrderByDescending(r => r.RequisitionDate));
                }


                var userDepartment = _context.Employees
                                              .Where(e => e.EmpCode == empCode)
                                              .Select(e => e.Department)
                                              .FirstOrDefault();

                if (!string.IsNullOrEmpty(userDepartment))
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Department == userDepartment && r.Status == "Approved")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                }
                else
                {
                    requisitions = new List<RequisitionItem>();
                }
            }
            else
            {

                requisitions = _context.RequisitionItems
                                       .Where(r => r.EmployeeCode == empCode && r.Status == "Approved")
                                       .GroupBy(r => r.RequisitionCode)
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
            }


            ViewBag.DeleteMessage = TempData["DeleteMessage"];

            return View(requisitions.OrderByDescending(r => r.RequisitionDate));
        }
        public IActionResult PendingList()
        {

            string empCode = HttpContext.Session.GetString("EmpCode");


            if (string.IsNullOrEmpty(empCode))
            {

                return RedirectToAction("Login");
            }


            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            List<RequisitionItem> requisitions;


            if (userRole != "Employee")
            {
                if (userRole == "Admin")
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Status == "Approved")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                    return View(requisitions.OrderByDescending(r => r.RequisitionDate));
                }


                var userDepartment = _context.Employees
                                              .Where(e => e.EmpCode == empCode)
                                              .Select(e => e.Department)
                                              .FirstOrDefault();

                if (!string.IsNullOrEmpty(userDepartment))
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Department == userDepartment && r.Status == "Pending")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                }
                else
                {
                    requisitions = new List<RequisitionItem>();
                }
            }
            else
            {

                requisitions = _context.RequisitionItems
                                       .Where(r => r.EmployeeCode == empCode && r.Status=="Pending")
                                       .GroupBy(r => r.RequisitionCode)
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
            }


            ViewBag.DeleteMessage = TempData["DeleteMessage"];

            return View(requisitions.OrderByDescending(r => r.RequisitionDate));
        }

        public IActionResult CompleteList()
        {

            string empCode = HttpContext.Session.GetString("EmpCode");


            if (string.IsNullOrEmpty(empCode))
            {

                return RedirectToAction("Login");
            }


            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            List<RequisitionItem> requisitions;


            if (userRole != "Employee")
            {
                if (userRole == "Admin")
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Status == "Complete")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                    return View(requisitions.OrderByDescending(r => r.RequisitionDate));
                }


                var userDepartment = _context.Employees
                                              .Where(e => e.EmpCode == empCode)
                                              .Select(e => e.Department)
                                              .FirstOrDefault();

                if (!string.IsNullOrEmpty(userDepartment))
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Department == userDepartment && r.Status=="Complete")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                }
                else
                {
                    requisitions = new List<RequisitionItem>();
                }
            }
            else
            {

                requisitions = _context.RequisitionItems
                                       .Where(r => r.EmployeeCode == empCode && r.Status == "Complete")
                                       .GroupBy(r => r.RequisitionCode)
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
            }


            ViewBag.DeleteMessage = TempData["DeleteMessage"];

            return View(requisitions.OrderByDescending(r => r.RequisitionDate));
        }

        public IActionResult List()
        {
           
            string empCode = HttpContext.Session.GetString("EmpCode");

           
            if (string.IsNullOrEmpty(empCode))
            {
                
                return RedirectToAction("Login");
            }

           
            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            List<RequisitionItem> requisitions;

           
            if (userRole != "Employee")
            {
                if (userRole == "Admin")
                {

                    requisitions = _context.RequisitionItems
                                          
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                    return View(requisitions.OrderByDescending(r => r.RequisitionDate));
                }

                
                var userDepartment = _context.Employees
                                              .Where(e => e.EmpCode == empCode)
                                              .Select(e => e.Department)
                                              .FirstOrDefault();

                if (!string.IsNullOrEmpty(userDepartment))
                {
                   
                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Department == userDepartment)  
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                }
                else
                {
                    requisitions = new List<RequisitionItem>();  
                }
            }
            else
            {
               
                requisitions = _context.RequisitionItems
                                       .Where(r => r.EmployeeCode == empCode)
                                       .GroupBy(r => r.RequisitionCode)
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
                
                
            }

          
            ViewBag.DeleteMessage = TempData["DeleteMessage"];

            return View(requisitions.OrderByDescending(r => r.RequisitionDate));
        }

        public IActionResult RejectList()
        {

            string empCode = HttpContext.Session.GetString("EmpCode");


            if (string.IsNullOrEmpty(empCode))
            {

                return RedirectToAction("Login");
            }


            var userRole = _context.Employees
                                   .Where(e => e.EmpCode == empCode)
                                   .Select(e => e.role)
                                   .FirstOrDefault();

            List<RequisitionItem> requisitions;


            if (userRole != "Employee")
            {
                if (userRole == "Admin")
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Status == "Rejected" && r.PendingBy == "Rejected By Admin")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                    return View(requisitions.OrderByDescending(r => r.RequisitionDate));
                }


                var userDepartment = _context.Employees
                                              .Where(e => e.EmpCode == empCode)
                                              .Select(e => e.Department)
                                              .FirstOrDefault();

                if (!string.IsNullOrEmpty(userDepartment))
                {

                    requisitions = _context.RequisitionItems
                                           .Where(r => r.Department == userDepartment && r.Status == "Rejected")
                                           .GroupBy(r => r.RequisitionCode)
                                           .Select(g => g.FirstOrDefault())
                                           .ToList();
                }
                else
                {
                    requisitions = new List<RequisitionItem>();
                }
            }
            else
            {

                requisitions = _context.RequisitionItems
                                       .Where(r => r.EmployeeCode == empCode && r.Status == "Rejected")
                                       .GroupBy(r => r.RequisitionCode)
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
            }


            ViewBag.DeleteMessage = TempData["DeleteMessage"];

            return View(requisitions.OrderByDescending(r => r.RequisitionDate));
        }

        public IActionResult Approval(int id)
        {
           
            var requisition = _context.RequisitionItems
                .FirstOrDefault(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }

           
            var requisitionCode = requisition.RequisitionCode;
            var requisitionsWithSameCode = _context.RequisitionItems
                .Where(r => r.RequisitionCode == requisitionCode)
                .ToList();

            
            var itemCodes = requisitionsWithSameCode.Select(r => r.ItemCode).Distinct().ToList();

            var inventoryItems = _context.Inventory
                .Where(i => itemCodes.Contains(i.ItemCode))
                .Select(i => new InventoryItem
                {
                    ItemCode = i.ItemCode,
                    Items = i.Items,
                    Quantity = i.Quantity 
                })
                .ToList();

            var viewModel = new ApprovalRequisitionViewModel
            {
                Requisition = requisition,
                RequisitionItems = requisitionsWithSameCode,
                InventoryItems = inventoryItems
            };

            
            return View(viewModel);
        }

        public IActionResult ApprovalByAdmin(int id)
        {

            var requisition = _context.RequisitionItems
                .FirstOrDefault(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }


            var requisitionCode = requisition.RequisitionCode;
            var requisitionsWithSameCode = _context.RequisitionItems
                .Where(r => r.RequisitionCode == requisitionCode)
                .ToList();


            var itemCodes = requisitionsWithSameCode.Select(r => r.ItemCode).Distinct().ToList();

            var inventoryItems = _context.Inventory
                .Where(i => itemCodes.Contains(i.ItemCode))
                .Select(i => new InventoryItem
                {
                    ItemCode = i.ItemCode,
                    Items = i.Items,
                    Quantity = i.Quantity
                })
                .ToList();

            var viewModel = new ApprovalRequisitionViewModel
            {
                Requisition = requisition,
                RequisitionItems = requisitionsWithSameCode,
                InventoryItems = inventoryItems
            };


            return View(viewModel);
        }
        public IActionResult View(int id)
        {

            var requisition = _context.RequisitionItems
                .FirstOrDefault(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }


            var requisitionCode = requisition.RequisitionCode;
            var requisitionsWithSameCode = _context.RequisitionItems
                .Where(r => r.RequisitionCode == requisitionCode)
                .ToList();


            var itemCodes = requisitionsWithSameCode.Select(r => r.ItemCode).Distinct().ToList();

            var inventoryItems = _context.Inventory
                .Where(i => itemCodes.Contains(i.ItemCode))
                .Select(i => new InventoryItem
                {
                    ItemCode = i.ItemCode,
                    Items = i.Items,
                    Quantity = i.Quantity
                })
                .ToList();

            var viewModel = new ApprovalRequisitionViewModel
            {
                Requisition = requisition,
                RequisitionItems = requisitionsWithSameCode,
                InventoryItems = inventoryItems
            };


            return View(viewModel);
        }

        //public IActionResult Approval(int id)
        //{
        //    // Fetch the requisition from the database by ID
        //    var requisition = _context.RequisitionItems.FirstOrDefault(r => r.Id == id);

        //    if (requisition == null)
        //    {
        //        return NotFound();
        //    }

        //    // Fetch all requisition items with the same requisition code
        //    var requisitionCode = requisition.RequisitionCode;
        //    var requisitionsWithSameCode = _context.RequisitionItems
        //        .Where(r => r.RequisitionCode == requisitionCode)
        //        .ToList();

        //    // Pass the list of requisition items with the same requisition code to the view
        //    return View(requisitionsWithSameCode);
        //}

        public IActionResult PreRequisitionForm()
        {
            return View();
        }

        public IActionResult PreRequisitionFormView(int id)
        {
            
            var requisition = _context.Requisitions.FirstOrDefault(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }

            
            return View(requisition);
        }
        public IActionResult PreRequisitionApprovalFormView(int id)
        {
            
            var requisition = _context.Requisitions.FirstOrDefault(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }

            
            return View(requisition);
        }


        public IActionResult PreRequisitionList()
        {
            
            var requisitions = _context.Requisitions.ToList();
            ViewBag.DeleteMessage = TempData["DeleteMessage"];
            
            return View(requisitions);
        }

        [HttpPost]       
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Requisition requisition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(requisition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PreRequisitionList)); 
            }
            return View(requisition);
        }

        public IActionResult DeleteProduct(int id)
        {
            
            var product = _context.RequisitionItems.Find(id);

            if (product == null)
            {
                return NotFound();
            }

         
            var requisitionCode = product.RequisitionCode;

            var productsToDelete = _context.RequisitionItems
                                            .Where(p => p.RequisitionCode == requisitionCode)
                                            .ToList();

            _context.RequisitionItems.RemoveRange(productsToDelete);

           
            _context.SaveChanges();

            TempData["DeleteMessage"] = "Deleted Successfully.";

            return RedirectToAction(nameof(List));
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
