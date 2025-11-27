namespace ososalriadahDashBoard.Controllers

open System
open System.Collections.Generic
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open ososalriadahDashBoard.Models
open ososalriadahDashBoard.Services

[<CLIMutable>]
type AuctionResponse =
    {
        id: int
        name: string
        start: Nullable<DateTime>
        [<JsonPropertyName("end")>]
        ``end``: Nullable<DateTime>
        content: string
        url: string
        imagePath: string
        pdfPath: string
        openingPrice: Nullable<decimal>
        agentName: string
        district: string
        city: string
        isDeleted: bool
        isPermanentlyDeleted: bool
    }

module private AuctionResponse =
    let fromAuction (auction: Auction) : AuctionResponse =
        {
            id = auction.Id
            name = auction.Name
            start = auction.Start
            ``end`` = auction.EndDate
            content = auction.Content
            url = auction.Url
            imagePath = auction.ImagePath
            pdfPath = auction.PdfPath
            openingPrice = auction.OpeningPrice
            agentName = auction.AgentName
            district = auction.District
            city = auction.City
            isDeleted = auction.IsDeleted
            isPermanentlyDeleted = auction.IsPermanentlyDeleted
        }

[<ApiController>]
[<Route("api/[controller]")>]
[<AllowAnonymous>]
type AuctionController(repository : IAuctionRepository) =
    inherit ControllerBase()

    [<HttpGet>]
    member this.Get() : ActionResult<IEnumerable<AuctionResponse>> =
        repository.GetAll()
        |> List.map AuctionResponse.fromAuction
        |> this.Ok
        |> ActionResult<IEnumerable<AuctionResponse>>
