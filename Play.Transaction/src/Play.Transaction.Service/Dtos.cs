using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Transaction.Service
{
    public class Dtos
    {
        public record SaleDto(Guid Id, Guid CustomerId, DateTimeOffset SaleDate, decimal TotalAmount, DateTimeOffset CreatedDate);
        public record CreateSaleDto(Guid CustomerId, decimal TotalAmount);
        public record UpdateSaleDto(Guid CustomerId, decimal TotalAmount);
        public record SaleItemDto(Guid Id, Guid SaleId, Guid ProductId, int Quantity, decimal Price, DateTimeOffset CreatedDate);
        public record CreateSaleItemDto(Guid SaleId, Guid ProductId, int Quantity, decimal Price);
        public record UpdateSaleItemDto(Guid SaleId, Guid ProductId, int Quantity, decimal Price);
        public record ProductDto(Guid Id, string ProductName);
        public record CustomerDto(Guid Id, string CustomerName);
    }
}