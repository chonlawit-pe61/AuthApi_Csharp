using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApi_Csharp.models
{
    public class Bookmodel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string imageName { get; set; }
    }
}