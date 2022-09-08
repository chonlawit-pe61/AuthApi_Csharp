using AuthApi.service;
using AuthApi_Csharp.models;
using AuthApi_Csharp.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    public class BookController : Controller
    {
        private readonly MongoDBBookStore _bookStore;
        public BookController(MongoDBBookStore mongoDBBookStore)
        {
            _bookStore = mongoDBBookStore;
        }

        [HttpGet("GetListItems")]
        public Task<List<Bookmodel>> GetColorList()
        {
            return _bookStore.GetBooks();
        }

        [HttpPost("NewBook")]
        public Task<object> PostNewBook([FromForm] BookRegister reqBookRegister)
        {
            return _bookStore.PostBook(reqBookRegister);
        }
        // [HttpPut("UpdateDataBook")]
        // public Task<object> Update([FromForm] BookRegister reqBookRegister)
        // {
        //     return _bookStore.UpdateBook(reqBookRegister);
        // }
    }
}