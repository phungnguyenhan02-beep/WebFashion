using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class ApplicationDbContext : DbContext
    {
       // public ApplicationDbContext() : base("name=WebThoiTrangEntities") { }
        public DbSet<AddressUser> AddressUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ImageProduct> ImageProducts { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
    }
}