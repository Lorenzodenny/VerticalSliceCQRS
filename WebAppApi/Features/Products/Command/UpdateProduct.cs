﻿using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Product;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Products.Command
{
    public class UpdateProduct
    {
        // STEP 1) creo il comando CQRS
        public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<ProductVm>;

        // STEP 2) fluent validator
        public class Validator : AbstractValidator<UpdateProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.ProductId).GreaterThan(0).WithMessage("ProductId deve essere maggiore di 0");
                RuleFor(x => x.Request.ProductName).NotEmpty().WithMessage("Il nome del prodotto non può essere vuoto");
            }
        }

        // STEP 3) creo l'handler
        public class Handler : IRequestHandler<UpdateProductCommand, ProductVm>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<UpdateProductCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<UpdateProductCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<ProductVm> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    var existingProduct = await _unitOfWork.Context.Products.FindAsync(new object[] { request.Request.ProductId }, cancellationToken);
                    if (existingProduct is null || existingProduct.IsDeleted)
                    {
                        throw new Exception("Prodotto non trovato o cancellato.");
                    }

                    existingProduct.ProductName = request.Request.ProductName;
                    _unitOfWork.Context.Products.Update(existingProduct);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    return new ProductVm(
                        existingProduct.ProductId,
                        existingProduct.ProductName,
                        existingProduct.IsDeleted,
                        null);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
        }


        // STEP 4 l'endpoint
        public static void MapUpdateProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/products/{id:int}",
            async (int id, UpdateProductCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var updatedCommand = command with { Request = command.Request with { ProductId = id } };
                    var result = await sender.Send(updatedCommand);
                    return Results.Ok(result);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .RequireAuthorization()
            .WithName("UpdateProduct")
            .Produces<ProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");
        }



    }
}
