using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebAppApi.Database;

namespace WebAppApi.Features.ExportExcel
{
    public class ExportUserCartProductViewExcel
    {
        // Query per l'export in Excel
        public class Query : IRequest<byte[]>
        {
        }

        // Handler per l'export in Excel
        public class Handler : IRequestHandler<Query, byte[]>
        {
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context;
            }

            public async Task<byte[]> Handle(Query request, CancellationToken cancellationToken)
            {
                // Interroga la vista direttamente
                var result = await _context.VwUserCartProductDetails.ToListAsync(cancellationToken);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("UserCartProductDetails");

                    // Aggiungi intestazioni
                    worksheet.Cell(1, 1).Value = "UserName";
                    worksheet.Cell(1, 2).Value = "ProductName";
                    worksheet.Cell(1, 3).Value = "Quantity";
                    worksheet.Cell(1, 4).Value = "Email";

                    // Aggiungi dati
                    int currentRow = 2;
                    foreach (var row in result)
                    {
                        worksheet.Cell(currentRow, 1).Value = row.UserName;
                        worksheet.Cell(currentRow, 2).Value = row.ProductName;
                        worksheet.Cell(currentRow, 3).Value = row.Quantity;
                        worksheet.Cell(currentRow, 4).Value = row.Email;
                        currentRow++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
        }

        // Endpoint per l'export in Excel
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/export/user-cart-product-view-excel", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new Query());
                return Results.File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserCartProductDetails.xlsx");
            })
            .RequireAuthorization()
            .WithName("ExportUserCartProductViewExcel")
            .WithTags("Export");
        }
    }
}
