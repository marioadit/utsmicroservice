using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Play.Transaction.Service.Dtos;

namespace Play.Transaction.Service.Clients
{
    public class CustomerClient
    {
        private readonly HttpClient httpClient;

        public CustomerClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<CustomerDto> GetCustomerAsync(Guid customerId)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<CustomerDto>($"/api/Customer/{customerId}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}