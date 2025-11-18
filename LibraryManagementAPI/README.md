# Library Management API

A comprehensive ASP.NET Core 8 REST API for managing library books, borrowers, and products with **local JSON storage** (no database required).

## Features

### 📚 Book Management
- Add, Update, Delete, and View Books
- Fields: Id, Title, Author, ISBN, Genre, Quantity, PublishedDate, Publisher, Language, ShelfLocation
- Search by title, author, or genre with pagination
- Track book availability

### 👥 Borrower Management
- Add, Update, Delete, and View Borrowers
- Fields: Id, Name, ContactNumber, Email, MembershipId (unique), Address, MembershipStart/ExpiryDate
- Unique membership ID validation

### 📖 Borrow & Return System
- Borrow books with automatic quantity decrease
- Return books with automatic quantity increase
- Overdue tracking and fine calculation (Rs 10/day)
- BorrowRecord fields: BorrowDate, DueDate, ReturnDate, IsOverdue, FineAmount, Status

### 🛒 Product Management (12 fields max)
- ProductId, Name, Description, SKU (unique), Category, Price, QuantityInStock, Manufacturer, Weight, Dimensions, CreatedAt, IsActive
- Full CRUD operations
- Advanced search with category filter
- Sorting: price_asc, price_desc, name_asc, name_desc, newest, oldest
- Pagination support

### 🔗 Third-Party API Integration
- Fetch book information from OpenLibrary API
- Response caching with TTL (default: 3600s)
- Comprehensive API logging
- Local storage of external data
- Mock API support

### 📊 Logging & Monitoring
- Structured logging with Serilog
- API call logs with response times
- File-based log persistence
- Error tracking

## Tech Stack

- **Framework**: ASP.NET Core 8
- **Storage**: JSON files (local storage)
- **Caching**: In-Memory Cache
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

## Project Structure

```
LibraryManagementAPI/
├── Controllers/           # API endpoints
├── Services/             # Business logic
├── Models/               # Data models
├── Data/
│   ├── JsonStorageService.cs
│   └── Storage/          # JSON files storage
├── Middleware/           # Error handling
├── Utilities/            # Helper functions
├── Postman/              # Postman collection
├── logs/                 # Application logs
├── Program.cs            # Application entry
├── appsettings.json      # Configuration
└── LibraryManagementAPI.csproj
```

## Setup & Installation

### Prerequisites
- .NET 8 SDK
- Visual Studio Code or Visual Studio

### Steps

1. **Clone/Extract the repository**
   ```bash
   cd LibraryManagementAPI
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` or `http://localhost:5000`

4. **Access Swagger UI**
   Navigate to `https://localhost:5001/swagger` to test endpoints

## Configuration

Edit `appsettings.json`:

```json
{
  "DataStoragePath": "Data/Storage",
  "ExternalApis": {
    "BookInfoApiUrl": "https://openlibrary.org/isbn/{isbn}.json",
    "CacheTTLSeconds": 3600
  }
}
```

## API Endpoints

### Books
- `GET /api/books` - Get all books (with search, genre filter, pagination)
- `GET /api/books/{id}` - Get book by ID
- `GET /api/books/by-isbn/{isbn}` - Get book by ISBN
- `POST /api/books` - Add new book
- `PUT /api/books/{id}` - Update book
- `DELETE /api/books/{id}` - Delete book

### Borrowers
- `GET /api/borrowers` - Get all borrowers
- `GET /api/borrowers/{id}` - Get borrower by ID
- `GET /api/borrowers/by-membership/{membershipId}` - Get by membership ID
- `POST /api/borrowers` - Add new borrower
- `PUT /api/borrowers/{id}` - Update borrower
- `DELETE /api/borrowers/{id}` - Delete borrower

### Borrow & Return
- `POST /api/borrows/borrow` - Borrow a book
  ```json
  {
    "borrowerId": 1,
    "bookId": 1,
    "days": 14
  }
  ```
- `POST /api/borrows/return` - Return a book
  ```json
  {
    "borrowerId": 1,
    "bookId": 1
  }
  ```
- `GET /api/borrows` - Get all borrow records
- `GET /api/borrows/{id}` - Get borrow record by ID
- `GET /api/borrows/borrower/{borrowerId}` - Get active borrows for borrower
- `GET /api/borrows/overdue/list` - Get overdue records

### Products
- `GET /api/products` - Get all products (with search, category, sort, pagination)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/by-sku/{sku}` - Get product by SKU
- `POST /api/products` - Add new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product (soft delete)

### External APIs
- `GET /api/external/bookinfo/{isbn}` - Get book info from external API with caching
- `GET /api/external/logs` - Get API call logs

## Sample API Requests

### Add a Book
```bash
curl -X POST http://localhost:5000/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "isbn": "9780132350884",
    "genre": "Programming",
    "quantity": 5,
    "publishedDate": "2008-08-01",
    "publisher": "Prentice Hall",
    "language": "English",
    "shelfLocation": "A-101"
  }'
```

### Borrow a Book
```bash
curl -X POST http://localhost:5000/api/borrows/borrow \
  -H "Content-Type: application/json" \
  -d '{
    "borrowerId": 1,
    "bookId": 1,
    "days": 14
  }'
```

### Search Books
```bash
curl "http://localhost:5000/api/books?searchTerm=Clean&genre=Programming&page=1&pageSize=10"
```

## Data Storage

All data is stored in JSON files in the `Data/Storage/` directory:
- `Books.json`
- `Borrowers.json`
- `BorrowRecords.json`
- `Products.json`
- `ExternalBookInfo.json`
- `ApiLogs.json`

## Logging

Application logs are saved to `logs/app-YYYYMMDD.txt` with daily rotation.

## Error Handling

All endpoints return standardized error responses:

```json
{
  "message": "Error description",
  "error": "Detailed error message",
  "timestamp": "2024-11-18T10:30:00Z"
}
```

## Testing

Use the included Postman collection (`Postman/LibraryManagementAPI.postman_collection.json`) to test all endpoints.

### Import to Postman:
1. Open Postman
2. Click "Import"
3. Select the collection file
4. Start testing

## Project Features Checklist

✅ Full CRUD operations for Books, Borrowers, Products
✅ Borrow/Return system with fines
✅ Search, Filter, and Pagination
✅ Third-party API integration with caching
✅ Comprehensive logging
✅ Error handling middleware
✅ Swagger/OpenAPI documentation
✅ Local JSON storage (no database)
✅ Validation and business logic
✅ Postman collection included

## Future Enhancements

- Unit tests
- Authentication/Authorization
- Database integration option
- Email notifications
- Reservation system
- User dashboard

## License

MIT

## Support

For issues and questions, please contact the development team.
