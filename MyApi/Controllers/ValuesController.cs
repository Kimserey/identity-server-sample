using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers
{
    [Authorize]
    public class ValuesController : Controller
    {
        public IActionResult Get()
        {
            return Json(new[] { "Data1", "Data2" });
        }
    }
}
