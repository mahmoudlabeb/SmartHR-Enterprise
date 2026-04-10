using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartHR.Models;
using SmartHR.Services;
using SmartHR.ViewModels;

namespace SmartHR.Controllers
{
    public class AccountController : Controller
    {
        // ✅ FIX C1: Use ApplicationUser (not base IdentityUser) so that
        //    custom fields (FullName, DepartmentId, etc.) are accessible and
        //    the DI registration matches the one in Program.cs.
        private readonly UserManager<ApplicationUser>   _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService                  _emailService;
        private readonly ILogger<AccountController>     _logger;
        private readonly IWebHostEnvironment            _env;

        public AccountController(
            UserManager<ApplicationUser>   userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService                  emailService,
            ILogger<AccountController>     logger,
            IWebHostEnvironment            env)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _emailService  = emailService;
            _logger        = logger;
            _env           = env;
        }

        // ── 1. Login (GET) ────────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ── 2. Login (POST) ───────────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !user.IsActive)
            {
                _logger.LogWarning("Inactive user {Email} attempted login.", model.Email);
                ModelState.AddModelError(string.Empty, "حسابك قيد المراجعة في انتظار موافقة الإدارة.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);

                var loggedInUser = await _userManager.FindByEmailAsync(model.Email);
                if (loggedInUser != null && await _userManager.IsInRoleAsync(loggedInUser, AppRoles.Client))
                {
                    return RedirectToAction("Products", "Home");
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} is locked out.", model.Email);
                ModelState.AddModelError(string.Empty,
                    "الحساب مقفل مؤقتاً بسبب محاولات دخول فاشلة. حاول مرة أخرى بعد قليل.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty,
                "فشل تسجيل الدخول. تأكد من البريد الإلكتروني وكلمة المرور.");
            return View(model);
        }

        // ── 2a. Register (GET) ────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // ── 2b. Register (POST) ───────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني مسجل مسبقاً.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = false, // Must be approved by an admin
                HireDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password. Email: {Email}", model.Email);
                return RedirectToAction(nameof(RegisterConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ── 2c. Register Confirmation (GET) ───────────────────────────────
        [AllowAnonymous]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        // ── 3. Logout (POST) ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }

        // ── 4. Forgot Password (GET) ──────────────────────────────────────
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ── 5. Forgot Password (POST) — ✅ FIX M6: email now actually sent ─
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (model == null) return View();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Security: do NOT reveal whether the email exists in our database
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            // Generate the reset token
            var code        = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword", "Account",
                new { userId = user.Id, code },
                protocol: Request.Scheme);

            // ✅ FIX M6: Actually send the email (was commented out before)
            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "SmartHR — استعادة كلمة المرور",
                    $@"<div dir='rtl' style='font-family:Arial;'>
                        <h2>استعادة كلمة المرور</h2>
                        <p>مرحباً {user.FullName},</p>
                        <p>لقد تلقينا طلب إعادة تعيين كلمة مرور حسابك.</p>
                        <p>
                            <a href='{callbackUrl}'
                               style='background:#007bff;color:#fff;padding:10px 20px;
                                      text-decoration:none;border-radius:5px;display:inline-block;'>
                                إعادة تعيين كلمة المرور
                            </a>
                        </p>
                        <p>إذا لم تطلب ذلك، يمكنك تجاهل هذه الرسالة.</p>
                    </div>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password-reset email to {Email}", model.Email);
                // Silently continue — don't reveal internal errors to the user
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // ── 6. Forgot Password Confirmation (GET) ─────────────────────────
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ── 7. Reset Password (GET) ───────────────────────────────────────
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
                return BadRequest("كود إعادة التعيين مطلوب.");
            return View(new ResetPasswordViewModel { Code = code });
        }

        // ── 8. Reset Password (POST) ──────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ── 9. Reset Password Confirmation (GET) ──────────────────────────
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ── 10. Access Denied ─────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── 11. Dev-only: Force-reset seed passwords ───────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> ResetSeedPasswords()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var seedAccounts = new[]
            {
                ("superadmin@smarthr.com", "Admin@123"),
                ("admin@smarthr.com",      "Admin@123"),
                ("hr@smarthr.com",         "HrAdmin@123"),
                ("manager@smarthr.com",    "Manager@123!"),
                ("employee@smarthr.com",   "Emp@123!"),
                ("employee2@smarthr.com",  "Emp@123!"),
                ("employee3@smarthr.com",  "Emp@123!"),
                ("client@smarthr.com",     "Client@123!"),
            };

            var results = new System.Text.StringBuilder();
            foreach (var (email, password) in seedAccounts)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    results.AppendLine($"❌ NOT FOUND: {email}");
                    continue;
                }

                // Ensure user is active & email confirmed
                user.IsActive = true;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // Reset lockout if locked out
                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, null);

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                results.AppendLine(result.Succeeded
                    ? $"✅ OK: {email} → {password}"
                    : $"❌ FAILED: {email} — {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return Content(results.ToString(), "text/plain");
        }
    }
}