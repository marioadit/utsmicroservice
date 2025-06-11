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
        private readonly RabbitMqPublisher rabbitMqPublisher;

        public SalesItemController(
            IRepository<SaleItem> saleItemRepository,
            IRepository<Sale> saleRepository,
            ProductClient productClient,
            RabbitMqPublisher rabbitMqPublisher)
        {
            this.saleItemRepository = saleItemRepository;
            this.saleRepository = saleRepository;
            this.productClient = productClient;
            this.rabbitMqPublisher = rabbitMqPublisher;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleItemsDto>>> GetAllAsync()
        {
            var saleItems = await saleItemRepository.GetAllAsync();
            var saleItemDtos = new List<SaleItemsDto>();

            foreach (var saleItem in saleItems)
            {
                var product = await productClient.GetProductAsync(saleItem.ProductId);
                if (product == null) continue;

                saleItemDtos.Add(saleItem.AsDtto(product));
            }

            return Ok(saleItemDtos);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleItemsDto>> GetAsync(Guid id)
        {
            var saleItem = await saleItemRepository.GetAsync(id);
            if (saleItem is null)
            {
                return NotFound();
            }

            var product = await productClient.GetProductAsync(saleItem.ProductId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            return Ok(saleItem.AsDtto(product));

        }

        [HttpPost]
        public async Task<ActionResult<SaleItemDto>> CreateAsync(CreateSaleItemDto createSaleItemDto)
        {
            if (createSaleItemDto.SaleId == Guid.Empty)
            {
                return BadRequest("SaleId is required.");
            }

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

            // Kurangi stok
            int newStock = product.StockQuantity - createSaleItemDto.Quantity;
            if (newStock < 0)
            {
                return BadRequest("Not enough stock.");
            }

            var updateProductDto = new UpdateProductDto(
                product.ProductName,
                product.CategoryId,
                product.Price,
                newStock,
                product.Description
            );

            bool updateSuccess = await productClient.UpdateProductAsync(product.Id, updateProductDto);
            if (!updateSuccess)
            {
                return StatusCode(500, "Failed to update product stock.");
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

            var stockChangeMessage = new
            {
                ProductId = saleItem.ProductId,
                ProductName = product.ProductName,
                QuantityChanged = -saleItem.Quantity,
                Action = "StockDecreased",
                Source = "SalesItemController.Create",
                Timestamp = DateTimeOffset.UtcNow
            };

            await rabbitMqPublisher.PublishMessageAsync(stockChangeMessage);

            return CreatedAtAction(nameof(GetAsync), new { id = saleItem.Id }, saleItem.AsDtto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, UpdateSaleItemDto updateSaleItemDto)
        {
            if (updateSaleItemDto.SaleId == Guid.Empty)
            {
                return BadRequest("SaleId is required.");
            }

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

            int oldQuantity = existingSaleItem.Quantity;
            int newQuantity = updateSaleItemDto.Quantity;
            int quantityDiff = newQuantity - oldQuantity;
            int newStock = product.StockQuantity - quantityDiff;

            if (newStock < 0)
            {
                return BadRequest("Not enough stock for update.");
            }

            var updateProductDto = new UpdateProductDto(
                product.ProductName,
                product.CategoryId,
                product.Price,
                newStock,
                product.Description
            );

            bool updateSuccess = await productClient.UpdateProductAsync(product.Id, updateProductDto);
            if (!updateSuccess)
            {
                return StatusCode(500, "Failed to update product stock.");
            }

            existingSaleItem.SaleId = updateSaleItemDto.SaleId;
            existingSaleItem.ProductId = updateSaleItemDto.ProductId;
            existingSaleItem.Quantity = newQuantity;
            existingSaleItem.Price = updateSaleItemDto.Price;

            await saleItemRepository.UpdateAsync(existingSaleItem);

            var stockChangeMessage = new
            {
                ProductId = existingSaleItem.ProductId,
                ProductName = product.ProductName,
                QuantityChanged = -quantityDiff, // bisa positif (nambah stok) atau negatif (kurangi)
                Action = "StockAdjusted",
                Source = "SalesItemController.Update",
                Timestamp = DateTimeOffset.UtcNow
            };

            await rabbitMqPublisher.PublishMessageAsync(stockChangeMessage);

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

            var product = await productClient.GetProductAsync(existingSaleItem.ProductId);
            if (product != null)
            {
                int newStock = product.StockQuantity + existingSaleItem.Quantity;

                var updateProductDto = new UpdateProductDto(
                    product.ProductName,
                    product.CategoryId,
                    product.Price,
                    newStock,
                    product.Description
                );

                bool updateSuccess = await productClient.UpdateProductAsync(product.Id, updateProductDto);
                if (!updateSuccess)
                {
                    return StatusCode(500, "Failed to restore product stock.");
                }
            }

            await saleItemRepository.DeleteAsync(id);
            var stockChangeMessage = new
            {
                ProductId = existingSaleItem.ProductId,
                ProductName = product?.ProductName ?? "Unknown Product",
                QuantityChanged = existingSaleItem.Quantity, // restore
                Action = "StockRestored",
                Source = "SalesItemController.Delete",
                Timestamp = DateTimeOffset.UtcNow
            };

            await rabbitMqPublisher.PublishMessageAsync(stockChangeMessage);

            return NoContent();
        }
        // Temporary test endpoint in your controller
        [HttpGet("test-rabbit")]
        public async Task<IActionResult> TestRabbit()
        {
            try
            {
                var testMessage = new { Test = "Hello RabbitMQ", Time = DateTime.UtcNow };
                await rabbitMqPublisher.PublishMessageAsync(testMessage);
                return Ok("Message published");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed: {ex.Message}");
            }
        }
    }
}