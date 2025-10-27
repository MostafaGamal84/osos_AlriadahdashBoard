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

    member private this.EnsureUploadsFolder () =
        let ensureWebRoot () =
            if String.IsNullOrWhiteSpace(webHostEnvironment.WebRootPath) then
                let contentRoot =
                    if String.IsNullOrWhiteSpace(webHostEnvironment.ContentRootPath) then
                        Directory.GetCurrentDirectory()
                    else
                        webHostEnvironment.ContentRootPath

                let fallbackRoot = Path.Combine(contentRoot, "wwwroot")
                if not (Directory.Exists(fallbackRoot)) then
                    Directory.CreateDirectory(fallbackRoot) |> ignore

                webHostEnvironment.WebRootPath <- fallbackRoot
                fallbackRoot
            else
                webHostEnvironment.WebRootPath

        let webRoot = ensureWebRoot ()
        let uploadsFolder = Path.Combine(webRoot, "uploads")
        Directory.CreateDirectory(uploadsFolder) |> ignore
        uploadsFolder

    member private this.SaveImage (auction : Auction) =
        let ensureDefaultImage () =
            if String.IsNullOrWhiteSpace(auction.ImagePath) then
                auction.ImagePath <- AuctionDefaults.DefaultImageUrl

        let imageFile = auction.ImageFile

        if isNull imageFile then
            ensureDefaultImage()
            true
        else
            let hasContent =
                try
                    imageFile.Length > 0L
                with
                | :? NotSupportedException
                | :? InvalidOperationException -> true

            if not hasContent then
                ensureDefaultImage()
                true
            else
                try
                    let uploadsFolder = this.EnsureUploadsFolder()

                    let extension =
                        let ext = Path.GetExtension(imageFile.FileName)
                        if String.IsNullOrWhiteSpace(ext) then "" else ext

                    let fileName = String.Concat(Guid.NewGuid().ToString("N"), extension)
                    let filePath = Path.Combine(uploadsFolder, fileName)

                    use destinationStream =
                        new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)

                    try
                        imageFile.CopyTo(destinationStream)
                    with ex ->
                        // Reset the stream and retry through a buffered copy if the provider does not allow direct CopyTo
                        destinationStream.SetLength(0L)
                        use sourceStream = imageFile.OpenReadStream()
                        sourceStream.CopyTo(destinationStream)

                    destinationStream.Flush()

                    let relativePath = $"/uploads/{fileName}"
                    auction.ImagePath <- relativePath
                    true
                with ex ->
                    this.ModelState.AddModelError("ImageFile", $"Could not save the selected image. {ex.Message}")
                    false

    member this.Index () : IActionResult =
        let auctions = repository.GetAll()
        this.View(auctions) :> IActionResult

    member this.Create () : IActionResult =
        this.View(new Auction()) :> IActionResult

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Create (auction : Auction) =
        if this.ModelState.IsValid && this.SaveImage(auction) then
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
        elif this.ModelState.IsValid && this.SaveImage(auction) then
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
