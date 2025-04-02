using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Universal;

namespace Play.Catalog.Service.Entity
{
    public class Product : IEntity
    {
        public Guid Id { get; init; }
        public string ProductName { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}