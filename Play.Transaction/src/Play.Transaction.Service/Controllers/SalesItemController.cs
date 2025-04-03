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
    public class SalesItemController : ControllerBase
    {
        private readonly IRepository<SaleItem> saleItemRepository;
        private readonly IRepository<Sale> saleRepository; // Tambahkan repositori Sale untuk validasi SaleId
        private readonly ProductClient productClient;

        public SalesItemController(IRepository<SaleItem> saleItemRepository, IRepository<Sale> saleRepository, ProductClient productClient)
        {
            this.saleItemRepository = saleItemRepository;
            this.saleRepository = saleRepository; // Inisialisasi repositori Sale
            this.productClient = productClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetAllAsync()
        {
            var saleItems = await saleItemRepository.GetAllAsync();
            var saleItemDtos = new List<SaleItemDto>();

            foreach (var saleItem in saleItems)
            {
                var product = await productClient.GetProductAsync(saleItem.ProductId);
                if (product == null) continue;

                saleItemDtos.Add(saleItem.AsDtto());
            }

            return Ok(saleItemDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleItemDto>> GetAsync(Guid id)
        {
            var saleItem = await saleItemRepository.GetAsync(id);
            if (saleItem is null)
            {
                return NotFound();
            }

            return Ok(saleItem.AsDtto());
        }

        [HttpPost]
        public async Task<ActionResult<SaleItemDto>> CreateAsync(CreateSaleItemDto createSaleItemDto)
        {
            if (createSaleItemDto.SaleId == Guid.Empty)
            {
                return BadRequest("SaleId is required.");
            }

            // Validasi SaleId apakah ada pada Sale
            var sale = await saleRepository.GetAsync(createSaleItemDto.SaleId);
            if (sale == null)
            {
                return BadRequest("Sale not found.");
            }

            var product = await productClient.GetProductAsync(createSaleItemDto.ProductId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            var saleItem = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = createSaleItemDto.SaleId,
                ProductId = createSaleItemDto.ProductId,
                Quantity = createSaleItemDto.Quantity,
                Price = createSaleItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await saleItemRepository.CreateAsync(saleItem);

            return CreatedAtAction(nameof(GetAsync), new { id = saleItem.Id }, saleItem.AsDtto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, UpdateSaleItemDto updateSaleItemDto)
        {
            if (updateSaleItemDto.SaleId == Guid.Empty)
            {
                return BadRequest("SaleId is required.");
            }

            // Validasi SaleId apakah ada pada Sale
            var sale = await saleRepository.GetAsync(updateSaleItemDto.SaleId);
            if (sale == null)
            {
                return BadRequest("Sale not found.");
            }

            var existingSaleItem = await saleItemRepository.GetAsync(id);
            if (existingSaleItem == null)
            {
                return NotFound();
            }

            var product = await productClient.GetProductAsync(updateSaleItemDto.ProductId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            existingSaleItem.SaleId = updateSaleItemDto.SaleId;
            existingSaleItem.ProductId = updateSaleItemDto.ProductId;
            existingSaleItem.Quantity = updateSaleItemDto.Quantity;
            existingSaleItem.Price = updateSaleItemDto.Price;

            await saleItemRepository.UpdateAsync(existingSaleItem);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var existingSaleItem = await saleItemRepository.GetAsync(id);
            if (existingSaleItem == null)
            {
                return NotFound();
            }

            await saleItemRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}