namespace ososalriadahDashBoard.Controllers

open System.Collections.Generic
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open ososalriadahDashBoard.Models
open ososalriadahDashBoard.Services

[<ApiController>]
[<Route("api/[controller]")>]
[<AllowAnonymous>]
type AuctionController(repository : IAuctionRepository) =
    inherit ControllerBase()

    [<HttpGet>]
    member this.Get() : ActionResult<IEnumerable<Auction>> =
        let auctions = repository.GetAll()
        ActionResult<IEnumerable<Auction>>(this.Ok(auctions))
