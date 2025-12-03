using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.Books
{
    public interface IBookService
    {
        Task<Entities.Book?> GetById(int id);
        Task<BookCreateResponse?> CreateBookAsync(BookCreateRequest request);
        Task<BookUpdateResponse?> UpdateBookAsync(BookUpdateRequest request);
        Task<bool> DeleteBookAsync(int id);
    }
}
