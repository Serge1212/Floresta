using Floresta.Models;
using Floresta.Services;
using Floresta.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Floresta.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View(); // повертає сторінку реєстрації

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)//дані, які передадуться після натискання "зареєструватися"
        {
            if (ModelState.IsValid) //якщо всі дані вірні
            {
               //створюємо змінну користувача і передаємо дані з в'ю моделі
                User user = new User { Email = model.Email, UserName = model.Email, Name = model.Name, UserSurname = model.Surname };
               //створюємо користувача
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) //якщо створення було успішним
                {
                    // генеруємо код підтвердження для користувача
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action( //============
                        "ConfirmEmail",           
                        "Account",                                 //Генеруємо посилання, за яким користувач перейде на електронній пошті
                        new { userId = user.Id, code = code },
                        protocol: HttpContext.Request.Scheme);//==========

                    //екземпляр класу, який використовується для надсилання Email
                    EmailService emailService = new EmailService();

                    await emailService.SendEmailAsync(model.Email, "Підтвердіть свій акаунт",
                        $"Підтвердіть свій акаунт за наступним посиланням: <a href='{callbackUrl}'>Підтвердити акаунт</a>");

                    return Content("Для завершення реєстрації, перейдіть за посиланням, яке було надіслане вам на пошту.");
                }
                else //якщо дані невірні
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description); //відображаємо помилки
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        //в параметри передаються айді користувача та код, який був згенерований в POST-версії методу реєстрації
        public async Task<IActionResult> ConfirmEmail(string userId, string code) 
        {
            if (userId == null || code == null) //якщо хоча б одне значення порожнє
            {
                return View("Error"); //повертаємо помилку
            }
            //якщо ні
            var user = await _userManager.FindByIdAsync(userId); //шукаємо користувача за айді
            if (user == null) //якщо користувача немає
            {
                return View("Error"); //повертаємо помилку
            }
            //якщо ні
            var result = await _userManager.ConfirmEmailAsync(user, code);//підтверджуємо пошту
            if (result.Succeeded) //якщо підтвердження пошти пройшло успішно
                return RedirectToAction("Index", "Home"); //повертаємо користувача на головну сторінку
            else //якщо ні
                return View("Error"); //повертаємо помилку
        }

        [HttpGet]
        //даний метод приймає рядок посилання, на яке буде здійснено перехід при автентифікації
        public async Task<IActionResult> Login(string returnUrl = null) =>
            View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                //отримуємо всі провайдери зовнішнього логування (наразі тільки Google)
                ExternalLogin = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            });

        [AllowAnonymous]
        [HttpPost]
        //метод приймає два параметри: провайдер(Google, Facebook...), посилання для повернення
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl }); //генеруємо посилання перенаправлення
            //генеруємо властивості для автентифікації
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);//відображаємо автентифікацію провайдера
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/"); //визначаємо посилання перенаправлення

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogin = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null) //якщо є помилки провайдера
            {
                //відображаємо помилки та здійснюємо перенаправлення на сторінку логування
                ModelState.AddModelError(string.Empty, $"Помилка зовнішнього провайдера {remoteError}");
                return View("Login", loginViewModel);
            }
            //отримуємо інформацію автентифікації
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null) //якшо інформацію не вдалося завантажити
            {
                //відображаємо помилку і перенаправляємо користувача на сторінку логування
                ModelState.AddModelError(string.Empty, "Помилка завантаження інформації зовнішнього провайдера");

                return View("Login", loginViewModel);
            }
            //здійснюємо зовнішнє логування
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            //якщо все вдалося
            if (signInResult.Succeeded)
            {
                //перенаправляємо користувача за посиланням перенаправлення
                return LocalRedirect(returnUrl);
            }
            else
            {
                //дістаємо електронну пошту
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                //якщо пошта є
                if (email != null)
                {
                    //шукаємо користувача за поштою
                    var user = await _userManager.FindByEmailAsync(email);
                    //якщо такого користувача немає
                    if (user == null)
                    {
                        //генеруємо нові дані користувача
                        user = new User
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                            UserSurname = info.Principal.FindFirstValue(ClaimTypes.Surname)
                        };
                        // і створюємо користувача в базі даних
                        await _userManager.CreateAsync(user);
                    }
                    //додаємо логування до таблиці AspNetLogins
                    await _userManager.AddLoginAsync(user, info);
                    //Автентифікуємо користувача
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //перенаправляємо на посилання перенаправлення
                    return LocalRedirect(returnUrl);
                }
                //якщо були помилки, то генеруємо їх
                ViewBag.ErrorTitle = $"Заява електронної пошти не була отримана від: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Будь ласка, зв'яжіться з підтримкою florestaofficial200.gmail.com";
                return View("Error");//і повертаємо сторінку помилок
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model) //модель, яка приймає введені дані користувача
        {
            if (ModelState.IsValid) //якщо всі дані були введені вірно
            {
                var user = await _userManager.FindByNameAsync(model.Email); //шукаємо користувача за Email
                if (user != null) //якшо користувач знайдений
                {
                    //тоді перевіряємо, чи електронна пошта була підтверджена
                    if (!await _userManager.IsEmailConfirmedAsync(user))//якщо ні
                    {
                        //відображаємо помилку
                        ModelState.AddModelError(string.Empty, "Ви не підтвердили свою пошту!");
                        return View(model);
                    }
                }
                //якщо все добре, то здійснюємо вхід
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                { //і повертаємо користувача на головну сторінку
                    return RedirectToAction("Index", "Home");
                }
                else
                { //якщо дані введені некоректно, то повертаємо помилку
                    ModelState.AddModelError("", "Невірний логін або(і) пароль");
                }
            }
            //повернення представлення
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            //здійснюємо вихід з сайту
            await _signInManager.SignOutAsync();
            //повертаємо на головну сторінку
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //приймає параметром модель, яка має інформацію про назву електронної пошти
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            //якщо передані дані вірні
            if (ModelState.IsValid)
            {
                //шукаємо користувача за електронною поштою
                var user = await _userManager.FindByEmailAsync(model.Email);
                //якщо такого користувача немає, або його пошта не була підтверджена
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return View("ForgotPasswordConfirmation");
                }
                //генеруємо код для посилання, яке буде в повідомленні електронної пошти
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //генеруємо посилання
                var callbackUrl = Url.Action("ResetPassword",
                    "Account",
                    new { userId = user.Id, code = code },
                    protocol: HttpContext.Request.Scheme);
                //генеруємо повідомлення електронної пошти
                EmailService emailService = new EmailService();

                await emailService.SendEmailAsync(model.Email,
                    "Зміна паролю",
                    $"Перейдіть за наступним посиланням, щоб змінити ваш пароль:" +
                    $" <a href='{callbackUrl}'>Змінити пароль</a>");

                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            //якщо код є, то поверне представлення
            //якщо ні, то поверне сторінку з помилками
            return code == null ? View("Error") : View(); 
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //модель, яка приймає електронну пошту і новий пароль користувача
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            //якщо дані були невірними
            if (!ModelState.IsValid)
            {//то повертаємо представлення
                return View(model);
            }
            //якщо все добре
            //то шукаємо користувача за електронною поштою
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) //якщо користувача не було знайдено
            {//повертаємо представлення
                return View("ResetPasswordConfirmation");
            }
            //скидаємо пароль 
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded) //якщо все вдалося
            {//повертаємо представлення
                return View("ResetPasswordConfirmation");
            }//якшо були помилки
            foreach (var error in result.Errors)
            {//виводимо їх
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
