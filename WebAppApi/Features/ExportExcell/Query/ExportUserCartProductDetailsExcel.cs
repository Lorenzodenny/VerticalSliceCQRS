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
    public class ExportUserCartProductDetailsExcel
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
                var result = await _context.Users
                    .Join(_context.Carts, u => u.UserId, c => c.UserId, (u, c) => new { u, c })
                    .Join(_context.CartProducts, uc => uc.c.CartId, cp => cp.CartId, (uc, cp) => new { uc, cp })
                    .Join(_context.Products, uccp => uccp.cp.ProductId, p => p.ProductId, (uccp, p) => new
                    {
                        uccp.uc.u.UserName,
                        uccp.uc.u.Email,
                        p.ProductName,
                        uccp.cp.Quantity
                    })
                    .ToListAsync(cancellationToken);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("UserCartProductDetails");

                    worksheet.Cell(1, 1).Value = "UserName";
                    worksheet.Cell(1, 2).Value = "ProductName";
                    worksheet.Cell(1, 3).Value = "Quantity";
                    worksheet.Cell(1, 4).Value = "Email";

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
            app.MapGet("/api/export/user-cart-product-details-excel", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new Query());
                return Results.File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserCartProductDetails.xlsx");
            })
            .WithName("ExportUserCartProductDetailsExcel")
            .WithTags("Export");
        }
    }
}
