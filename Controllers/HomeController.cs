using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using e_commerce_web_customer.Application.Account;
using e_commerce_web_customer.Application.Constants;
using e_commerce_web_customer.Application.Home;
using e_commerce_web_customer.Models;

namespace e_commerce_web_customer.Controllers;

public class HomeController : Controller
{
    private readonly IHomePageViewModelFactory _homePageViewModelFactory;
    private readonly IHeaderAccountProvider _headerAccountProvider;

    public HomeController(
        IHomePageViewModelFactory homePageViewModelFactory,
        IHeaderAccountProvider headerAccountProvider)
    {
        _homePageViewModelFactory = homePageViewModelFactory;
        _headerAccountProvider = headerAccountProvider;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _homePageViewModelFactory.CreateAsync(cancellationToken);
        model.Hero.Account = await GetCurrentAccountAsync(cancellationToken);
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private Task<ViewModels.Shared.HeaderAccountViewModel> GetCurrentAccountAsync(
        CancellationToken cancellationToken)
    {
        var isLoggedIn = HttpContext.Session.GetString(SessionKeys.IsLoggedIn) == "true";
        var userEmail = HttpContext.Session.GetString(SessionKeys.UserEmail);
        var userDisplayName = HttpContext.Session.GetString(SessionKeys.UserDisplayName);
        var userPhoneNumber = HttpContext.Session.GetString(SessionKeys.UserPhoneNumber);

        return _headerAccountProvider.GetAccountAsync(
            isLoggedIn,
            userEmail,
            userDisplayName,
            userPhoneNumber,
            cancellationToken);
    }
}
