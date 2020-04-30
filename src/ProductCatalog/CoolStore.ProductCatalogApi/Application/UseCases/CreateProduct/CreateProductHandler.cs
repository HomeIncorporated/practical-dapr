using System;
using System.Threading;
using System.Threading.Tasks;
using CoolStore.ProductCatalogApi.Domain;
using CoolStore.ProductCatalogApi.Dtos;
using CoolStore.ProductCatalogApi.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using N8T.Infrastructure.Data;

namespace CoolStore.ProductCatalogApi.Application.UseCases.CreateProduct
{
    public class ProductCreatedHandler
        : IRequestHandler<CreateProductCommand, CatalogProductDto>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public ProductCreatedHandler(
            ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [TransactionScope]
        public async Task<CatalogProductDto> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = Product.Of(
                Guid.NewGuid(),
                request.Name,
                request!.Description,
                request.Price,
                request.ImageUrl,
                request.StoreId,
                request.CategoryId);

            var cats = await _dbContext.Categories.ToListAsync(cancellationToken: cancellationToken);
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken: cancellationToken);

            if (category == null) throw new NullReferenceException($"Couldn't find out any Category # {request.CategoryId}");
            product.AssignCategory(category);

            var entityCreated = await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var productCreated = entityCreated.Entity;

            return new CatalogProductDto
            {
                Id = productCreated.Id,
                Name = productCreated.Name,
                Description = productCreated.Description,
                ImageUrl = productCreated.ImageUrl,
                Price = productCreated.Price
            };
        }
    }
}
