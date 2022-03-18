#nullable disable
using estore.MicroServices.Orders.Models;
using Microsoft.EntityFrameworkCore;

namespace estore.MicroServices.Orders.DataContext
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext()
        {
        }

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
              : base(options)
        {
        }

        public virtual DbSet<Order> Orders { get; set; }
    }
}
