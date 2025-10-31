using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using u22710362_HW3.Models;

namespace u22710362_HW3.Controllers
{
    public class HomeController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // GET: Home/Index - Merged View with filtering
        public async Task<ActionResult> Index(int? brandFilter, int? categoryFilter)
        {
            // Create ViewBag data for dropdowns
            ViewBag.Brands = await db.brands.ToListAsync();
            ViewBag.Categories = await db.categories.ToListAsync();
            ViewBag.Stores = await db.stores.ToListAsync();

            // Fetch all staff with related data
            var staffList = await db.staffs
                .Include(s => s.stores)
                .Include(s => s.staffs2)
                .ToListAsync();

            // Fetch all customers
            var customersList = await db.customers.ToListAsync();

            // Fetch products with filtering and related data
            var productsQuery = db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .AsQueryable();

            // Apply filters
            if (brandFilter.HasValue && brandFilter.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.brand_id == brandFilter.Value);
            }

            if (categoryFilter.HasValue && categoryFilter.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.category_id == categoryFilter.Value);
            }

            var productsList = await productsQuery.ToListAsync();

            // Pass data to view using ViewBag
            ViewBag.StaffList = staffList;
            ViewBag.CustomersList = customersList;
            ViewBag.ProductsList = productsList;
            ViewBag.SelectedBrand = brandFilter;
            ViewBag.SelectedCategory = categoryFilter;

            return View();
        }

        // POST: Create Staff (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff(staffs staff)
        {
            if (ModelState.IsValid)
            {
                db.staffs.Add(staff);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // POST: Create Customer (Async)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(customers customer)
        {
            if (ModelState.IsValid)
            {
                db.customers.Add(customer);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}