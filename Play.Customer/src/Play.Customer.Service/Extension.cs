using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Play.Customer.Service.Entity;
using static Play.Customer.Service.Dtos;

namespace Play.Customer.Service
{
    public static class Extension
    {
        public static CustomerDto AsDto(this Customers customer)
        {
            return new CustomerDto( customer.Id, customer.CustomerName, customer.ContactNumber, customer.Email, customer.Address);
        }
    }
}