namespace ososalriadahDashBoard.Controllers

open System
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open ososalriadahDashBoard.Models

[<AllowAnonymous>]
type AccountController (configuration : IConfiguration) =
    inherit Controller()

    member private this.ValidateCredentials (model : LoginViewModel) =
        let username = configuration.GetValue<string>("AdminCredentials:Username")
        let password = configuration.GetValue<string>("AdminCredentials:Password")
        String.Equals(model.Username, username, StringComparison.OrdinalIgnoreCase)
        && String.Equals(model.Password, password, StringComparison.Ordinal)

    member this.Login (returnUrl : string) =
        let isAuthenticated =
            not (isNull this.User)
            && not (isNull this.User.Identity)
            && this.User.Identity.IsAuthenticated

        if isAuthenticated then
            this.RedirectToAction("Index", "Auctions") :> IActionResult
        else
            let model = new LoginViewModel(ReturnUrl = returnUrl)
            this.View(model) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Login (model : LoginViewModel) =
        if this.ModelState.IsValid then
            if this.ValidateCredentials(model) then
                let claims = [ Claim(ClaimTypes.Name, model.Username) ]
                let identity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                let principal = ClaimsPrincipal(identity)
                let authProperties = AuthenticationProperties(IsPersistent = model.RememberMe)
                this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties)
                    .GetAwaiter()
                    .GetResult()
                match model.ReturnUrl with
                | returnUrl when not (string.IsNullOrEmpty(returnUrl)) && this.Url.IsLocalUrl(returnUrl) ->
                    this.Redirect(returnUrl) :> IActionResult
                | _ -> this.RedirectToAction("Index", "Auctions") :> IActionResult
            else
                this.ModelState.AddModelError(string.Empty, "Invalid username or password.")
                this.View(model) :> IActionResult
        else
            this.View(model) :> IActionResult

    [<Authorize>]
    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Logout () =
        this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            .GetAwaiter()
            .GetResult()
        this.RedirectToAction("Login") :> IActionResult
