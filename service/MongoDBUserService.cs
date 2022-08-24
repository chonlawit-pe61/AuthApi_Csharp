using System.Xml.Linq;
using AuthApi.models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AuthApi.service
{
    public class MongoDBUserService
    {
        private IMongoCollection<UserModel> _UserCollection;
        private readonly AppSettings _applicationSettings;

        public MongoDBUserService(IOptions<MongoDBSettings> MongoDBSettings, IOptions<AppSettings> _applicationSettings)
        {
            var client = new MongoClient(MongoDBSettings.Value.ConnectionURL);
            var database = client.GetDatabase(MongoDBSettings.Value.DatabaseName);
            _UserCollection = database.GetCollection<UserModel>(MongoDBSettings.Value.CollectionName);
            this._applicationSettings = _applicationSettings.Value;
        }

        public async Task<List<UserModel>> GetItem()
        {
            return await _UserCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<object> RegisterUser(UserRegister userRegister)
        {
            // var results = _UserCollection.Find(emp => emp.username == userRegister.username);
            var filter = Builders<UserModel>.Filter.Eq(c => c.username, userRegister.username);
            var results = await _UserCollection.Find(filter).ToListAsync();
            if (results.Count == 0)
            {
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
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", results[0].username),
                    new Claim(ClaimTypes.Role, "Admin")
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encrypterToken = tokenHandler.WriteToken(token);
            return new { token = encrypterToken };
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
    }
}