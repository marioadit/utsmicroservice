using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Catalog.Service.Entity;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service
{
    public static class Extension
    {
        public static ProductDto AsDto(this Product product)
        {
            return new ProductDto(product.Id, product.ProductName, product.CategoryId, product.Price, product.StockQuantity, product.Description, product.CreatedDate);
        }
        public static CategoryDto AsDto(this Category category)
        {
            return new CategoryDto(category.Id, category.CategoryName, category.CreatedDate);
        }
    }
}