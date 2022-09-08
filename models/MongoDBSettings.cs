namespace AuthApi.models
{
    public class MongoDBSettings
    {
        public string ConnectionURL { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
    }

    public class MongoDB_Book
    {
        public string ConnectionURL { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
    }
}