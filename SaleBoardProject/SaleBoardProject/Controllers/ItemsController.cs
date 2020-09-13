using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        public async Task<IActionResult> Edit(int id, [Bind("itemID,name,description,price,Quantity")] Item item)
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
                    await _context.SaveChangesAsync();
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
                return RedirectToAction("UserHome");
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

        //Cart functions

        [Authorize]
        public IActionResult Cart()
        {
            var cartList = new List<Item>();

            if (HttpContext.Session.GetString("cartID") != null)
            {
                var cartContentID = HttpContext.Session.GetString("cartID");
                var cartListID = new List<String>(cartContentID.Split(','));
                HttpContext.Session.SetString("cartID", cartContentID);

                var cartContentAMT = HttpContext.Session.GetString("cartAMT");
                var cartListAMT = new List<String>(cartContentAMT.Split(','));
                HttpContext.Session.SetString("cartAMT", cartContentAMT);

                var i = 0;
                while (i < cartListID.Count)
                {
                    var temp = Int32.Parse(cartListID[i]);
                    cartList.Add(_context.Items.Find(temp));
                    cartList[i].Quantity = Int32.Parse(cartListAMT[i]);
                    i++;
                }
            }
            
            return View(cartList);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddToCart(int itemID, int amount)
        {
            if (HttpContext.Session.GetString("cartID") != null)
            {
                var cartContentID = HttpContext.Session.GetString("cartID");
                cartContentID = cartContentID + "," + itemID.ToString();

                HttpContext.Session.SetString("cartID", cartContentID);

                var cartContentAMT = HttpContext.Session.GetString("cartAMT");
                cartContentAMT = cartContentAMT + "," + amount.ToString();

                HttpContext.Session.SetString("cartAMT", cartContentAMT);
            }
            else
            {
                HttpContext.Session.SetString("cartID", itemID.ToString());

                HttpContext.Session.SetString("cartAMT", amount.ToString());
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public IActionResult RemoveFromCart(int itemID)
        {
            if (HttpContext.Session.GetString("cartID") != null)
            {
                var cartContentID = HttpContext.Session.GetString("cartID");
                var cartListID = new List<String>(cartContentID.Split(','));

                var temp = cartListID.IndexOf(itemID.ToString());
                if (cartListID.Remove(itemID.ToString()))
                {
                    var cartContentAMT = HttpContext.Session.GetString("cartAMT");
                    var cartListAMT = new List<String>(cartContentAMT.Split(','));

                    cartListAMT.RemoveAt(temp);

                    cartContentAMT = "";

                    foreach (var item in cartListAMT)
                    {
                        if (cartContentAMT.CompareTo("") == 0)
                        {
                            cartContentAMT = item.ToString();
                        }
                        else
                        {
                            cartContentAMT = cartContentAMT + "," + item.ToString();
                        }
                    }

                    HttpContext.Session.SetString("cartAMT", cartContentAMT);
                }

                cartContentID = "";

                foreach (var item in cartListID)
                {
                    if (cartContentID.CompareTo("") == 0)
                    {
                        cartContentID = item.ToString();
                    }
                    else
                    {
                        cartContentID = cartContentID + "," + item.ToString();
                    }
                }

                HttpContext.Session.SetString("cartID", cartContentID);
            }
            
            return RedirectToAction("Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyCart()
        {
            if (HttpContext.Session.GetString("cartID") != null)
            {
                var cartContentID = HttpContext.Session.GetString("cartID");
                var cartListID = new List<String>(cartContentID.Split(','));

                var cartContentAMT = HttpContext.Session.GetString("cartAMT");
                var cartListAMT = new List<String>(cartContentAMT.Split(','));

                var itemList = new List<Item>();

                foreach (var item in cartListID)
                {
                    var temp = Int32.Parse(item);
                    itemList.Add(_context.Items.Find(temp));
                }

                var transaction = new Transaction();
                await _context.AddAsync(transaction);
                await _context.SaveChangesAsync();

                var i = 0;
                while (i < cartListID.Count())
                {
                    var itemSale = new ItemSale
                    {
                        item = itemList[i],
                        transactionID = transaction.transactionID,
                        quantity = Int32.Parse(cartListAMT[i])
                    };

                    var item = _context.Items.Find(itemList[i].itemID);
                    item.Quantity = item.Quantity - itemSale.quantity;
                    _context.Items.Update(item);

                    await _context.AddAsync(itemSale);
                    await _context.SaveChangesAsync();
                    i++;
                }

                double costTotal = 0;

                foreach (var item in transaction.sales)
                {
                    costTotal = costTotal + (item.quantity * item.item.price);
                }

                transaction.price = costTotal;

                var user = await _userManager.GetUserAsync(User);
                transaction.buyer = user.UserName;

                _context.Update(transaction);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("cartID");
                HttpContext.Session.Remove("cartAMT");
            }

            return RedirectToAction("UserHome");
        }
    }
}
