using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- ЭТА ДИРЕКТИВА БЫЛА ПРОПУЩЕНА ИЛИ БЫЛА НЕПРАВИЛЬНО НАПИСАНА
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore; // <-- ЭТА ДИРЕКТИВА ТАКЖЕ НЕОБХОДИМА
using ServiceUsers.Models;

namespace ServiceUsers.Data{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base (options)
    {

    }
    protected override void OnModelCreating(ModelBuilder builder){
        base.OnModelCreating(builder);
    }
    }
    
}