using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using System;
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

        // ➕ ADD TO FAVORITE
        public async Task<bool> AddAsync(Guid userId, int bookId)
        {
            var exists = await _db.FavoriteBooks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);

            if (exists) return false;

            var favorite = new FavoriteBook
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow
            };

            _db.FavoriteBooks.Add(favorite);
            await _db.SaveChangesAsync();
            return true;
        }

        // ❌ REMOVE FROM FAVORITE
        public async Task<bool> RemoveAsync(Guid userId, int bookId)
        {
            var favorite = await _db.FavoriteBooks
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favorite == null) return false;

            _db.FavoriteBooks.Remove(favorite);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
