using Floresta.Interfaces;
using Floresta.Models;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    public class MapController : Controller
    {
        private IRepository<Marker> _repo;
        private FlorestaDbContext _context;
        private SignInManager<User> _signInManager;

        public MapController(IRepository<Marker> repo,
            FlorestaDbContext context,
            SignInManager<User> signInManager)
        {
            _repo = repo;
            _context = context;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {   //якщо користувач автентифікований
            if (_signInManager.IsSignedIn(User))
            {
                //отримуємо колекцію саджанців, кількість яких більша 0
                var seedlings = _context.Seedlings.Where(s => s.Amount > 0).ToList();
                var model = new PaymentViewModel();
                //додаємо в модель колекцію саджанців
                model.Seedlings = seedlings;
                //повертаємо представлення
                return View(model);
            }
            else
            {   //якщо користувач не автентифікований - переадресовуємо його на сторінку логінування
                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize(Roles = "admin")]
        public IActionResult Markers()
        {
            var markers = _repo.GetAll();
            return View(markers);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Index(Marker marker)
        {
            await _repo.AddAsync(marker);
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int? id)
        {
            if (id != null)
            {
                var marker = _repo.GetById(id);
                return View(marker);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Marker marker)
        {
            await _repo.UpdateAsync(marker);
            return RedirectToAction("Markers");
        }

        

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        { 
            if (await _repo.DeleteAsync(id))
            {
                return RedirectToAction("Markers");
            }
            else
                return NotFound();
        }

        public JsonResult GetMarkers()
        {
            var markers = _context.Markers.ToList();
            return new JsonResult(markers);
        }

        public JsonResult GetRequiredData()
        {
            var markers = _context.Markers.ToList();
            var seedlings = _context.Seedlings.Select(s => new { s.Id, s.Amount});
            bool IsAdmin = _signInManager.IsSignedIn(User) && User.IsInRole("admin");

            return new JsonResult(new
            {
                markers,
                seedlings,
                IsAdmin
            });
        }
    }
}
