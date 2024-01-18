using BookShopingCartMvcUI.Data;
using BookShopingCartMvcUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShopingCartMvcUI.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserOrderRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User is not logged-in");
            }

            var orders = await _context.Orders
                                    .Include(x => x.OrderStatus)
                                    .Include(x => x.OrderDetail)
                                    .ThenInclude(x => x.Book)
                                    .ThenInclude(x => x.Genre)
                                    .Where(a => a.UserId == userId)
                                    .ToListAsync();

            return orders;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
