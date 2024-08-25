using Dapper;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class ExportUserCartProductDetailsPdfQuery : IRequest<byte[]> { }

public class ExportUserCartProductDetailsPdfHandler : IRequestHandler<ExportUserCartProductDetailsPdfQuery, byte[]>
{
    private readonly string _connectionString;

    public ExportUserCartProductDetailsPdfHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<byte[]> Handle(ExportUserCartProductDetailsPdfQuery request, CancellationToken cancellationToken)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var sql = "SELECT * FROM vw_UserCartProductDetails";
            var result = await connection.QueryAsync(sql);

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                foreach (var row in result)
                {
                    document.Add(new Paragraph($"{row.UserName}, {row.ProductName}, {row.Quantity}, {row.Email}"));
                }

                document.Close();
                return stream.ToArray();
            }
        }
    }
}

public static class ExportUserCartProductDetailsEndpoint
{
    public static void MapExportUserCartProductDetailsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/export/user-cart-product-details", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new ExportUserCartProductDetailsPdfQuery());
            return Results.File(result, "application/pdf", "UserCartProductDetails.pdf");
        })
        .WithName("ExportUserCartProductDetails")
        .WithTags("Export");
    }
}
