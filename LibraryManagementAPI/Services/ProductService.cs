using LibraryManagementAPI.Models;
using LibraryManagementAPI.Data;

namespace LibraryManagementAPI.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync(string? search = null, string? category = null, string? sort = null, int page = 1, int pageSize = 10);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(int id, Product product);
    Task DeleteAsync(int id);
    Task<Product?> GetBySKUAsync(string sku);
}

public class ProductService : IProductService
{
    private readonly IJsonStorageService _storage;
    private readonly ILogger<ProductService> _logger;
    private const string FileName = "Products";

    public ProductService(IJsonStorageService storage, ILogger<ProductService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync(string? search = null, string? category = null, string? sort = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var products = await _storage.LoadAsync<Product>(FileName);
            products = products.Where(p => p.IsActive).ToList();

            // Search filter
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            // Category filter
            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            // Sorting
            products = sort?.ToLower() switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                "oldest" => products.OrderBy(p => p.CreatedAt).ToList(),
                _ => products
            };

            // Pagination
            var totalCount = products.Count;
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            _logger.LogInformation($"Retrieved {products.Count} products with filters: search={search}, category={category}, sort={sort}");
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            return await _storage.GetByIdAsync<Product>(FileName, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product with ID {id}");
            throw;
        }
    }

    public async Task<Product> AddAsync(Product product)
    {
        try
        {
            var products = await _storage.LoadAsync<Product>(FileName);

            // Check for duplicate SKU
            if (products.Any(p => p.SKU.Equals(product.SKU, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"SKU {product.SKU} already exists");

            product.ProductId = products.Any() ? products.Max(p => p.ProductId) + 1 : 1;
            product.CreatedAt = DateTime.UtcNow;
            products.Add(product);
            await _storage.SaveAsync(FileName, products);
            _logger.LogInformation($"Product added with ID {product.ProductId}: {product.Name}");
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            throw;
        }
    }

    public async Task<Product> UpdateAsync(int id, Product product)
    {
        try
        {
            var products = await _storage.LoadAsync<Product>(FileName);
            var existingProduct = products.FirstOrDefault(p => p.ProductId == id);

            if (existingProduct == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            product.ProductId = id;
            product.CreatedAt = existingProduct.CreatedAt;

            products[products.IndexOf(existingProduct)] = product;
            await _storage.SaveAsync(FileName, products);
            _logger.LogInformation($"Product updated with ID {id}");
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product with ID {id}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var products = await _storage.LoadAsync<Product>(FileName);
            var product = products.FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                product.IsActive = false;
                products[products.IndexOf(product)] = product;
                await _storage.SaveAsync(FileName, products);
                _logger.LogInformation($"Product soft-deleted with ID {id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product with ID {id}");
            throw;
        }
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        try
        {
            var products = await _storage.LoadAsync<Product>(FileName);
            return products.FirstOrDefault(p => p.SKU.Equals(sku, StringComparison.OrdinalIgnoreCase) && p.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product with SKU {sku}");
            throw;
        }
    }
}
