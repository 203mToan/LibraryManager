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
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Bio = request.Bio,
                Nationality = request.Nationality,
                BirthYear = request.BirthYear,
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

            if (!string.IsNullOrWhiteSpace(request.FullName))
                author.FullName = request.FullName;

            if (request.Bio != null)
                author.Bio = request.Bio;

            if (request.National != null)
                author.Nationality = request.National;

            if (request.BirthYear.HasValue)
                author.BirthYear = request.BirthYear;

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

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            return true;
        }

        // ⭐ PAGINATION CHUẨN – KHÔNG TRẢ ALL DATA
        public async Task<PagedAuthorResponse> GetAllAuthorsAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Authors
                .Include(a => a.Books)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuthorResponse
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Bio = a.Bio,
                    Nationality = a.Nationality,
                    BirthYear = a.BirthYear,
                    BookCount = a.Books.Count
                })
                .ToListAsync();

            return new PagedAuthorResponse(
                items,
                totalItems,
                pageSize
            );
        }
    }
}
