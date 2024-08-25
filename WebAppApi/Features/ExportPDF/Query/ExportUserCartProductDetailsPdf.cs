using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebAppApi.Database;

namespace WebAppApi.Features.ExportPDF
{
    public class ExportUserCartProductDetailsPdf
    {
        // Query per l'export in PDF
        public class Query : IRequest<byte[]>
        {
        }

        // Handler per l'export in PDF
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

        // Endpoint per l'export in PDF
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/export/user-cart-product-details-pdf", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new Query());
                return Results.File(result, "application/pdf", "UserCartProductDetails.pdf");
            })
            .WithName("ExportUserCartProductDetailsPdf")
            .WithTags("Export");
        }
    }
}
