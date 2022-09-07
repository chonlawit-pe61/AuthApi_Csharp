using AuthApi.models;
using AuthApi.service;
using AuthApi_Csharp.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly MongoDBUserService _userService;

        public UserController(MongoDBUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<List<UserModel>> Get()
        {
            return await _userService.GetItem();
        }

        [HttpPost("CreateUser")]
        public async Task<object> Register([FromForm] UserRegister userRegister)
        {
            return await _userService.RegisterUser(userRegister);
        }

        [HttpPost("Login")]
        public async Task<object> Login([FromBody] UserLogin reqUserLogin)
        {
            return await _userService.LoginUser(reqUserLogin);
        }
        [HttpGet("GetMe")]
        public Task<object> AdminsAndSellersEndpoint(string reqJwt)
        {
            var currentUser = _userService.GetCurrentUser(reqJwt);

            return currentUser;
        }

    }
}
