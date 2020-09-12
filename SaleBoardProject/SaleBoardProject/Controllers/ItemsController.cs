using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaleBoardProject.Data;
using SaleBoardProject.Models;

namespace SaleBoardProject.Controllers
{
    public class ItemsController : Controller
    {
        private readonly SaleBoardDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ItemsController(SaleBoardDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Items
        [Authorize]
        //Index page is the all items catalouge
        public async Task<IActionResult> Index()
        {
            return View(await _context.Items.ToListAsync());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(String query)
        {
            if (query != null)
            {
            var catalouge = await _context.Items.ToListAsync();
            var results = new List<Item>();

            // CultureInfo paramiters [https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c]
            var culture = new CultureInfo("en-AU");

            foreach (var item in catalouge)
            {
                if (culture.CompareInfo.IndexOf(item.name, query, CompareOptions.IgnoreCase) >= 0)
                {
                    results.Add(item);
                }
            }
            
            return View(results);
            }

            return View(await _context.Items.ToListAsync());
        }

        // GET: Items/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.itemID == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart()
        {
            //Todo redirect
            return View(await _context.Items.ToListAsync());
        }

        [Authorize]
        public async Task<IActionResult> UserHome()
        {
            var user = await _userManager.GetUserAsync(User);

            var catalouge = await _context.Items.ToListAsync();
            var results = new List<Item>();

            if ( _context.RegisteredUsers.Find(user.UserName).isAdmin)
            {
                results = catalouge;
            } else
            {
                //get all items currently being sold by the user
                foreach (var item in catalouge)
                {
                    if (item.seller.CompareTo(user.UserName) == 0)
                    {
                        results.Add(item);
                    }
                }
            }
            
            //put result into viewmodel
            var model = new UserHomeViewModel();
            model.itemsForSale = results;

            var sales = await _context.ItemSales.ToListAsync();
            var relivantSales = new List<ItemSale>();

            //get all sales bought from the user
            foreach (var item in sales)
            {
                if (item.item.seller.CompareTo(user.UserName) == 0)
                {
                    relivantSales.Add(item);
                }
            }

            var transactions = await _context.Transactions.ToArrayAsync();
            var relivantTransactions = new List<Transaction>();

            //get all relivant transactions 
            foreach (var item in transactions)
            {
                foreach (var sale in relivantSales)
                {
                    if (sale.transactionID == item.transactionID)
                    {
                        relivantTransactions.Add(item);
                    }
                }
            }

            var buyers = new List<String>();

            //get a list of everyone who has bought from the user
            foreach (var item in relivantTransactions)
            {
                if (!buyers.Contains(item.buyer))
                {
                    buyers.Add(item.buyer);
                }
            }

            var spending = new List<double>();

            //count the amount spent buy each user
            foreach (var item in buyers)
            {
                double total = 0;
                foreach (var sale in relivantTransactions)
                {
                    if (sale.buyer.CompareTo(item) == 0)
                    {
                        total = total + sale.price;
                    }
                }
                spending.Add(total);
            }

            var userSpending = new List<UserSpending>();

            //collect relevent data
            var i = 0;
            while (i < buyers.Count)
            {
                var temp = new UserSpending
                {
                    buyer = buyers[i],
                    Spending = spending[i]
                };
                userSpending.Add(temp);
                i++;
            }

            model.transactionHistory = userSpending;

            return View(model);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------

        // GET: Items/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,name,description,price,Quantity")] Item item)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                item.seller = user.UserName;
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: Items/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,name,description,price,Quantity")] Item item)
        {
            var user = await _userManager.GetUserAsync(User);
            item.seller = user.UserName;
            if (id != item.itemID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.itemID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: Items/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.itemID == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.itemID == id);
        }
    }
}
