using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Universal;

namespace Play.Customer.Service.Entity
{
    public class Customers : IEntity
    {
        public Guid Id { get; init; }
        public string CustomerName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}