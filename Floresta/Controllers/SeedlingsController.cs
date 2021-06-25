using Floresta.Interfaces;
using Floresta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    [Authorize(Roles = "admin")] //доступ має тільки адмін
    public class SeedlingsController : Controller
    {
        private IRepository<Seedling> _repo;

        public SeedlingsController(IRepository<Seedling> repo)
        {
            //отримуємо реалізацію функціональності CRUD для саджанців
            _repo = repo;
        }
        public IActionResult Index()
        {   //повертаємо представлення з колекцією саджанців         
            return View(_repo.GetAll());
        }

        public IActionResult Create()
        {    //повертаємо представлення сторінки створення
            return View();
        }

        [HttpPost]
        //після введення даних, даний метод реагує на підтвердження
        public async Task<IActionResult> Create(Seedling seedling)
        {   //якщо всі дані були введені вірно
            if (ModelState.IsValid)
            {   //додаємо саджанець
                await _repo.AddAsync(seedling);
                //повертаємо на сторінку з колекцією саджанців
                return RedirectToAction("Index");
            }
            //в іншому випадку повертаємо представлення
            //з провальною валідацією
            return View(seedling);
        }

        //предаставлення для редагування даних
        public IActionResult Edit(int? id)
        {       //якщо айді не передалося
            if (!id.HasValue)
                //повертаємо помилку протоколу 400
                return BadRequest();
            //якщо все добре, то отримуємо саджанець за адйі
            var seedling = _repo.GetById(id);
            if (seedling == null) //якщо саджанець не вдалося отримати
                return NotFound(); //повертаємо 404
            return View(seedling); //якщо все добре - повертаємо представлення
        }

        [HttpPost]
        //метод, який буде реагувати на підтвердження введених даних редагування
        public async Task<IActionResult> Edit(Seedling seedling)
        {
            //оновлюємо дані саджанця
            await _repo.UpdateAsync(seedling);
            //повертаємо на сторінку з колекцією саджанців
            return RedirectToAction("Index");
        }

        [HttpPost]
        //метод, який реагуватиме на кнопку "Видалити"
        public async Task<IActionResult> Delete(int id)
        {
            //якщо видалення вдастся і повернеться істинність
            if(await _repo.DeleteAsync(id))
                //повертаємо на сторінку з колекцією саджанців
                return RedirectToAction("Index");
            else
                //в іншому випадку повертаємо 404
            return NotFound();
        }
    }
}
