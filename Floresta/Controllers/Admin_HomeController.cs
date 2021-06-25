using Floresta.Models;
using Floresta.Services;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace Floresta.Controllers
{
    [Authorize(Roles = "admin, moderator")]
    public class Admin_HomeController : Controller
    {
        private SignInManager<User> _signInManager;
        private UserManager<User> _userManager;
        private FlorestaDbContext _context;

        public Admin_HomeController(SignInManager<User> signInManager,
            UserManager<User> userManager,
            FlorestaDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            if (_signInManager.IsSignedIn(User)) //якщо користувач автентифікований
            {
                var user = await _userManager.GetUserAsync(User); //шукаємо його
                if (user != null) //якщо користувач знайдений
                {
                    var model = new ShowUserViewModel
                    {
                        Name = user.Name,
                        Surname = user.UserSurname, //передаємо його дані в модель
                        Email = user.Email
                    };

                    return View(model); //повертаємо представлення
                }
            }
            return View(); //в іншому випадку повертаємо представлення без даних
        }

        public IActionResult GetQuestions()
        {
            //повертає представлення з колекцією питань
            var questions = _context.Questions
                .Include(c => c.User)
                .Include(t => t.QuestionTopic);
            return View(questions);
        }

        public IActionResult GetQuestionTopics()
        {
            //повертає представлення для отримання колекції тем питання
            return View();
        }

        public IActionResult AnswerQuestion()
        { 
            //повертає представлення для відправки відповіді на питання
            return View();
        }
  
        public IActionResult PurchasesDiagram()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnswerQuestion(int id, SendEmailViewModel model)
        {
            //отримуємо питання за айді
            Question question = _context.Questions.FirstOrDefault(x => id == x.Id);
            //отримуємо користувача за айді
            var user = _context.Users.FirstOrDefault(x => question.UserId == x.Id);
            EmailService emailService = new EmailService();
            //відправляємо відповідь електронною поштою
            await emailService.SendEmailAsync(user.Email, "Відповідь на питання",
                $"Ваше питання було: \"{question.QuestionText}\"\n\nОфіційна відповідь на ваше питання:\n\n{model.Message}");
            //ставимо мітку, що на дане питання є відповідь
            question.IsAnswered = true;
            //оновлюємо дані питання
            _context.Questions.Update(question);
            //зберігаємо зміни
            await _context.SaveChangesAsync();
            //повертаємо на представлення з колекцією питань
            return RedirectToAction("GetQuestions");
        }


        public IActionResult Purchases()
        {   //отримання колекції з потрібними даними
            var purchases = _context
                .Payments
                .Include(u => u.User)
                .Include(m => m.Marker)
                .Include(s => s.Seedling);
            //повернення представлення з колекцією оплат
            return View(purchases);
        }

        
        [HttpPost]
        //підтвердження покупки
        public async Task<IActionResult> Purchases(int? id)
        {
            if (id != null) //якщо айді передалось
            {   //отримуємо дані покупки
                var purchase = _context.Payments.FirstOrDefault(x => x.Id == id);
                //отримуємо дані користувача
                var user = _context.Users.FirstOrDefault(x => x.Id == purchase.UserId);
                EmailService emailService = new EmailService();
                //надсилаємо повідомлення електронною поштою
                await emailService.SendEmailAsync(user.Email, "Вітання!!!",
                    $"Дорога(-ий) {user.Name} {user.UserSurname}, ваша оплата була успішно підтверджена!" +
                    $"\nВаше бажання врятувати світ є більшим, ніж наша вдячність вам!" +
                    $"\nСлідкуйте за нашими оновленнями, щоб бути в курсі всього!");
                //ставимо мітку, що оплата вдалася
                purchase.IsPaymentSucceded = true;
                //оновлюємо дані оплати
                _context.Update(purchase);
                //зберігаємо зміни
                await _context.SaveChangesAsync();
                return RedirectToAction("Purchases");
            }
            else
            {   //якщо айді не передалося, то повертаємо 404
                return NotFound();
            }
        }

        [HttpPost]
        //скасувати оплату
        public async Task<IActionResult> DeclinePurchase(int? id)
        {
            if (id != null)
            {
                var purchase = _context.Payments.FirstOrDefault(x => x.Id == id);
                var user = _context.Users.FirstOrDefault(x => x.Id == purchase.UserId);
                var seedling = _context.Seedlings.FirstOrDefault(x => x.Id == purchase.SeedlingId);
                var marker = _context.Markers.FirstOrDefault(x => x.Id == purchase.MarkerId);

                EmailService emailService = new EmailService();
                await emailService.SendEmailAsync(user.Email, "Статус Оплати",
                    $"{user.Name} {user.UserSurname}, на жаль, ваша оплата не була успішною." +
                    $" Зв'яжіться з підтримкою Floresta для отримання більш детальної інформації.");
                //додаємо назад кількість саджанців
                seedling.Amount += purchase.PurchasedAmount;
                //оновлюємо дані саджанця
                _context.Update(seedling);

                //додаємо назад придбані місця
                marker.PlantCount += purchase.PurchasedAmount;
                //вказуємо, що мітка доступна для висадки саджанців
                marker.isPlantingFinished = false;
                //оновлюємо дані мітки
                _context.Update(marker);
                //вказуємо, що оплата провалилася
                purchase.IsPaymentFailed = true;
                //оновлюємо дані оплати
                _context.Update(purchase);
                //зберігаємо всі зміни
                await _context.SaveChangesAsync();
                //повертаємо на сторінку з колекцією оплат
                return RedirectToAction("Purchases");
            }
            else
            { //якщо айді немає - повертаємо 404
                return NotFound();
            }
        }

        public async Task<IActionResult> GetTeamParticipants()
        {
            var participants = await _context
                .Users
                .Where(u => u.IsClaimingForTeamParticipating)
                .ToListAsync();
            return View(participants);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmParticipating(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "moderator");
                user.IsTeamParticipant = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                EmailService emailService = new EmailService();

                await emailService.SendEmailAsync(user.Email, "Статус участі в команді",
                    $"{user.Name} {user.UserSurname}, ви тепер офіційно учасник команди Floresta Team!<br /> " +
                    $"Давайте зробимо світ чистішим киснем наших дерев!");
                return RedirectToAction("GetTeamParticipants");
            }
            else
                return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeclineParticipating(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                user.IsClaimingForTeamParticipating = false;
                user.IsTeamParticipant = false;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                EmailService emailService = new EmailService();

                await emailService.SendEmailAsync(user.Email, "Статус участі в команді",
                    $"{user.Name} {user.UserSurname}, на жаль, ви не стали учасником Floresta Team, " +
                    $"проте ви однаково зможете зробити світ кращим, посадивши дерево!");

                return RedirectToAction("GetTeamParticipants");
            }
            else
                return NotFound();
        }

        public JsonResult GetSeedlingsRates()
        {
            var statistics = _context.Payments.Include(s => s.Seedling)
                .GroupBy(p => p.Seedling.Name)
                .Select(p => new { seedling = p.Key, sum = p.Sum(p => p.PurchasedAmount) })
                .AsEnumerable()
                .ToDictionary(d => d.seedling, d => d.sum);
         
            return new JsonResult(statistics);
        }
    }
}
