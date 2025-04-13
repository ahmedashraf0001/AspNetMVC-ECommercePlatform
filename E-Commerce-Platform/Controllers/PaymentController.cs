using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}