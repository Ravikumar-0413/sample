# Quick Start Guide

## 5-Minute Setup

### Prerequisites
- .NET 8 SDK installed
- Postman (optional, for testing)

### Steps

1. **Navigate to project**:
   ```bash
   cd LibraryManagementAPI
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Access Swagger UI**:
   - Open browser: `https://localhost:5001/swagger`
   - Or `http://localhost:5000`

4. **Test with sample requests**:

   **Add a Book**:
   ```bash
   curl -X POST http://localhost:5000/api/books \
     -H "Content-Type: application/json" \
     -d '{
       "title":"The Pragmatic Programmer",
       "author":"Hunt & Thomas",
       "isbn":"9780201616224",
       "genre":"Programming",
       "quantity":3,
       "publishedDate":"1999-10-20",
       "publisher":"Addison-Wesley",
       "language":"English",
       "shelfLocation":"B-202"
     }'
   ```

   **Add a Borrower**:
   ```bash
   curl -X POST http://localhost:5000/api/borrowers \
     -H "Content-Type: application/json" \
     -d '{
       "name":"Alice Johnson",
       "contactNumber":"9876543210",
       "email":"alice@example.com",
       "membershipId":"MEM0001",
       "address":"456 Oak Ave",
       "membershipStartDate":"2024-01-01",
       "membershipExpiryDate":"2025-12-31"
     }'
   ```

   **Borrow a Book**:
   ```bash
   curl -X POST http://localhost:5000/api/borrows/borrow \
     -H "Content-Type: application/json" \
     -d '{
       "borrowerId":1,
       "bookId":1,
       "days":14
     }'
   ```

   **Return a Book**:
   ```bash
   curl -X POST http://localhost:5000/api/borrows/return \
     -H "Content-Type: application/json" \
     -d '{
       "borrowerId":1,
       "bookId":1
     }'
   ```

## Using Postman

1. **Import Collection**:
   - Open Postman
   - Click "Import"
   - Select `Postman/LibraryManagementAPI.postman_collection.json`

2. **Test Endpoints**:
   - All endpoints are pre-configured
   - Click "Send" to test

## Project Features

✅ **Book Management** - Add, update, delete, search books
✅ **Borrower Management** - Manage library members
✅ **Borrow/Return System** - Track loans and calculate fines
✅ **Product Catalog** - Manage products with inventory
✅ **External API Integration** - Fetch book data with caching
✅ **Search & Pagination** - Filter and paginate results
✅ **Error Handling** - Comprehensive error messages
✅ **Logging** - Full activity logging
✅ **Swagger Documentation** - Interactive API docs
✅ **Local JSON Storage** - No database needed

## Data Storage

All data is stored in JSON files:
- `Data/Storage/Books.json`
- `Data/Storage/Borrowers.json`
- `Data/Storage/BorrowRecords.json`
- `Data/Storage/Products.json`
- `Data/Storage/ExternalBookInfo.json`
- `Data/Storage/ApiLogs.json`

## API Endpoints Quick Reference

### Books
- `GET /api/books` - List all books
- `POST /api/books` - Add new book
- `GET /api/books/{id}` - Get book details
- `PUT /api/books/{id}` - Update book
- `DELETE /api/books/{id}` - Delete book

### Borrowers
- `GET /api/borrowers` - List all borrowers
- `POST /api/borrowers` - Add new borrower
- `GET /api/borrowers/{id}` - Get borrower details
- `PUT /api/borrowers/{id}` - Update borrower
- `DELETE /api/borrowers/{id}` - Delete borrower

### Borrow & Return
- `POST /api/borrows/borrow` - Borrow a book
- `POST /api/borrows/return` - Return a book
- `GET /api/borrows` - List all records
- `GET /api/borrows/borrower/{id}` - Get borrower's active loans
- `GET /api/borrows/overdue/list` - Get overdue items

### Products
- `GET /api/products` - List products
- `POST /api/products` - Add product
- `GET /api/products/{id}` - Get product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### External APIs
- `GET /api/external/bookinfo/{isbn}` - Get book info from OpenLibrary
- `GET /api/external/logs` - Get API call logs

## Logs

Check application logs:
```bash
tail logs/app-*.txt
```

## Stop Application

Press `Ctrl + C` in terminal

## Troubleshooting

**Port already in use?**
```bash
netstat -ano | findstr :5000
```

**Rebuild solution**:
```bash
dotnet clean
dotnet build
```

**Check dependencies**:
```bash
dotnet restore
```

## Next Steps

- Import Postman collection for full testing
- Review `DEVELOPMENT.md` for architecture details
- Check `README.md` for complete documentation
- Explore code in `Services/` folder for business logic

---

Need help? Check the README.md or DEVELOPMENT.md files!
