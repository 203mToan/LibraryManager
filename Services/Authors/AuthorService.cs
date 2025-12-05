using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Services.Authors
{
    public class AuthorService : IAuthorService
    {
        private readonly AppDbContext _db;

        public AuthorService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Author?> GetByIdAsync(Guid id)
        {
            return await _db.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AuthorCreateResponse?> CreateAuthorAsync(AuthorCreateRequest request)
        {
            if (request == null) return null;

            var author = new Author
            {
                Id = Guid.NewGuid(), // safe for Guid PKs; remove if DB generates GUID
                FullName = request.FullName,
                Bio = request.Bio,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Authors.AddAsync(author);
            await _db.SaveChangesAsync();

            return new AuthorCreateResponse
            {
                Id = author.Id,
                Message = "Author created successfully"
            };
        }

        public async Task<AuthorUpdateResponse?> UpdateAuthorAsync(AuthorUpdateRequest request)
        {
            if (request == null) return null;

            var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == request.Id);
            if (author == null) return null;

            if (request.FullName != null) author.FullName = request.FullName;
            if (request.Bio != null) author.Bio = request.Bio;

            author.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new AuthorUpdateResponse
            {
                Id = author.Id,
                Message = "Author updated successfully"
            };
        }

        public async Task<bool> DeleteAuthorAsync(Guid id)
        {
            var author = await _db.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) return false;

            // Optional business rule: prevent delete if has books
            // if (author.Books != null && author.Books.Any()) return false;

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedAuthorResponse> GetAllAuthorsAsync(int? pageIndex, int? pageSize)
        {
            var query = _db.Authors.AsQueryable();

            var totalItems = await query.CountAsync();

            if (!pageIndex.HasValue || !pageSize.HasValue || pageSize.Value <= 0)
            {
                var all = await query.Include(a => a.Books).ToListAsync();
                var itemsAll = all.Select(a => new AuthorResponse
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Bio = a.Bio,
                    BookCount = a.Books?.Count
                }).ToList();

                return new PagedAuthorResponse(itemsAll, totalItems, pageSize);
            }

            var skip = (pageIndex.Value - 1) * pageSize.Value;
            var paged = await query
                .Include(a => a.Books)
                .Skip(skip)
                .Take(pageSize.Value)
                .ToListAsync();

            var items = paged.Select(a => new AuthorResponse
            {
                Id = a.Id,
                FullName = a.FullName,
                Bio = a.Bio,
                BookCount = a.Books?.Count
            }).ToList();

            return new PagedAuthorResponse(items, totalItems, pageSize);
        }
    }
}
