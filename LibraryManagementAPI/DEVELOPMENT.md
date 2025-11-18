# Development Guide

## Local JSON Storage Implementation

This project uses **local JSON file storage** instead of a database. All data is persisted in JSON files located in `Data/Storage/` directory.

### How It Works

1. **JsonStorageService**: Core service that handles all file I/O operations
   - `LoadAsync<T>()`: Loads data from JSON file
   - `SaveAsync<T>()`: Saves data to JSON file
   - `GetByIdAsync<T>()`: Retrieves specific record by ID
   - `DeleteAsync<T>()`: Deletes a record and updates the file

2. **Data Files**:
   - `Books.json` - Book catalog
   - `Borrowers.json` - Library members
   - `BorrowRecords.json` - Borrow/Return transactions
   - `Products.json` - Product inventory
   - `ExternalBookInfo.json` - Cached external API data
   - `ApiLogs.json` - API call logs

### Advantages

- **No database setup required** - Just clone and run
- **Easy to understand** - Plain JSON files
- **Perfect for testing** - Export/import data easily
- **Suitable for prototyping** - Quick development cycles
- **Cross-platform** - Works on Windows, Linux, macOS

### Migration to Database

To upgrade to a real database (SQL Server, PostgreSQL, etc.):

1. Install Entity Framework Core packages:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. Create a DbContext class
3. Replace IJsonStorageService with Entity Framework
4. Add migrations and update the database

## Architecture

### Layered Architecture

```
Controllers (API Endpoints)
    ↓
Services (Business Logic)
    ↓
JsonStorageService (Data Access)
    ↓
JSON Files (Storage)
```

### Service Layer

All business logic is in the `Services/` folder:
- **BookService**: Book management
- **BorrowerService**: Member management  
- **BorrowService**: Borrowing logic, fines calculation
- **ProductService**: Product catalog
- **ExternalApiService**: Third-party API integration

### Controller Layer

RESTful endpoints in `Controllers/` folder:
- **BooksController**: `/api/books`
- **BorrowersController**: `/api/borrowers`
- **BorrowsController**: `/api/borrows`
- **ProductsController**: `/api/products`
- **ExternalApiController**: `/api/external`

## Running the Application

### Local Development

1. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access API**:
   - Swagger UI: `https://localhost:5001/swagger` (or `http://localhost:5000`)
   - API Base: `https://localhost:5001/api`

### With Watch Mode (Auto-reload)

```bash
dotnet watch run
```

## Testing with Postman

1. Open Postman
2. Import the collection: `Postman/LibraryManagementAPI.postman_collection.json`
3. Update the base URL if different from `http://localhost:5000`
4. Test all endpoints

## Logging

Logs are written to two locations:
1. **Console**: Real-time output during development
2. **File**: `logs/app-YYYYMMDD.txt` with daily rotation

Configure logging in `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

## Error Handling

All endpoints return standardized error responses:

```json
{
  "message": "User-friendly error message",
  "error": "Technical error details",
  "timestamp": "2024-11-18T10:30:00Z"
}
```

HTTP Status Codes:
- `200` - Success
- `201` - Created
- `204` - No Content (deletion)
- `400` - Bad Request (validation error)
- `404` - Not Found
- `500` - Internal Server Error

## Caching Strategy

### API Response Caching

External API responses are cached in memory:
- **Default TTL**: 3600 seconds (1 hour)
- **Cache Key**: `book_info_{isbn}`
- **Local Storage**: Also persisted to `ExternalBookInfo.json`

Configure cache TTL in `appsettings.json`:
```json
"ExternalApis": {
  "CacheTTLSeconds": 3600
}
```

## Validation

### Input Validation

All endpoints validate input:
- Required fields check
- Data type validation
- Business rule validation (e.g., unique SKU, membership expiry)
- Quantity/Price non-negative checks

### Automatic Validations

- Membership ID uniqueness
- SKU uniqueness  
- Book ISBN validation
- Borrower expiry date check
- Book quantity availability

## Performance Considerations

### Pagination

All list endpoints support pagination:
```bash
GET /api/books?page=1&pageSize=10
```

- Default page size: 10
- Maximum recommended: 100

### Sorting (Products)

Products support multiple sort options:
- `price_asc` - Price ascending
- `price_desc` - Price descending
- `name_asc` - Name A-Z
- `name_desc` - Name Z-A
- `newest` - Recently created
- `oldest` - Oldest first

### Filtering

- **Books**: Search by title/author, filter by genre
- **Products**: Search by name/SKU/description, filter by category
- **Borrows**: Filter by borrower, get overdue records

## Extension Points

### Adding a New Module

1. Create model in `Models/`
2. Create service in `Services/`
3. Create controller in `Controllers/`
4. Register service in `Program.cs`
5. Add JSON storage file handling in service

### Integrating a Different External API

1. Add configuration in `appsettings.json`
2. Create new method in `ExternalApiService`
3. Add caching logic
4. Create endpoint in `ExternalApiController`
5. Add to Postman collection

## Troubleshooting

### Data Not Persisting

- Check if `Data/Storage/` directory exists
- Verify write permissions to the directory
- Check console logs for errors

### API Not Starting

- Verify port 5000/5001 is not in use
- Check for syntax errors: `dotnet build`
- Review error logs

### External API Calls Failing

- Verify internet connection
- Check API endpoint URL in `appsettings.json`
- Review API logs: `GET /api/external/logs`
- Check response time metrics

## Best Practices

1. **Always validate input** - Use the validation provided
2. **Check logs** - Use Serilog to debug issues
3. **Pagination** - Always use pagination for large datasets
4. **Error handling** - Catch and log all exceptions
5. **Caching** - Use cache for frequently accessed data
6. **Testing** - Use Postman to test all endpoints

## Next Steps

- Add authentication/authorization
- Implement database with Entity Framework Core
- Add unit tests with xUnit
- Create Docker image
- Deploy to Azure

---

For questions or issues, refer to the README.md file.
