using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System.Threading.Tasks;

namespace MyApi.Services.Books
{
    public interface IBookService
    {
        Task<Book?> GetById(int id);
        Task<BookCreateResponse?> CreateBookAsync(BookCreateRequest request);
        Task<BookUpdateResponse?> UpdateBookAsync(BookUpdateRequest request);
        Task<bool> DeleteBookAsync(int id);
        Task<PagedBookResponse> GetAllBooksAsync(int page, int pageSize, int? CatergoryId);
        Task<BookResponse?> GetByIdAsync(int id);

    }
}
