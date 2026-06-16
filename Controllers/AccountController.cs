using e_commerce_web_customer.Application.Contracts;
using e_commerce_web_customer.Application.Constants;
using e_commerce_web_customer.Application.Account;
using e_commerce_web_customer.Application.Services;
using e_commerce_web_customer.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce_web_customer.Controllers;

public sealed class AccountController(
    IAccountService accountService,
    IAccountProfilePageProvider accountProfilePageProvider,
    IAccountOrderDetailProvider accountOrderDetailProvider,
    CartSessionService cartSession) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var success = await accountService.LoginAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            cancellationToken);
        if (success)
        {
            var profile = await accountService.GetProfileAsync(
                model.Email,
                cancellationToken);

            HttpContext.Session.SetString(SessionKeys.IsLoggedIn, "true");
            HttpContext.Session.SetString(SessionKeys.UserEmail, profile?.Email ?? model.Email);
            HttpContext.Session.SetString(
                SessionKeys.UserDisplayName,
                profile?.DisplayName ?? ResolveDisplayName(model.Email));
            if (!string.IsNullOrWhiteSpace(profile?.PhoneNumber))
            {
                HttpContext.Session.SetString(SessionKeys.UserPhoneNumber, profile.PhoneNumber.Trim());
            }
            else
            {
                HttpContext.Session.Remove(SessionKeys.UserPhoneNumber);
            }

            TempData["AuthSuccess"] = "Đăng nhập thành công! Chào mừng bạn quay lại TechStore.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            string.Empty,
            "Email hoặc mật khẩu không chính xác, hoặc tài khoản chưa tồn tại.");

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        cartSession.Clear();
        cartSession.ClearBuyNow();
        HttpContext.Session.Remove(SessionKeys.IsLoggedIn);
        HttpContext.Session.Remove(SessionKeys.UserEmail);
        HttpContext.Session.Remove(SessionKeys.UserDisplayName);
        HttpContext.Session.Remove(SessionKeys.UserPhoneNumber);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile(
        string? tab = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsLoggedIn())
        {
            var returnUrl = Url.Action(
                nameof(Profile),
                "Account",
                new { tab = AccountProfileTabs.Normalize(tab) });
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var model = await accountProfilePageProvider.GetProfilePageAsync(
            HttpContext.Session.GetString(SessionKeys.UserEmail),
            HttpContext.Session.GetString(SessionKeys.UserDisplayName),
            HttpContext.Session.GetString(SessionKeys.UserPhoneNumber),
            AccountProfileTabs.Normalize(tab),
            cancellationToken);

        return View(model);
    }

    [HttpGet("account/orders/{code}")]
    public async Task<IActionResult> OrderDetail(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!IsLoggedIn())
        {
            var returnUrl = Url.Action(
                nameof(OrderDetail),
                "Account",
                new { code });
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var model = await accountOrderDetailProvider.GetOrderDetailAsync(
            HttpContext.Session.GetString(SessionKeys.UserEmail),
            HttpContext.Session.GetString(SessionKeys.UserDisplayName),
            HttpContext.Session.GetString(SessionKeys.UserPhoneNumber),
            code,
            cancellationToken);

        return model is null
            ? NotFound()
            : View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await accountService.UserExistsAsync(
            model.Email,
            cancellationToken);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký sử dụng.");
            return View(model);
        }

        var success = await accountService.RegisterAsync(
            model,
            cancellationToken);
        if (success)
        {
            TempData["AuthSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(
            string.Empty,
            "Đăng ký thất bại. Đã xảy ra lỗi, vui lòng thử lại.");

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        TempData["AuthNotice"] =
            "Khôi phục mật khẩu cần được kết nối với dịch vụ gửi email trước khi sử dụng.";

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider, string? source = null)
    {
        var providerName = provider?.ToLowerInvariant() switch
        {
            "google" => "Google",
            "facebook" => "Facebook",
            _ => null
        };

        if (providerName is null)
        {
            return BadRequest();
        }

        TempData["AuthNotice"] =
            $"Kết nối {providerName} cần được cấu hình trước khi sử dụng đăng nhập mạng xã hội.";

        return string.Equals(source, nameof(Register), StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(Register))
            : RedirectToAction(nameof(Login));
    }

    private static string ResolveDisplayName(string email)
    {
        return email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? email;
    }

    private bool IsLoggedIn()
    {
        return HttpContext.Session.GetString(SessionKeys.IsLoggedIn) == "true";
    }
}
