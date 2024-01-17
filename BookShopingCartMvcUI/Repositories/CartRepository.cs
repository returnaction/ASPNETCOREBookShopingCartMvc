using BookShopingCartMvcUI.Data;
using BookShopingCartMvcUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Reflection.Metadata.Ecma335;
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

        public async Task<int> AddItem(int bookId, int qty)
        {
           string userId = GetUserId();
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("user is not logged-in");
                }

                ShoppingCart cart = await GetCart(userId);

                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserID = userId
                    };
                    
                    _context.ShoppingCarts.Add(cart);

                }

                _context.SaveChanges();

                CartDetail? cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _context.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };

                    _context.CartDetails.Add(cartItem);
                }

                _context.SaveChanges();

                transaction.Commit();
               

            }
            catch (Exception)
            {

               
            }

            int cartItemCount = await GetCartItemCount(userId);

            return cartItemCount;

        }

        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();

            try
            {

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("user is not loggod-in");
                }

                ShoppingCart? cart = await GetCart(userId);

                if (cart is null)
                {
                    throw new Exception("Cart is empty");
                }

                _context.SaveChanges();

                CartDetail? cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    throw new Exception("Not items in cart");
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
            }
            catch (Exception)
            {

               
            }

            int cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;


        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId is null)
                throw new Exception("Invalid userid");

            ShoppingCart? shoppingCart = await _context.ShoppingCarts
                                       .Include(a => a.CartDetails)
                                       .ThenInclude(a => a.Book)
                                       .ThenInclude(a => a.Genre)
                                       .Where(a => a.UserID == userId)
                                       .FirstOrDefaultAsync();
                                       

            return shoppingCart;

        }

        private async Task<ShoppingCart> GetCart(string userId)
        {
            ShoppingCart? cart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserID == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId="")
        {
            if (!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _context.ShoppingCarts
                              join cartDetail in _context.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }
                              ).ToListAsync();

            return data.Count;
        }

        private string GetUserId()
        {
            ClaimsPrincipal? principal = _httpContextAccessor.HttpContext.User;
            string? userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
