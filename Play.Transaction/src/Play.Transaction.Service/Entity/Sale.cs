using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Universal;

namespace Play.Transaction.Service.Entity
{
    public class Sale : IEntity
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; set; }
        public DateTimeOffset SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}