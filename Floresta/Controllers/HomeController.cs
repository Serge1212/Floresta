using Floresta.Models;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<User> _userManager;
        private FlorestaDbContext _context;

        public HomeController(UserManager<User> userManager,
            FlorestaDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel();
            //передаємо теми питань в випадаючий список
            model.Topics = _context.QuestionTopics
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Topic
                });
            //повертаємо представлення
            return View(model);
               
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult FAQ()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AskQuestion(HomeViewModel model)
        {
            //отримуємо юзера, який задав питання
            var user = await _userManager.GetUserAsync(User);
            if (user != null) //якщо такий юзер є
            {
                //створюємо екземпляр питання
                Question question = new Question
                {
                    QuestionText = model.Question,
                    QuestionTopicId = model.TopicId
                };
                //додаємо питання
                _context.Questions.Add(question);
                //додаємо задане питання в дані юзера
                user.Questions.Add(question);
                //зберігаємо зміни
                await _context.SaveChangesAsync();
                //повертаємо юзера на головну сторінку
                return RedirectToAction("Index", "Home");
            }
            else
                //якщо юзера не знайдено, то повертаємо на сторінку логування
                return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> JoinTeam(HomeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                if (model.PhoneNumber != null)
                {
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsClaimingForTeamParticipating = true;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public JsonResult GetDataForChart()
        {
            var users = _context.Payments.Where(p => p.IsPaymentSucceded).Select(p => p.UserId).Distinct().Count();
            var trees = _context.Payments.Where(p => p.IsPaymentSucceded).Sum(p => p.PurchasedAmount);
            var remainingTrees = _context.Seedlings.Sum(s => s.Amount);
            return new JsonResult(new {users, trees, remainingTrees });
        }
    }
}
