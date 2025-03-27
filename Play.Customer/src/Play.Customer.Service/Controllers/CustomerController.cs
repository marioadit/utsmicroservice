using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Customer.Service.Entity;
using Play.Universal;
using static Play.Customer.Service.Dtos;

namespace Play.Customer.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<Customers> customerRepository;
        public CustomerController(IRepository<Customers> customerRepository)
        {
            this.customerRepository = customerRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await customerRepository.GetAllAsync();
            return customers.Select(customer => customer.AsDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetAsync(int id)
        {
            var customer = await customerRepository.GetAsync(id);
            if (customer is null)
            {
                return NotFound();
            }
            return customer.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> PostAsync(CreateCustomerDto customerDto)
        {
            var customer = new Customers
            {
                CustomerName = customerDto.CustomerName,
                ContactNumber = customerDto.ContactNumber,
                Email = customerDto.Email,
                Address = customerDto.Address
            };
            await customerRepository.CreateAsync(customer);
            return CreatedAtAction(nameof(GetAsync), new { id = customer.Id }, customer.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, UpdateCustomerDto customerDto)
        {
            var existingCustomer = await customerRepository.GetAsync(id);
            if (existingCustomer is null)
            {
                return NotFound();
            }
            existingCustomer.CustomerName = customerDto.CustomerName;
            existingCustomer.ContactNumber = customerDto.ContactNumber;
            existingCustomer.Email = customerDto.Email;
            existingCustomer.Address = customerDto.Address;
            await customerRepository.UpdateAsync(existingCustomer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var existingCustomer = await customerRepository.GetAsync(id);
            if (existingCustomer is null)
            {
                return NotFound();
            }
            await customerRepository.DeleteAsync(existingCustomer.Id);
            return NoContent();
        }
    }
}