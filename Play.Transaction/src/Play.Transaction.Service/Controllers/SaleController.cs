using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Transaction.Service.Clients;
using Play.Transaction.Service.Entity;
using Play.Universal;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : ControllerBase
    {
        private readonly IRepository<Sale> saleRepository;
        private readonly CustomerClient customerClient;
        private readonly IRepository<SaleItem> saleItemRepository;
        private readonly ProductClient productClient;

        public SaleController(IRepository<Sale> saleRepository, CustomerClient customerClient, IRepository<SaleItem> saleItemRepository, ProductClient productClient)
        {
            this.saleRepository = saleRepository;
            this.customerClient = customerClient;
            this.saleItemRepository = saleItemRepository;
            this.productClient = productClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDto>>> GetAllAsync()
        {
            var sales = await saleRepository.GetAllAsync();
            var salesDtos = new List<SaleDto>();

            foreach (var sale in sales)
            {
                var customer = await customerClient.GetCustomerAsync(sale.CustomerId);
                if (customer == null) continue;

                salesDtos.Add(new SaleDto(
                    sale.Id,
                    sale.CustomerId,
                    sale.SaleDate,
                    sale.TotalAmount,
                    sale.CreatedDate
                ));
            }

            return Ok(salesDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailSaleDto>> GetAsync(Guid id)
        {
            var sale = await saleRepository.GetAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            var customer = await customerClient.GetCustomerAsync(sale.CustomerId);
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            var saleDto = new DetailSaleDto(
                sale.Id,
                sale.CustomerId,
                customer.CustomerName,
                sale.SaleDate,
                sale.CreatedDate,
                sale.SaleItems.Select(si => new SaleItemDto(
                    si.Id,
                    si.SaleId,
                    si.ProductId,
                    si.Quantity,
                    si.Price,
                    si.CreatedDate
                )).ToList(),
                sale.TotalAmount
            );

            return Ok(saleDto);
        }

        [HttpPost]
        public async Task<ActionResult<SaleDto>> CreateAsync(CreateSaleDto createSaleDto)
        {
            if (createSaleDto.CustomerId == Guid.Empty)
            {
                return BadRequest("CustomerId is required.");
            }

            var customer = await customerClient.GetCustomerAsync(createSaleDto.CustomerId);
            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                CustomerId = createSaleDto.CustomerId,
                SaleDate = DateTimeOffset.UtcNow,
                TotalAmount = createSaleDto.TotalAmount,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await saleRepository.CreateAsync(sale);

            return CreatedAtAction(nameof(GetAsync), new { id = sale.Id }, sale.AsDtto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, UpdateSaleDto updateSaleDto)
        {
            if (updateSaleDto.CustomerId == Guid.Empty)
            {
                return BadRequest("CustomerId is required.");
            }

            var existingSale = await saleRepository.GetAsync(id);
            if (existingSale == null)
            {
                return NotFound();
            }

            var customer = await customerClient.GetCustomerAsync(updateSaleDto.CustomerId);
            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            existingSale.CustomerId = updateSaleDto.CustomerId;
            existingSale.TotalAmount = updateSaleDto.TotalAmount;

            await saleRepository.UpdateAsync(existingSale);

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingSale = await saleRepository.GetAsync(id);
            if (existingSale == null)
            {
                return NotFound("Sale not found.");
            }

            await saleRepository.DeleteAsync(id);

            return NoContent();
        }

    }
}