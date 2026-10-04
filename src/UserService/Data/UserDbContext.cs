using Microsoft.EntityFrameworkCore;
using UserService.Entities;
namespace UserService.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options) { 
    public DbSet<User> Users => Set<User>(); // Represents the Users table in the database, allowing CRUD operations on User entities.
    public DbSet<Role> Roles => Set<Role>(); // Represents the Roles table in the database, allowing CRUD operations on Role entities.

    protected override void OnModelCreating(ModelBuilder b) { 
        b.Entity<Role>(
            e=>{e.HasIndex(x=>x.Name).IsUnique(); // Ensure that the Name property (Role Name) of the Role entity is unique across all records in the Roles table.
                e.HasData(
                    new Role{Id=1,Name="Passenger"},
                    new Role{Id=2,Name="Administrator"}
                );
            }
        ); 
        
        b.Entity<User>(
            e => {
                e.HasIndex(x=>x.Email).IsUnique(); // Ensure that the Email property of the User entity is unique across all records in the Users table.
                e.HasIndex(x=>x.PhoneNumber).IsUnique(); // Ensure that the PhoneNumber property of the User entity is unique across all records in the Users table.

                e.HasOne(x=>x.Role)  // Every single User profile must point to exactly one Role.
                    .WithMany(x=>x.Users) // Every single Role can be assigned to multiple User profiles.
                    .HasForeignKey(x=>x.RoleId); // The RoleId property in the User entity is the foreign key that establishes the relationship with the Role entity.
            }
        ); 
    } 
}
