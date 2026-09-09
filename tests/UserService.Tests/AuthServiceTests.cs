using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using UserService.DTOs;
using UserService.Entities;
using UserService.Repositories;
using UserService.Services;
namespace UserService.Tests;
public class AuthServiceTests {
 private sealed class Users: IUserRepository { public List<User> Items=[]; public Task<User?> GetByIdAsync(int i)=>Task.FromResult(Items.FirstOrDefault(x=>x.Id==i)); public Task<User?> GetByEmailAsync(string e)=>Task.FromResult(Items.FirstOrDefault(x=>x.Email==e)); public Task<User?> GetByPhoneNumberAsync(string p)=>Task.FromResult(Items.FirstOrDefault(x=>x.PhoneNumber==p)); public Task AddAsync(User u){u.Id=Items.Count+1;Items.Add(u);return Task.CompletedTask;} public Task UpdateAsync(User u)=>Task.CompletedTask; }
 private sealed class Roles: IRoleRepository { public Role Passenger=new(){Id=1,Name="Passenger"}; public Task<Role?> GetByIdAsync(int i)=>Task.FromResult<Role?>(i==1?Passenger:null); public Task<Role?> GetByNameAsync(string n)=>Task.FromResult<Role?>(n=="Passenger"?Passenger:null); }
 private static (AuthService,Users) Make(){var u=new Users();var c=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Jwt:Key","12345678901234567890123456789012"},{"Jwt:Issuer","test"},{"Jwt:Audience","test"},{"Jwt:ExpiresInMinutes","60"}}).Build();return(new AuthService(u,new Roles(),c),u);}
 [Test] public async Task Registration_assigns_passenger_and_hashes_password(){var(s,u)=Make();var r=await s.RegisterAsync(new("A","a@b.com","1","Password1"));Assert.That(r.Role,Is.EqualTo("Passenger"));Assert.That(u.Items[0].PasswordHash,Is.Not.EqualTo("Password1"));}
 [Test] public async Task Duplicate_email_is_rejected(){var(s,_)=Make();await s.RegisterAsync(new("A","a@b.com","1","Password1"));Assert.ThrowsAsync<InvalidOperationException>(()=>s.RegisterAsync(new("B","a@b.com","2","Password1")));}
 [Test] public async Task Duplicate_phone_is_rejected(){var(s,_)=Make();await s.RegisterAsync(new("A","a@b.com","1","Password1"));Assert.ThrowsAsync<InvalidOperationException>(()=>s.RegisterAsync(new("B","b@b.com","1","Password1")));}
 [Test] public void Invalid_registration_is_rejected(){var(s,_)=Make();Assert.ThrowsAsync<ArgumentException>(()=>s.RegisterAsync(new("","","","x")));}
 [Test] public async Task Login_returns_identity_and_role_claims(){var(s,_)=Make();var r=await s.RegisterAsync(new("A","a@b.com","1","Password1"));var login=await s.LoginAsync(new("a@b.com","Password1"));var t=new JwtSecurityTokenHandler().ReadJwtToken(login.Token);Assert.That(login.UserId,Is.EqualTo(r.Id));Assert.That(t.Claims.Any(x=>x.Type==ClaimTypes.NameIdentifier&&x.Value==r.Id.ToString()),Is.True);Assert.That(t.Claims.Any(x=>x.Type==ClaimTypes.Role&&x.Value=="Passenger"),Is.True);}
 [Test] public async Task Incorrect_password_is_rejected(){var(s,_)=Make();await s.RegisterAsync(new("A","a@b.com","1","Password1"));Assert.ThrowsAsync<UnauthorizedAccessException>(()=>s.LoginAsync(new("a@b.com","wrong")));}
 [Test] public void Unknown_user_is_rejected(){var(s,_)=Make();Assert.ThrowsAsync<UnauthorizedAccessException>(()=>s.LoginAsync(new("x@y.com","Password1")));}
}
