using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service.Clients
{
    public class ProductClient
    {
        private readonly HttpClient httpClient;
        private readonly AsyncRetryPolicy retryPolicy;

        public ProductClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            // Konfigurasi Polly untuk retry hingga 3 kali
            retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
                {
                    // Log setiap percobaan gagal
                    Console.WriteLine($"Retry {retryCount} failed. Waiting {timeSpan} before next retry.");

                    // Jika sudah mencapai batas retry, matikan service
                    if (retryCount == 3)
                    {
                        Console.WriteLine("Maximum retry attempts reached. Shutting down the service.");
                        Environment.Exit(1); // Exit dengan kode error
                    }
                });
        }

        public async Task<ProductDto> GetProductAsync(Guid productId)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(async () =>
                {
                    return await httpClient.GetFromJsonAsync<ProductDto>($"/api/Product/{productId}");
                });
            }
            catch (HttpRequestException ex)
            {
                // Log error jika diperlukan
                Console.WriteLine($"Request failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProductAsync(Guid productId, UpdateProductDto updateProductDto)
        {
            try
            {
                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    return await httpClient.PutAsJsonAsync($"/api/Product/{productId}", updateProductDto);
                });

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Update request failed: {ex.Message}");
                return false;
            }
        }

    }
}