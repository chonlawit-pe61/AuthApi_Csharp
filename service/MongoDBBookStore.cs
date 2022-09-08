using AuthApi.models;
using AuthApi_Csharp.models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthApi_Csharp.service
{
    public class MongoDBBookStore
    {
        private IMongoCollection<Bookmodel> _BookCollection;
        private readonly IWebHostEnvironment _environment;


        public MongoDBBookStore(
            IOptions<MongoDB_Book> MongoDB_Book,
            IWebHostEnvironment webHostEnvironment
        )
        {
            var client = new MongoClient(MongoDB_Book.Value.ConnectionURL);
            var database = client.GetDatabase(MongoDB_Book.Value.DatabaseName);
            _BookCollection = database.GetCollection<Bookmodel>(MongoDB_Book.Value.CollectionName);
            this._environment = webHostEnvironment;
        }

        public async Task<List<Bookmodel>> GetBooks()
        {
            var Data = await _BookCollection.Find(new BsonDocument()).ToListAsync();
            if (Data != null && Data.Count > 0)
            {
                Data.ForEach(item =>
            {
                item.imageName = GetImagebyProduct(item.imageName);
            });
            }
            return Data;
        }
        private string GetImagebyProduct(string productcode)
        {
            string ImageUrl = string.Empty;
            string HostUrl = "https://localhost:7045/";
            string Filepath = GetFilePath(productcode);
            string Imagepath = Filepath + "\\image.jpg";
            if (!System.IO.File.Exists(Imagepath))
            {
                ImageUrl = HostUrl + "/uploads/common/noimage.jpg";
            }
            else
            {
                ImageUrl = HostUrl + "/uploads/Product/" + "/image.jpg";
            }
            return ImageUrl;

        }

        public async Task<object> PostBook(BookRegister reqBookRegister)
        {
            var filter = Builders<Bookmodel>.Filter.Eq(c => c.name, reqBookRegister.name);
            var results = await _BookCollection.Find(filter).ToListAsync();
            if (results.Count == 0)
            {
                var DataimageName = SaveImg(reqBookRegister.file);
                var NewBook = new Bookmodel
                {
                    name = reqBookRegister.name,
                    description = reqBookRegister.description,
                    category = reqBookRegister.category,
                    imageName = DataimageName
                };

                _BookCollection.InsertOne(NewBook);
                return NewBook;
            }
            else
            {
                return "Username Is already or Password";
            }
        }
        private string SaveImg(IFormFile imageFile)
        {
            string Filename = imageFile.FileName;
            var imagePath = GetFilePath(Filename);
            if (!System.IO.Directory.Exists(imagePath))
            {
                System.IO.Directory.CreateDirectory(imagePath);
            }

            string imagepath = imagePath + "\\image.jpg";

            if (System.IO.File.Exists(imagepath))
            {
                System.IO.File.Delete(imagepath);
            }
            using (FileStream stream = System.IO.File.Create(imagepath))
            {
                imageFile.CopyToAsync(stream);
            }
            return Filename;
        }
        private string GetFilePath(string ProductCode)
        {
            return this._environment.WebRootPath + "\\uploads\\Product\\";
        }


        //public async Task<object> UpdateBook(BookRegister reqBookRegister)
        //{
        //    var UpdataData = await _BookCollection.ReplaceOne(x => x.Id == reqBookRegister.Id);

        //    return reqBookRegister;
        //}

    }
}