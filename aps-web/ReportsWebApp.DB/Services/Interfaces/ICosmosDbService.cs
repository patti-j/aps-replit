using Microsoft.Azure.Cosmos;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface ICosmosDbService<TEntity> where TEntity : class
{
    /// <summary>
    /// Adds a new item to the Cosmos DB container asynchronously.
    /// </summary>
    /// <param name="item">The item to add to the container.</param>
    /// <param name="id">The partition key value for the item.</param>
    /// <returns>The response from the Cosmos DB operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is null or whitespace.</exception>
    /// <exception cref="CosmosException">Thrown if a Cosmos DB client-side or server-side error occurs.</exception>
    Task<ItemResponse<TEntity>> AddItemAsync(TEntity item, string id);
    Task<ItemResponse<TEntity>> UpdateItemAsync(TEntity item, string id, bool allowCreate = false);
    Task<TEntity> GetItemAsync(string id);
    Task DeleteItemAsync(string id);
}