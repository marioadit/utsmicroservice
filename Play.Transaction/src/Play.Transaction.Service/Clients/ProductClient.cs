using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service.Clients
{
    public class ProductClient
    {
        private readonly HttpClient httpClient;

        public ProductClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ProductDto> GetProductAsync(Guid productId)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<ProductDto>($"/api/Product/{productId}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}