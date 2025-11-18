using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryManagementAPI.Data;

public interface IJsonStorageService
{
    Task<List<T>> LoadAsync<T>(string fileName) where T : class;
    Task SaveAsync<T>(string fileName, List<T> data) where T : class;
    Task<T?> GetByIdAsync<T>(string fileName, int id) where T : class;
    Task DeleteAsync<T>(string fileName, int id) where T : class;
}

public class JsonStorageService : IJsonStorageService
{
    private readonly string _storagePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService(IWebHostEnvironment environment)
    {
        _storagePath = Path.Combine(environment.ContentRootPath, "Data", "Storage");
        
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<List<T>> LoadAsync<T>(string fileName) where T : class
    {
        var filePath = Path.Combine(_storagePath, $"{fileName}.json");

        if (!File.Exists(filePath))
            return new List<T>();

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public async Task SaveAsync<T>(string fileName, List<T> data) where T : class
    {
        var filePath = Path.Combine(_storagePath, $"{fileName}.json");
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> GetByIdAsync<T>(string fileName, int id) where T : class
    {
        var data = await LoadAsync<T>(fileName);
        var property = typeof(T).GetProperty("Id");
        return data.FirstOrDefault(item => (int?)property?.GetValue(item) == id);
    }

    public async Task DeleteAsync<T>(string fileName, int id) where T : class
    {
        var data = await LoadAsync<T>(fileName);
        var property = typeof(T).GetProperty("Id");
        var itemToRemove = data.FirstOrDefault(item => (int?)property?.GetValue(item) == id);
        
        if (itemToRemove != null)
        {
            data.Remove(itemToRemove);
            await SaveAsync(fileName, data);
        }
    }
}
