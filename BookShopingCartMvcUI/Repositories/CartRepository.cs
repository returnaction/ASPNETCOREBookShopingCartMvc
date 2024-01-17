using BookShopingCartMvcUI.Data;
using BookShopingCartMvcUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookShopingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AddItem(int bookId, int qty)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {


                string userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                ShoppingCart cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserID = userId
                    };
                    await _context.ShoppingCarts.AddAsync(cart);

                }

                _context.SaveChanges();

                CartDetail? cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCart_id == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCart_id = cart.Id,
                        Quantity = qty,

                    };

                    _context.CartDetails.Add(cartItem);
                }

                _context.SaveChanges();

                transaction.Commit();
                return true;

            }
            catch (Exception)
            {

                return false;
            }

        }

        public async Task<bool> RemoveItem(int bookId)
        {
            try
            {
                string userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                ShoppingCart? cart = await GetCart(userId);

                if (cart is null)
                {
                    return false;
                }

                _context.SaveChanges();

                CartDetail? cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCart_id == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    return false;
                }
                else if (cartItem.Quantity == 1)
                {
                    _context.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity--;
                }

                _context.SaveChanges();
                return true;

            }
            catch (Exception)
            {

                return false;
            }

        }

        public async Task<IEnumerable<ShoppingCart>> GetUserCart()
        {
            var userId = GetUserId();
            if (userId is null)
                throw new Exception("Invalid userid");

            List<ShoppingCart> shoppingCart = await _context.ShoppingCarts
                                       .Include(a => a.CartDetails)
                                       .ThenInclude(a => a.Book)
                                       .ThenInclude(a => a.Genre)
                                       .Where(a => a.UserID == userId)
                                       .ToListAsync();

            return shoppingCart;

        }

        private async Task<ShoppingCart> GetCart(string userId)
        {
            ShoppingCart? cart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserID == userId);
            return cart;
        }

        private string GetUserId()
        {
            ClaimsPrincipal? principal = _httpContextAccessor.HttpContext.User;
            string? userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
