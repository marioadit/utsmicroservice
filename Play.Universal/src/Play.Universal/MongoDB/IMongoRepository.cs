using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Play.Universal.MongoDB
{
    public class IMongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public IMongoRepository(IMongoDatabase database, string collectionName)
        {
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task CreateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(T => T.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<T> GetAsync(int id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(T => T.Id, id);
            return await dbCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            FilterDefinition<T> filter = filterBuilder.Eq(existingT => existingT.Id, entity.Id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }
    }
}