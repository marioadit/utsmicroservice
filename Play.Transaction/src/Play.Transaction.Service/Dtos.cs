using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Transaction.Service
{
    public class Dtos
    {
        public record SaleDto(Guid Id, Guid CustomerId, DateTimeOffset SaleDate, decimal TotalAmount, DateTimeOffset CreatedDate);
        public record DetailSaleDto(Guid Id, Guid CustomerId, string CustomerName, DateTimeOffset SaleDate, DateTimeOffset CreatedDate, IEnumerable<SaleItemDto> SaleItems, decimal TotalAmount);
        public record CreateSaleDto(Guid CustomerId, decimal TotalAmount);
        public record UpdateSaleDto(Guid CustomerId, decimal TotalAmount);
        public record SaleItemDto(Guid Id, Guid SaleId, Guid ProductId, int Quantity, decimal Price, DateTimeOffset CreatedDate);
        public record SaleItemsDto
(
    Guid Id,
    Guid SaleId,
    Guid ProductId,
    string ProductName, // ⬅️ HARUS ADA
    int Quantity,
    decimal Price,
    DateTimeOffset CreatedDate
);

        public record CreateSaleItemDto(Guid SaleId, Guid ProductId, int Quantity, decimal Price);
        public record UpdateSaleItemDto(Guid SaleId, Guid ProductId, int Quantity, decimal Price);
        public record ProductDto(Guid Id, string ProductName, Guid CategoryId, decimal Price, int StockQuantity, string Description, DateTimeOffset CreatedDate);
        public record CustomerDto(Guid Id, string CustomerName);
        public record DetailCustomerDto(Guid Id, string CustomerName, string Email, string PhoneNumber, DateTimeOffset CreatedDate);
        public record UpdateProductDto(string ProductName, Guid CategoryId, decimal Price, int StockQuantity, string Description);


    }
}