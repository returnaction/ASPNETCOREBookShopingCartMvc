using BookShopingCartMvcUI.Models;

namespace BookShopingCartMvcUI.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<ShoppingCart> GetUserCart();
        Task<int> RemoveItem(int bookId);
        Task<int> GetCartItemCount(string userId = "");
        Task<bool> DoCheckout();
    }
}