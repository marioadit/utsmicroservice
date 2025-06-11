using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service.Clients
{
    public class CustomerClient
    {
        private readonly HttpClient httpClient;
        private readonly AsyncRetryPolicy retryPolicy;

        public CustomerClient(HttpClient httpClient)
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

        public async Task<DetailCustomerDto> GetCustomerAsync(Guid customerId)
        {
            try
            {
                return await retryPolicy.ExecuteAsync(async () =>
                {
                    return await httpClient.GetFromJsonAsync<DetailCustomerDto>($"/api/Customer/{customerId}");
                });
            }
            catch (HttpRequestException ex)
            {
                // Log error jika diperlukan
                Console.WriteLine($"Request failed: {ex.Message}");
                return null;
            }
        }
    }
}