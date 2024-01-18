using BookShopingCartMvcUI.Models;

namespace BookShopingCartMvcUI.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrders();
    }
}