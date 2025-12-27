using System;
using System.Threading.Tasks;

namespace MyApi.Services.FavoriteBooks
{
    public interface IFavoriteBookService
    {
        Task<bool> AddAsync(Guid userId, int bookId);
        Task<bool> RemoveAsync(Guid userId, int bookId);
    }
}
