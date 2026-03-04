using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Services.FavoriteBooks
{
    public class FavoriteBookService : IFavoriteBookService
    {
        private readonly AppDbContext _db;

        public FavoriteBookService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<bool> AddAsync(Guid userId, int bookId)
        {
            var exists = await _db.FavoriteBooks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);

            if (exists) return false;

            _db.FavoriteBooks.Add(new FavoriteBook
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveAsync(Guid userId, int bookId)
        {
            var favorite = await _db.FavoriteBooks
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favorite == null) return false;

            _db.FavoriteBooks.Remove(favorite);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<FavoritePagedResponse<FavoriteBookResponse>> GetMyFavoritesAsync(
            Guid userId,
            int page,
            int pageSize
        )
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.FavoriteBooks
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Author)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FavoriteBookResponse
                {
                    BookId = f.Book.Id,
                    Title = f.Book.Title,
                    ThumbnailUrl = f.Book.ThumbnailUrl,
                    AuthorName = f.Book.Author.FullName,
                    CategoryName = f.Book.Category.Name,
                    YearPublished = f.Book.YearPublished,
                    StockQuantity = f.Book.StockQuantity,
                    Publisher = f.Book.Publisher,
                    IsFavorited = true
                })
                .ToListAsync();

            return new FavoritePagedResponse<FavoriteBookResponse>
            {
                Items = items,
                TotalItems = totalItems,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
        public async Task<bool> IsFavoritedAsync(Guid userId, int bookId)
        {
            return await _db.FavoriteBooks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }
    }
}
