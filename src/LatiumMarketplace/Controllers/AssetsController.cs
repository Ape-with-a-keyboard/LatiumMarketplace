using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LatiumMarketplace.Data;
using LatiumMarketplace.Models.AssetViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LatiumMarketplace.Models;
using Microsoft.AspNetCore.Http;

namespace LatiumMarketplace.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssetsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        //Listing of assets/requests belonging to a specific user
        [AllowAnonymous]
        public async Task<IActionResult> MyListings(string assetLocation, string searchString, string sortby, bool recent, bool accessory)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> locationQuery = from m in _context.Asset
                                               orderby m.location
                                               select m.location;

            var assets = from m in _context.Asset
                         select m;
           

            switch (sortby)
            {

                case "request":
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    assets = assets.Where(s => s.request.Equals(true));
                    break;
                case "asset":
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    assets = assets.Where(s => s.request.Equals(false));
                    break;
                case "all":
                    assets = from m in _context.Asset
                             select m;
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    break;
            }
            
            if (recent == true)
            {
                assets = assets.OrderByDescending(s => s.addDate);
            }
            if (!String.IsNullOrEmpty(assetLocation))
            {
                assets = assets.Where(x => x.location == assetLocation);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                assets = assets.Where(x => x.name.Contains(searchString));
            }

            var assetLocatioinVM = new AssetLocation();
            assetLocatioinVM.locations = new SelectList(await locationQuery.Distinct().ToListAsync());
            assetLocatioinVM.assets = await assets.ToListAsync();
            return View(assetLocatioinVM);
        }
        // GET: Assets
        [AllowAnonymous]
        public async Task<IActionResult> Index(string assetLocation, string searchString, string sortby, bool recent, bool accessory)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> locationQuery = from m in _context.Asset
                                            orderby m.location
                                            select m.location;

            var assets = from m in _context.Asset
                         select m;

            switch (sortby)
            {
                case "request":
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    assets = assets.Where(s => s.request.Equals(true));
                    break;
                case "asset":
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    assets = assets.Where(s => s.request.Equals(false));
                    break;
                case "all":
                    assets = from m in _context.Asset
                             select m;
                    if (accessory == true)
                    {
                        assets = assets.Where(s => s.accessory != null);
                    }
                    break;
            }

            if (recent == true)
            {
                assets = assets.OrderByDescending(s => s.addDate);
            }

            if (!String.IsNullOrEmpty(assetLocation))
            {
                assets = assets.Where(x => x.location == assetLocation);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                assets = assets.Where(x => x.name.Contains(searchString));
            }

            var assetLocatioinVM = new AssetLocation();
            assetLocatioinVM.locations = new SelectList(await locationQuery.Distinct().ToListAsync());
            assetLocatioinVM.assets = await assets.ToListAsync();
            return View(assetLocatioinVM);
            //return View(await assets.ToListAsync());
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Asset.SingleOrDefaultAsync(m => m.assetID == id);
            if (asset == null)
            {
                return NotFound();
            }

            HttpContext.Response.Cookies.Append("assetId", id.ToString(),
                new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false
                }
            );
            HttpContext.Response.Cookies.Append("assetOwnerId", asset.ownerID.ToString(),
                new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false
                }
            );

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Assets/CreateReq
        // Returns view for creating request
        public IActionResult CreateReq()
        {
            return View();
        }

        // POST: Assets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("assetID,addDate,description,location,name,ownerID,price,priceDaily,priceWeekly,priceMonthly,request,accessory")] Asset asset)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = user?.Id;
                DateTime today = DateTime.Now;
                asset.addDate = today;
                asset.ownerID = userId;
                asset.request = false;
                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        // POST: Assets/CreateReq
        // Used for creating requests
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReq([Bind("assetID,addDate,description,location,name,ownerID,price,priceDaily,priceWeekly,priceMonthly,request,accessory")] Asset asset)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = user?.Id;
                DateTime today = DateTime.Now;
                asset.addDate = today;
                asset.ownerID = userId;
                asset.request = true;
                asset.price = 0;
                asset.priceDaily = 0;
                asset.priceWeekly = 0;
                asset.priceMonthly = 0;
                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Asset.SingleOrDefaultAsync(m => m.assetID == id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Assets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("assetID,addDate,description,location,name,ownerID,pricep,riceDaily,priceWeekly,priceMonthly,request,accessory")] Asset asset)
        {
            if (id != asset.assetID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.assetID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Asset.SingleOrDefaultAsync(m => m.assetID == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Asset.SingleOrDefaultAsync(m => m.assetID == id);
            _context.Asset.Remove(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool AssetExists(int id)
        {
            return _context.Asset.Any(e => e.assetID == id);
        }

    }
}
