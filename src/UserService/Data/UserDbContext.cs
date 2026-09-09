using Microsoft.EntityFrameworkCore;
using UserService.Entities;
namespace UserService.Data;
public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options) { public DbSet<User> Users => Set<User>(); public DbSet<Role> Roles => Set<Role>(); protected override void OnModelCreating(ModelBuilder b) { b.Entity<Role>(e=>{e.HasIndex(x=>x.Name).IsUnique();e.HasData(new Role{Id=1,Name="Passenger"},new Role{Id=2,Name="Administrator"});}); b.Entity<User>(e=>{e.HasIndex(x=>x.Email).IsUnique();e.HasIndex(x=>x.PhoneNumber).IsUnique();e.HasOne(x=>x.Role).WithMany(x=>x.Users).HasForeignKey(x=>x.RoleId);}); } }
