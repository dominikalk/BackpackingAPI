using Backpacking.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.DbContexts
{
    public class BackpackingContext : IdentityDbContext<User>
    {
        public BackpackingContext(DbContextOptions<BackpackingContext> options) : base(options) { }
    }
}
