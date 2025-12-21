using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db)
        {
            _db = db;
        }

        // GET ALL (ADMIN) - PHÂN TRANG
        public async Task<CommentPagedResponse<CommentResponse>> GetAllAsync(int page, int pageSize)
        {
            var query = _db.Comments
                .Include(c => c.User)
                .Include(c => c.Book)
                .OrderByDescending(c => c.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    BookId = c.BookId,
                    BookTitle = c.Book.Title,
                    Rating = c.Rating,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new CommentPagedResponse<CommentResponse>
            {
                Items = items,
                TotalItems = totalItems,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        // GET COMMENT BY BOOK
        public async Task<IEnumerable<CommentResponse>> GetByBookIdAsync(int bookId)
        {
            return await _db.Comments
                .Where(c => c.BookId == bookId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    BookId = c.BookId,
                    Rating = c.Rating,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        // CREATE
        public async Task<CommentResponse> CreateAsync(Guid userId, CommentCreateRequest request)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookId = request.BookId,
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return new CommentResponse
            {
                Id = comment.Id,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Rating = comment.Rating,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        // UPDATE (CHỈ CHỦ COMMENT)
        public async Task<bool> UpdateAsync(Guid id, Guid userId, CommentUpdateRequest request)
        {
            var comment = await _db.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null) return false;

            comment.Rating = request.Rating;
            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        // DELETE (CHỈ CHỦ COMMENT)
        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var comment = await _db.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null) return false;

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
