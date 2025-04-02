using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Catalog.Service
{
    public class Dtos
    {
        public record ProductDto(Guid Id, string ProductName, Guid CategoryId, decimal Price, int StockQuantity, string Description, DateTimeOffset CreatedDate);
        public record CreateProductDto(string ProductName, Guid CategoryId, decimal Price, int StockQuantity, string Description);
        public record UpdateProductDto(string ProductName, Guid CategoryId, decimal Price, int StockQuantity, string Description);
        public record CategoryDto(Guid Id, string CategoryName, DateTimeOffset CreatedDate);
        public record CreateCategoryDto(string CategoryName);
        public record UpdateCategoryDto(string CategoryName);
    }
}