using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Services.Books
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _db;

        public BookService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // =========================
        // 1️⃣ HÀM CŨ – GIỮ NGUYÊN
        // =========================
        public async Task<Book?> GetById(int id)
        {
            return await _db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // =========================
        // 2️⃣ HÀM MỚI – CHO FE
        // =========================
        public async Task<BookResponse?> GetByIdAsync(int id)
        {
            var book = await _db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return null;

            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                ThumbnailUrl = book.ThumbnailUrl,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId ?? 0,
                Publisher = book.Publisher,
                YearPublished = book.YearPublished,
                StockQuantity = book.StockQuantity,
                AuthorName = book.Author.FullName,
                CategoryName = book.Category.Name
            };
        }

        // =========================
        // 3️⃣ CREATE
        // =========================
        public async Task<BookCreateResponse?> CreateBookAsync(BookCreateRequest request)
        {
            if (request == null) return null;

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

        // =========================
        // 4️⃣ UPDATE
        // =========================
        public async Task<BookUpdateResponse?> UpdateBookAsync(BookUpdateRequest request)
        {
            if (request == null) return null;

            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == request.Id);
            if (book == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Title))
                book.Title = request.Title;

            if (request.Description != null)
                book.Description = request.Description;

            if (request.ThumbnailUrl != null)
                book.ThumbnailUrl = request.ThumbnailUrl;

            if (request.AuthorId.HasValue)
                book.AuthorId = request.AuthorId.Value;

            if (request.CategoryId.HasValue)
                book.CategoryId = request.CategoryId.Value;

            if (request.Publisher != null)
                book.Publisher = request.Publisher;

            if (request.YearPublished.HasValue)
                book.YearPublished = request.YearPublished;

            if (request.StockQuantity.HasValue)
                book.StockQuantity = request.StockQuantity.Value;

            book.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new BookUpdateResponse
            {
                Id = book.Id,
                Message = "Book updated successfully"
            };
        }

        // =========================
        // 5️⃣ DELETE
        // =========================
        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            return true;
        }

        // =========================
        // 6️⃣ GET ALL – PAGINATION
        // =========================
        public async Task<PagedBookResponse> GetAllBooksAsync(int page, int pageSize, int? categoryId)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .Where(x => !categoryId.HasValue || x.CategoryId == categoryId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    ThumbnailUrl = b.ThumbnailUrl,
                    AuthorId = b.AuthorId,
                    CategoryId = b.CategoryId ?? 0,
                    Publisher = b.Publisher,
                    YearPublished = b.YearPublished,
                    StockQuantity = b.StockQuantity,
                    AuthorName = b.Author.FullName,
                    CategoryName = b.Category.Name
                })
                .ToListAsync();

            return new PagedBookResponse(items, totalItems, pageSize);
        }
    }
}
