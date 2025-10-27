namespace ososalriadahDashBoard.Services

open System
open System.Data
open Microsoft.Data.SqlClient
open Dapper
open Microsoft.FSharp.Core
open Microsoft.Data.SqlClient
open Dapper
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

    let toDbNullable (value : Nullable<_>) =
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
        member _.GetAll () : Auction list =
            use conn = createConnection ()
            SqlMapper.Query<Auction>(
                conn,
                selectColumns + " WHERE ISNULL(IsDeleted, 0) = 0 ORDER BY Start DESC")
            |> Seq.toList

        member _.GetById (id : int) : Auction option =
            use conn = createConnection ()
            let sql = selectColumns + " WHERE Id = @Id"
            let parameters = DynamicParameters()
            parameters.Add("@Id", id)
            SqlMapper.QuerySingleOrDefault<Auction>(conn, sql, parameters)
            |> Option.ofObj

        member _.Add (auction : Auction) : int =
            use conn = createConnection ()
            let sql =
                String.concat "\n" [
                    "INSERT INTO Auctions (Name, Start, [Content], ImagePath, PdfPath, IsDeleted, IsPermanentlyDeleted, [End], AgentName, City, District, OpeningPrice, Url)"
                    "VALUES (@Name, @Start, @Content, @ImagePath, @PdfPath, @IsDeleted, @IsPermanentlyDeleted, @EndDate, @AgentName, @City, @District, @OpeningPrice, @Url);"
                    "SELECT CAST(SCOPE_IDENTITY() as int);"
                ]
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
            SqlMapper.QuerySingle<int>(conn, sql, parameters)

        member _.Update (auction : Auction) : unit =
            use conn = createConnection ()
            let sql =
                String.concat "\n" [
                    "UPDATE Auctions"
                    "    SET Name = @Name,"
                    "        Start = @Start,"
                    "        [Content] = @Content,"
                    "        ImagePath = @ImagePath,"
                    "        PdfPath = @PdfPath,"
                    "        IsDeleted = @IsDeleted,"
                    "        IsPermanentlyDeleted = @IsPermanentlyDeleted,"
                    "        [End] = @EndDate,"
                    "        AgentName = @AgentName,"
                    "        City = @City,"
                    "        District = @District,"
                    "        OpeningPrice = @OpeningPrice,"
                    "        Url = @Url"
                    "  WHERE Id = @Id"
                ]
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
            SqlMapper.Execute(conn, sql, parameters) |> ignore

        member _.MarkDeleted (id : int) : unit =
            use conn = createConnection ()
            let sql = "UPDATE Auctions SET IsDeleted = 1 WHERE Id = @Id"
            let parameters = DynamicParameters()
            parameters.Add("@Id", id)
            SqlMapper.Execute(conn, sql, parameters) |> ignore
