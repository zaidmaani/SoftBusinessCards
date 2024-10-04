using Microsoft.EntityFrameworkCore;
using SoftBusinessCards.Models;

namespace SoftBusinessCards.Data
{
    public class BusinessCardContext : DbContext
    {
        public BusinessCardContext(DbContextOptions<BusinessCardContext> options) : base(options) { }

        public DbSet<BusinessCard> BusinessCards { get; set; }
    }
}
