namespace ososalriadahDashBoard

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open ososalriadahDashBoard.Services

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()
            |> ignore

        builder.Services.AddRazorPages() |> ignore
        builder.Services.AddScoped<IAuctionRepository, AuctionRepository>() |> ignore

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
                options.LoginPath <- PathString("/Account/Login")
                options.AccessDeniedPath <- PathString("/Account/Login")
                options.SlidingExpiration <- true)
            |> ignore

        builder.Services.AddAuthorization() |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthentication()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        app.MapRazorPages()

        app.Run()

        exitCode
