namespace ososalriadahDashBoard.Controllers

open System
open System.IO
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open ososalriadahDashBoard.Models
open ososalriadahDashBoard.Services

[<Authorize>]
type AuctionsController (repository : IAuctionRepository, webHostEnvironment : IWebHostEnvironment) =
    inherit Controller()

    member private this.SaveImage (auction : Auction) =
        if not (isNull auction.ImageFile) && auction.ImageFile.Length > 0L then
            let uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads")
            Directory.CreateDirectory(uploadsFolder) |> ignore

            let extension = Path.GetExtension(auction.ImageFile.FileName)
            let fileName = String.Concat(Guid.NewGuid().ToString("N"), extension)
            let filePath = Path.Combine(uploadsFolder, fileName)

            use stream = new FileStream(filePath, FileMode.Create)
            auction.ImageFile.CopyTo(stream)

            let relativePath = Path.Combine("uploads", fileName).Replace("\\", "/")
            auction.ImagePath <- relativePath

    member this.Index () : IActionResult =
        let auctions = repository.GetAll()
        this.View(auctions) :> IActionResult

    member this.Create () : IActionResult =
        this.View(new Auction()) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Create (auction : Auction) =
        if this.ModelState.IsValid then
            this.SaveImage(auction)
            repository.Add(auction) |> ignore
            this.TempData.["Success"] <- "Auction created successfully."
            this.RedirectToAction("Index") :> IActionResult
        else
            this.View(auction) :> IActionResult

    member this.Edit (id : int) =
        match repository.GetById(id) with
        | Some auction -> this.View(auction) :> IActionResult
        | None -> this.NotFound() :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Edit (id : int, auction : Auction) =
        if id <> auction.Id then
            this.BadRequest() :> IActionResult
        elif this.ModelState.IsValid then
            this.SaveImage(auction)
            repository.Update(auction)
            this.TempData.["Success"] <- "Auction updated successfully."
            this.RedirectToAction("Index") :> IActionResult
        else
            this.View(auction) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Delete (id : int) =
        repository.MarkDeleted(id)
        this.TempData.["Success"] <- "Auction deleted successfully."
        this.RedirectToAction("Index") :> IActionResult
