using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaleBoardProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Data
{
    public class SaleBoardDBContext : IdentityDbContext
    {
        public SaleBoardDBContext(DbContextOptions<SaleBoardDBContext> options) : base(options)
        {

        }

        public DbSet<User> RegisteredUsers { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<ItemSale> ItemSales { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

    }
}
