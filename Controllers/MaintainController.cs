using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using u22710362_HW3.Models;

namespace u22710362_HW3.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();
        // GET: Home/Maintain - Display all staff, customers, and products
        public async Task<ActionResult> Maintain()
        {
            // Fetch all staff with related data
            var staffList = await db.staffs
                .Include(s => s.stores)
                .Include(s => s.staffs2)
                .ToListAsync();

            // Fetch all customers
            var customersList = await db.customers.ToListAsync();

            // Fetch all products with related data
            var productsList = await db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .ToListAsync();

            // Pass data to view using ViewBag
            ViewBag.StaffList = staffList;
            ViewBag.CustomersList = customersList;
            ViewBag.ProductsList = productsList;
            ViewBag.Stores = await db.stores.ToListAsync();
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();

            return View();
        }

        // STAFF CRUD OPERATIONS

        // GET: Get Staff Details for Edit Modal
        public async Task<JsonResult> GetStaffDetails(int id)
        {
            var staff = await db.staffs.FindAsync(id);
            if (staff == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                staff_id = staff.staff_id,
                first_name = staff.first_name,
                last_name = staff.last_name,
                email = staff.email,
                phone = staff.phone,
                active = staff.active,
                store_id = staff.store_id,
                manager_id = staff.manager_id
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Update Staff (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStaff(staffs staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }

            return RedirectToAction("Maintain");
        }

        // POST: Delete Staff (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteStaff(int staff_id)
        {
            var staff = await db.staffs.FindAsync(staff_id);
            if (staff != null)
            {
                db.staffs.Remove(staff);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Maintain");
        }

        // CUSTOMER CRUD OPERATIONS

        // GET: Get Customer Details for Edit Modal
        public async Task<JsonResult> GetCustomerDetails(int id)
        {
            var customer = await db.customers.FindAsync(id);
            if (customer == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                customer_id = customer.customer_id,
                first_name = customer.first_name,
                last_name = customer.last_name,
                email = customer.email,
                phone = customer.phone,
                street = customer.street,
                city = customer.city,
                state = customer.state,
                zip_code = customer.zip_code
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Update Customer (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateCustomer(customers customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }

            return RedirectToAction("Maintain");
        }

        // POST: Delete Customer (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCustomer(int customer_id)
        {
            var customer = await db.customers.FindAsync(customer_id);
            if (customer != null)
            {
                db.customers.Remove(customer);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Maintain");
        }

        // PRODUCT CRUD OPERATIONS

        // GET: Get Product Details for Edit Modal
        public async Task<JsonResult> GetProductDetails(int id)
        {
            var product = await db.products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                product_id = product.product_id,
                product_name = product.product_name,
                brand_id = product.brand_id,
                category_id = product.category_id,
                model_year = product.model_year,
                list_price = product.list_price
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Update Product (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProduct(products product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Maintain");
            }

            return RedirectToAction("Maintain");
        }

        // POST: Delete Product (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProduct(int product_id)
        {
            var product = await db.products.FindAsync(product_id);
            if (product != null)
            {
                db.products.Remove(product);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Maintain");
        }
    }
}