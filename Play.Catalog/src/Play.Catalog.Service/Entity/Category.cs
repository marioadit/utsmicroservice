using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Universal;

namespace Play.Catalog.Service.Entity
{
    public class Category : IEntity
    {
        public Guid Id { get; init; }
        public string CategoryName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

    }
}