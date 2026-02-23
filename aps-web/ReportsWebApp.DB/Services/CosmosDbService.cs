using System.Net;

using Microsoft.Azure.Cosmos;
using Microsoft.CSharp.RuntimeBinder;

using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.DB.Services.SchedulerHelpers;

namespace ReportsWebApp.DB.Services
{
    public class CosmosDbService<TEntity> : ICosmosDbService<TEntity> where TEntity : class
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _container;
        private readonly Boolean _blocked;

        public CosmosDbService(CosmosClient cosmosClient, Database database, Container container)
        {
            _cosmosClient = cosmosClient;
            _database = database;
            _container = container;
        }

        public CosmosDbService()
        {
            _blocked = true;
        }

        /// <summary>
        /// Adds a new item to the Cosmos DB container asynchronously.
        /// </summary>
        /// <param name="item">The item to add to the container.</param>
        /// <param name="id">The partition key value for the item.</param>
        /// <returns>The response from the Cosmos DB operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is null or whitespace.</exception>
        /// <exception cref="CosmosException">Thrown if a Cosmos DB client-side or server-side error occurs.</exception>
        public async Task<ItemResponse<TEntity>> AddItemAsync(TEntity item, string id)
        {
            if(_blocked)
            {
                return null;
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }


            try
            {
                if (((dynamic)item).id != id)
                {
                    throw new ArgumentException("Id on item does not match provided id.", nameof(id));
                }
            }
            catch (RuntimeBinderException e)
            {
                throw new ArgumentException("Item does not have an id property.", nameof(item), e);
            }
            

            try
            {
                // Attempt to add the item to the Cosmos DB container
                return await _container.CreateItemAsync(item, new PartitionKey(id));
            }
            catch (CosmosException ex)
            {
                // Log the exception details
                // Consider logging details like ex.Message or ex.StatusCode as needed
                throw; // Re-throw the exception to be handled by the caller
            }
        }

        public async Task<ItemResponse<TEntity>> UpdateItemAsync(TEntity item, string id, bool allowCreate = false)
        {
            if(_blocked)
            {
                return null;
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }

            try
            {
                if (((dynamic)item).id.ToString() != id)
                {
                    throw new ArgumentException("ID of item and passed parameter do not match", nameof(id));
                }
            }
            catch (RuntimeBinderException e)
            {
                throw new ArgumentException("Item does not have an id property.", nameof(item), e);
            }
            

            try
            {
                var response = await _container.UpsertItemAsync(item, new PartitionKey(id));
                if (response.StatusCode == HttpStatusCode.OK || (response.StatusCode == HttpStatusCode.Created && allowCreate))
                {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.Created)
                {
                    //we dont want to create a new item, something bad has happened if we do
                    await DeleteItemAsync(id);
                    throw new InvalidOperationException("Item id did not match an existing item, cosmos item was either deleted or was never created.");
                }
                else
                {
                    throw new Exception($"Error updating item. Status Code: {response.StatusCode}");
                }
            }
            catch (CosmosException e)
            {
                throw;
            }
        }
        
        // Retrieves an item from the Cosmos DB container asynchronously
        public async Task<TEntity> GetItemAsync(string id)
        {
            if (_blocked)
            {
                return null;
            }
            try
            {
                ItemResponse<TEntity> response = await _container.ReadItemAsync<TEntity>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // Return null if item is not found
            }
        }

        // Deletes an item from the Cosmos DB container asynchronously
        public async Task DeleteItemAsync(string id)
        {
            if (_blocked)
            {
                return;
            }
            await _container.DeleteItemAsync<TEntity>(id, new PartitionKey(id));
        }
    }
}
