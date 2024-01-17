using BookShopingCartMvcUI.Data;
using BookShopingCartMvcUI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShopingCartMvcUI.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _context;

        public HomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> Genre()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
        {
            sTerm = sTerm.ToLower();

            IEnumerable<Book> books = await (from book in _context.Books
                                             join genre in _context.Genres
                                             on book.GenreId equals genre.Id
                                             where string.IsNullOrWhiteSpace(sTerm) || (book != null && book.BookName.ToLower().StartsWith(sTerm))
                                             select new Book
                                             {
                                                 Id = book.Id,
                                                 Image = book.Image,
                                                 AuthorName = book.AuthorName,
                                                 BookName = book.BookName,
                                                 GenreId = book.GenreId,
                                                 Price = book.Price,
                                                 GenreName = genre.GenreName
                                             }).ToListAsync();

            if (genreId > 0)
            {
                books = books.Where(a => a.GenreId == genreId).ToList();
            }

            return books;
        }


    }
}
