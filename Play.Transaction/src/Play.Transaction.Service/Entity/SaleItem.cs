using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Universal;

namespace Play.Transaction.Service.Entity
{
    public class SaleItem : IEntity
    {
        public Guid Id { get; init; }
        public Guid SaleId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}