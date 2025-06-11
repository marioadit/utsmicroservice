using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Transaction.Service.Entity;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service
{
    public static class Extension
    {
        public static SaleDto AsDtto(this Sale sale)
        {
            return new SaleDto(sale.Id, sale.CustomerId, sale.SaleDate, sale.TotalAmount, sale.CreatedDate);
        }
        public static SaleItemDto AsDtto(this SaleItem saleItem)
        {
            return new SaleItemDto(saleItem.Id, saleItem.SaleId, saleItem.ProductId, saleItem.Quantity, saleItem.Price, saleItem.CreatedDate);
        }
        public static SaleItemsDto AsDtto(this SaleItem saleItem, ProductDto product)
        {
            return new SaleItemsDto(
                saleItem.Id,
                saleItem.SaleId,
                saleItem.ProductId,
                product.ProductName,
                saleItem.Quantity,
                saleItem.Price,
                saleItem.CreatedDate
            );
        }

    }
}