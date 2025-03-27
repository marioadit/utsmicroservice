using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Customer.Service
{
    public class Dtos
    {
        public record CustomerDto(int Id, [Required] string CustomerName, string ContactNumber, string Email, string Address);
        public record CreateCustomerDto([Required] string CustomerName, string ContactNumber, string Email, string Address);
        public record UpdateCustomerDto([Required] string CustomerName, string ContactNumber, string Email, string Address);
    }
}