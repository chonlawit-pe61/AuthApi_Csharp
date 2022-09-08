using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace AuthApi_Csharp.models
{
    public class BookRegister
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string name { get; set; } = string.Empty;
        [Required]
        public string description { get; set; } = string.Empty;
        [Required]
        public string category { get; set; } = string.Empty;
        [Required]
        public IFormFile file { get; set; }
    }
}