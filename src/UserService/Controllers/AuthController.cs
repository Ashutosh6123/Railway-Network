using Microsoft.AspNetCore.Mvc; using UserService.DTOs; using UserService.Services;
namespace UserService.Controllers;
[ApiController][Route("api/auth")] public class AuthController(IAuthService auth):ControllerBase { [HttpPost("register")] public async Task<ActionResult<UserDto>> Register(RegisterRequest r)=>Created("",await auth.RegisterAsync(r)); [HttpPost("login")] public async Task<ActionResult<LoginResponse>> Login(LoginRequest r)=>Ok(await auth.LoginAsync(r)); }
