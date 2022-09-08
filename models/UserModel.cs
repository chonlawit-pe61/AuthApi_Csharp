using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace AuthApi.models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string username { get; set; } = string.Empty;
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string name { get; set; } = string.Empty;
        public string lname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }

}