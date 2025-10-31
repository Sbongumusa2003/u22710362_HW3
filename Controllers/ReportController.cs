using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using u22710362_HW3.Models;

namespace u22710362_HW3.Controllers
{
    public class ReportController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();
        // GET: Home/Report
        public async Task<ActionResult> Report()
        {
            try
            {
                // Get sales performance data - orders with customer and staff info
                var salesData = await db.orders
                    .Include(o => o.customers)
                    .Include(o => o.staffs)
                    .Include(o => o.stores)
                    .Include(o => o.order_items.Select(oi => oi.products))
                    .Where(o => o.order_date >= DbFunctions.AddMonths(DateTime.Now, -6))
                    .ToListAsync();

                // Calculate sales by brand
                var brandSales = await db.order_items
                    .Include(oi => oi.products.brands)
                    .GroupBy(oi => oi.products.brands.brand_name)
                    .Select(g => new
                    {
                        BrandName = g.Key,
                        TotalSales = g.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount)),
                        TotalQuantity = g.Sum(oi => oi.quantity)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .ToListAsync();

                // Calculate sales by category
                var categorySales = await db.order_items
                    .Include(oi => oi.products.categories)
                    .GroupBy(oi => oi.products.categories.category_name)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalSales = g.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount)),
                        TotalQuantity = g.Sum(oi => oi.quantity)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .ToListAsync();

                // Top performing staff
                var staffPerformance = await db.orders
                    .Include(o => o.staffs)
                    .Include(o => o.order_items)
                    .GroupBy(o => new { o.staffs.staff_id, o.staffs.first_name, o.staffs.last_name })
                    .Select(g => new
                    {
                        StaffName = g.Key.first_name + " " + g.Key.last_name,
                        TotalOrders = g.Count(),
                        TotalSales = g.Sum(o => o.order_items.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount)))
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(10)
                    .ToListAsync();

                // Monthly sales trend (last 6 months)
                var monthlySales = await db.orders
                    .Where(o => o.order_date >= DbFunctions.AddMonths(DateTime.Now, -6))
                    .GroupBy(o => new { Year = o.order_date.Year, Month = o.order_date.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalOrders = g.Count(),
                        TotalSales = g.Sum(o => o.order_items.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount)))
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                // Popular products
                var popularProducts = await db.order_items
                    .Include(oi => oi.products)
                    .GroupBy(oi => new { oi.products.product_id, oi.products.product_name })
                    .Select(g => new
                    {
                        ProductName = g.Key.product_name,
                        TotalQuantitySold = g.Sum(oi => oi.quantity),
                        TotalRevenue = g.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount))
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .Take(10)
                    .ToListAsync();

                // Store performance
                var storePerformance = await db.orders
                    .Include(o => o.stores)
                    .Include(o => o.order_items)
                    .GroupBy(o => new { o.stores.store_id, o.stores.store_name })
                    .Select(g => new
                    {
                        StoreName = g.Key.store_name,
                        TotalOrders = g.Count(),
                        TotalSales = g.Sum(o => o.order_items.Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount)))
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .ToListAsync();

                // Pass data to view
                ViewBag.BrandSales = brandSales;
                ViewBag.CategorySales = categorySales;
                ViewBag.StaffPerformance = staffPerformance;
                ViewBag.MonthlySales = monthlySales;
                ViewBag.PopularProducts = popularProducts;
                ViewBag.StorePerformance = storePerformance;

                // Get saved reports from document archive
                var archivePath = Server.MapPath("~/App_Data/Reports");
                if (!System.IO.Directory.Exists(archivePath))
                {
                    System.IO.Directory.CreateDirectory(archivePath);
                }

                var savedReports = System.IO.Directory.GetFiles(archivePath)
                    .Select(f => new
                    {
                        FileName = System.IO.Path.GetFileName(f),
                        FilePath = f,
                        CreatedDate = System.IO.File.GetCreationTime(f),
                        FileSize = new System.IO.FileInfo(f).Length
                    })
                    .OrderByDescending(f => f.CreatedDate)
                    .ToList();

                ViewBag.SavedReports = savedReports;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading report data: " + ex.Message;
                return View();
            }
        }

        // POST: Download Report
        [HttpPost]
        public ActionResult DownloadReport(string fileName)
        {
            try
            {
                var filePath = Server.MapPath("~/App_Data/Reports/" + fileName);

                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/pdf", fileName);
                }

                return HttpNotFound();
            }
            catch (Exception ex)
            {
                return Content("Error downloading file: " + ex.Message);
            }
        }

        // POST: Delete Report
        [HttpPost]
        public async Task<ActionResult> DeleteReport(string fileName)
        {
            try
            {
                var filePath = Server.MapPath("~/App_Data/Reports/" + fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true, message = "Report deleted successfully" });
                }

                return Json(new { success = false, message = "File not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting file: " + ex.Message });
            }
        }
    }
}