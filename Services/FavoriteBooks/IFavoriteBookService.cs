using MyApi.Model.Response;
using System;
using System.Threading.Tasks;

namespace MyApi.Services.FavoriteBooks
{
    public interface IFavoriteBookService
    {
        Task<bool> AddAsync(Guid userId, int bookId);
        Task<bool> RemoveAsync(Guid userId, int bookId);
        Task<bool> IsFavoritedAsync(Guid userId, int bookId);

        // ⭐ CHỈ ĐỔI TÊN RESPONSE
        Task<FavoritePagedResponse<FavoriteBookResponse>> GetMyFavoritesAsync(
            Guid userId,
            int page,
            int pageSize
        );
    }
}
