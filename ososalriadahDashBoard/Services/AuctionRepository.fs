namespace ososalriadahDashBoard.Services

open System
open System.Data
open Microsoft.FSharp.Core
open Dapper
open Microsoft.Data.SqlClient
open Microsoft.Extensions.Configuration
open ososalriadahDashBoard.Models

[<AllowNullLiteral>]
type IAuctionRepository =
    abstract member GetAll : unit -> Auction list
    abstract member GetById : int -> Auction option
    abstract member Add : Auction -> int
    abstract member Update : Auction -> unit
    abstract member MarkDeleted : int -> unit

module private SqlHelpers =
    let toDbString (value : string) =
        if String.IsNullOrWhiteSpace(value) then box DBNull.Value else box value

    let toDbNullable<'T when 'T : struct> (value : Nullable<'T>) =
        if value.HasValue then box value.Value else box DBNull.Value

    let selectColumns =
        "SELECT Id,
                Name,
                Start,
                [Content] AS Content,
                ImagePath,
                PdfPath,
                ISNULL(IsDeleted, 0) AS IsDeleted,
                ISNULL(IsPermanentlyDeleted, 0) AS IsPermanentlyDeleted,
                [End] AS EndDate,
                AgentName,
                City,
                District,
                OpeningPrice,
                Url
           FROM Auctions"

open SqlHelpers

type AuctionRepository (configuration : IConfiguration) =
    let connectionString = configuration.GetConnectionString("DefaultConnection")

    let createConnection () : IDbConnection =
        new SqlConnection(connectionString) :> IDbConnection

    interface IAuctionRepository with
        member _.GetAll () =
            use conn = createConnection ()
            conn.Query<Auction>(
                selectColumns + " WHERE ISNULL(IsDeleted, 0) = 0 ORDER BY Start DESC")
            |> Seq.toList

        member _.GetById (id : int) =
            use conn = createConnection ()
            let sql = selectColumns + " WHERE Id = @Id"
            conn.QuerySingleOrDefault<Auction>(sql, dict [ "Id", box id ])
            |> Option.ofObj

        member _.Add (auction : Auction) =
            use conn = createConnection ()
            let sql =
                "INSERT INTO Auctions (Name, Start, [Content], ImagePath, PdfPath, IsDeleted, IsPermanentlyDeleted, [End], AgentName, City, District, OpeningPrice, Url)\n                 VALUES (@Name, @Start, @Content, @ImagePath, @PdfPath, @IsDeleted, @IsPermanentlyDeleted, @EndDate, @AgentName, @City, @District, @OpeningPrice, @Url);\n                 SELECT CAST(SCOPE_IDENTITY() as int);"
            let parameters = DynamicParameters()
            parameters.Add("@Name", auction.Name)
            parameters.Add("@Start", toDbNullable auction.Start)
            parameters.Add("@Content", toDbString auction.Content)
            parameters.Add("@ImagePath", toDbString auction.ImagePath)
            parameters.Add("@PdfPath", toDbString auction.PdfPath)
            parameters.Add("@IsDeleted", auction.IsDeleted)
            parameters.Add("@IsPermanentlyDeleted", auction.IsPermanentlyDeleted)
            parameters.Add("@EndDate", toDbNullable auction.EndDate)
            parameters.Add("@AgentName", toDbString auction.AgentName)
            parameters.Add("@City", toDbString auction.City)
            parameters.Add("@District", toDbString auction.District)
            parameters.Add("@OpeningPrice", toDbNullable auction.OpeningPrice)
            parameters.Add("@Url", toDbString auction.Url)
            conn.QuerySingle<int>(sql, parameters)

        member _.Update (auction : Auction) =
            use conn = createConnection ()
            let sql =
                "UPDATE Auctions\n                    SET Name = @Name,\n                        Start = @Start,\n                        [Content] = @Content,\n                        ImagePath = @ImagePath,\n                        PdfPath = @PdfPath,\n                        IsDeleted = @IsDeleted,\n                        IsPermanentlyDeleted = @IsPermanentlyDeleted,\n                        [End] = @EndDate,\n                        AgentName = @AgentName,\n                        City = @City,\n                        District = @District,\n                        OpeningPrice = @OpeningPrice,\n                        Url = @Url\n                  WHERE Id = @Id"
            let parameters = DynamicParameters()
            parameters.Add("@Id", auction.Id)
            parameters.Add("@Name", auction.Name)
            parameters.Add("@Start", toDbNullable auction.Start)
            parameters.Add("@Content", toDbString auction.Content)
            parameters.Add("@ImagePath", toDbString auction.ImagePath)
            parameters.Add("@PdfPath", toDbString auction.PdfPath)
            parameters.Add("@IsDeleted", auction.IsDeleted)
            parameters.Add("@IsPermanentlyDeleted", auction.IsPermanentlyDeleted)
            parameters.Add("@EndDate", toDbNullable auction.EndDate)
            parameters.Add("@AgentName", toDbString auction.AgentName)
            parameters.Add("@City", toDbString auction.City)
            parameters.Add("@District", toDbString auction.District)
            parameters.Add("@OpeningPrice", toDbNullable auction.OpeningPrice)
            parameters.Add("@Url", toDbString auction.Url)
            conn.Execute(sql, parameters) |> ignore

        member _.MarkDeleted (id : int) =
            use conn = createConnection ()
            let sql = "UPDATE Auctions SET IsDeleted = 1 WHERE Id = @Id"
            conn.Execute(sql, dict [ "Id", box id ]) |> ignore
