namespace ososalriadahDashBoard.Models

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.AspNetCore.Http

type Auction() =
    [<Key>]
    member val Id : int = 0 with get, set

    [<Required(ErrorMessage = "Name is required")>]
    [<StringLength(250)>]
    member val Name : string = null with get, set

    [<Display(Name = "Start Time")>]
    [<DataType(DataType.DateTime)>]
    member val Start : Nullable<DateTime> = Nullable() with get, set

    [<Display(Name = "Details")>]
    member val Content : string = null with get, set

    [<Display(Name = "Image Path")>]
    member val ImagePath : string = null with get, set

    [<NotMapped>]
    member val ImageFile : IFormFile = null with get, set

    [<Display(Name = "PDF Path")>]
    member val PdfPath : string = null with get, set

    [<Display(Name = "Deleted")>]
    member val IsDeleted : bool = false with get, set

    [<Display(Name = "Permanently Deleted")>]
    member val IsPermanentlyDeleted : bool = false with get, set

    [<Display(Name = "End Time")>]
    [<DataType(DataType.DateTime)>]
    member val EndDate : Nullable<DateTime> = Nullable() with get, set

    [<Display(Name = "Agent Name")>]
    member val AgentName : string = null with get, set

    member val City : string = null with get, set

    member val District : string = null with get, set

    [<Display(Name = "Opening Price")>]
    [<DataType(DataType.Currency)>]
    member val OpeningPrice : Nullable<decimal> = Nullable() with get, set

    [<Url>]
    member val Url : string = null with get, set

type LoginViewModel() =
    [<Required>]
    member val Username : string = null with get, set

    [<Required>]
    [<DataType(DataType.Password)>]
    member val Password : string = null with get, set

    member val RememberMe : bool = false with get, set

    member val ReturnUrl : string = null with get, set
