using Microsoft.AspNetCore.Mvc;
namespace AuthApi_Csharp.Controllers
{
    [ApiController]
    public class StoreItem : Controller
    {
        public List<string> colorList = new List<string>() { "blue", "red", "green", "yellow", "pink" };

        [HttpGet("GetColorList")]
        public List<string> GetColorList()
        {
            try
            {
                return colorList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}