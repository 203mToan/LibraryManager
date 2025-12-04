using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.Books
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _db;

        public BookService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Book?> GetById(int id)
        {
            return await _db.Books
                .Include(x => x.Author)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BookCreateResponse?> CreateBookAsync(BookCreateRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                Description = request.Description,
                ThumbnailUrl = request.ThumbnailUrl,
                AuthorId = request.AuthorId,
                CategoryId = request.CategoryId,
                Publisher = request.Publisher,
                YearPublished = request.YearPublished,
                StockQuantity = request.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Books.AddAsync(book);
            await _db.SaveChangesAsync();

            return new BookCreateResponse
            {
                Id = book.Id,
                Message = "Book created successfully"
            };
        }

        public async Task<BookUpdateResponse?> UpdateBookAsync(BookUpdateRequest request)
        {
            var book = await _db.Books.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (book == null)
                return null;

            // Update fields only if provided
            if (request.Title != null) book.Title = request.Title;
            if (request.Description != null) book.Description = request.Description;
            if (request.ThumbnailUrl != null) book.ThumbnailUrl = request.ThumbnailUrl;
            if (request.AuthorId.HasValue) book.AuthorId = request.AuthorId.Value;
            if (request.CategoryId.HasValue) book.CategoryId = request.CategoryId.Value;
            if (request.Publisher != null) book.Publisher = request.Publisher;
            if (request.YearPublished.HasValue) book.YearPublished = request.YearPublished;
            if (request.StockQuantity.HasValue) book.StockQuantity = request.StockQuantity.Value;

            book.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new BookUpdateResponse
            {
                Id = book.Id,
                Message = "Book updated successfully"
            };
        }
        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<PagedBookResponse> GetAllBooksAsync(int? pageIndex, int? pageSize)
        {
            var books = await _db.Books
                .Include(x => x.Author)
                .Include(x => x.Category)
                .ToListAsync();
           var bookResponse = books.Select(book => new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                ThumbnailUrl = book.ThumbnailUrl,
                AuthorId = book.AuthorId,
                CategoryId = (int)book.CategoryId,
                Publisher = book.Publisher,
                YearPublished = book.YearPublished,
                StockQuantity = book.StockQuantity
            }).ToList();
            var totalItems = books.Count();
            return new PagedBookResponse(bookResponse, totalItems, pageSize);

        }
    }
}
