using AuthApi.models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AuthApi_Csharp.models;
using GlitchedPolygons.Services.JwtService;

namespace AuthApi.service
{
    public class MongoDBUserService
    {
        private IMongoCollection<UserModel> _UserCollection;
        private readonly AppSettings _applicationSettings;
        private readonly IWebHostEnvironment _environment;
        public MongoDBUserService(
            IOptions<MongoDBSettings> MongoDBSettings,
            IOptions<AppSettings> _applicationSettings,
            IWebHostEnvironment webHostEnvironment
        )
        {
            var client = new MongoClient(MongoDBSettings.Value.ConnectionURL);
            var database = client.GetDatabase(MongoDBSettings.Value.DatabaseName);
            _UserCollection = database.GetCollection<UserModel>(MongoDBSettings.Value.CollectionName);
            this._applicationSettings = _applicationSettings.Value;
            this._environment = webHostEnvironment;
        }

        public async Task<List<UserModel>> GetItem()
        {
            var Data = await _UserCollection.Find(new BsonDocument()).ToListAsync();
            return Data;
        }

        public async Task<object> RegisterUser(UserRegister userRegister)
        {
            // var results = _UserCollection.Find(emp => emp.username == userRegister.username);
            byte[] b;
            var filter = Builders<UserModel>.Filter.Eq(c => c.username, userRegister.username);
            var results = await _UserCollection.Find(filter).ToListAsync();
            if (results.Count == 0)
            {
                //img User
                // var DataimageName = SaveImg(userRegister.file);
                var NewUser = new UserModel { username = userRegister.username, name = userRegister.name, lname = userRegister.lname, Role = userRegister.Role };
                using (HMACSHA512? hmac = new HMACSHA512())
                {
                    NewUser.PasswordSalt = hmac.Key;
                    NewUser.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userRegister.password));
                }
                _UserCollection.InsertOne(NewUser);
                return NewUser;
            }
            else
            {
                return "Username Is already or Password";
            }
        }
        public async Task<object> LoginUser(UserLogin reqUserLogin)
        {
            var filter = Builders<UserModel>.Filter.Eq(c => c.username, reqUserLogin.username);
            var results = await _UserCollection.Find(filter).ToListAsync();
            if (results.Count == 0)
            {
                return "Username or Password incorrect";
            }
            var match = CheckPassword(reqUserLogin.password, results[0]);

            if (!match)
            {
                return "Username Or Password Was Invalid";
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(this._applicationSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", results[0].Id),
                    new Claim(ClaimTypes.Role, results[0].Role),
                    new Claim(ClaimTypes.Email, results[0].username),
                    new Claim("UserData", results[0].username + " " +results[0].lname),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encrypterToken = tokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevorked = false,
                UserId = results[0].Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            return new { token = encrypterToken, RefreshToken = refreshToken.Token };
        }
        private bool CheckPassword(string password, UserModel results)
        {
            bool result;

            using (HMACSHA512? hmac = new HMACSHA512(results.PasswordSalt))
            {
                var compute = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                result = compute.SequenceEqual(results.PasswordHash);
            }

            return result;
        }
        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }
        // private string SaveImg(IFormFile imageFile)
        // {
        //     string Filename = imageFile.FileName;
        //     var imagePath = GetFilePath(Filename);
        //     if (!System.IO.Directory.Exists(imagePath))
        //     {
        //         System.IO.Directory.CreateDirectory(imagePath);
        //     }

        //     string imagepath = imagePath + "\\image.jpg";

        //     if (System.IO.File.Exists(imagepath))
        //     {
        //         System.IO.File.Delete(imagepath);
        //     }
        //     using (FileStream stream = System.IO.File.Create(imagepath))
        //     {
        //         imageFile.CopyToAsync(stream);
        //     }
        //     return Filename;
        // }
        // [NonAction]
        // private string GetImagebyProduct(string productcode)
        // {
        //     string ImageUrl = string.Empty;
        //     string HostUrl = "https://localhost:7045/";
        //     string Filepath = GetFilePath(productcode);
        //     string Imagepath = Filepath + "\\image.jpg";
        //     if (!System.IO.File.Exists(Imagepath))
        //     {
        //         ImageUrl = HostUrl + "/uploads/common/noimage.jpg";
        //     }
        //     else
        //     {
        //         ImageUrl = HostUrl + "/uploads/Product/" + productcode + "/image.jpg";
        //     }
        //     return ImageUrl;

        // }

        [NonAction]
        private string GetFilePath(string ProductCode)
        {
            return this._environment.WebRootPath + "\\uploads\\Product\\" + ProductCode;
        }

        public async Task<object> GetCurrentUser(string reqJWT)
        {
            if (reqJWT == null)
            {
                return null;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(this._applicationSettings.Secret);

            try
            {

                tokenHandler.ValidateToken(reqJWT, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userData = jwtToken.Claims.First(x => x.Type == "UserData").Value;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                // return user id from JWT token if validation successful
                return new
                {
                    id = userId,
                    Data = userData
                };
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}