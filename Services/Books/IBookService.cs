using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System.Threading.Tasks;

namespace MyApi.Services.Books
{
    public interface IBookService
    {
        // Get book by Id
        Task<Book?> GetById(int id);

        // Create book
        Task<BookCreateResponse?> CreateBookAsync(BookCreateRequest request);

        // Update book
        Task<BookUpdateResponse?> UpdateBookAsync(BookUpdateRequest request);

        // Delete book
        Task<bool> DeleteBookAsync(int id);

        // ⭐ PAGINATION CHUẨN
        Task<PagedBookResponse> GetAllBooksAsync(int page, int pageSize);
        Task<BookResponse?> GetByIdAsync(int id);

    }
}
