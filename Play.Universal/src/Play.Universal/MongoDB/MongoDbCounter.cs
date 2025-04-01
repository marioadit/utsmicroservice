using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Play.Universal.MongoDB
{
    public class MongoDbCounter
    {
        private readonly IMongoCollection<Counter> _counterCollection;
        private readonly FilterDefinitionBuilder<Counter> _filterBuilder = Builders<Counter>.Filter;

        public MongoDbCounter(IMongoDatabase database)
        {
            _counterCollection = database.GetCollection<Counter>("Counters");
        }

        public async Task<int> GetNextSequenceAsync(string entityName)
        {
            var filter = _filterBuilder.Eq(c => c.Name, entityName);
            var update = Builders<Counter>.Update.Inc(c => c.Value, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var counter = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            return counter.Value;
        }
    }

    public class Counter
    {
        public string Name { get; set; }  // Stores entity type name
        public int Value { get; set; }    // Last assigned ID
    }
}