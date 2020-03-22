using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using Microsoft.AspNetCore.Authorization;

namespace BlazorBoilerplate.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet("GetAllBooks")]
        public async Task<ApiResponse> GetAllBooks()
        {
            List<Book> books = await _context.Books.IgnoreQueryFilters().ToListAsync();
            for (int b = 0; b < books.Count; b++)
            {
                Guid BookStoreId = _context.Books.IgnoreQueryFilters().Where(bo => bo.Id == books[b].Id).Select(bo => EF.Property<Guid>(bo, "TenantId")).FirstOrDefault();
                Tenant BookStore = await _context.Tenants.FindAsync(BookStoreId);
                books[b].BookStoreTitle = BookStore.Title;
            }
            return new ApiResponse(200, "Books Retrieved", books);
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ApiResponse> GetBooks() => new ApiResponse(200, "Books Retrieved", await _context.Books.ToListAsync());

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetBook(int id)
        {
            Book book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return new ApiResponse(404, "Book not found");
            }

            return new ApiResponse(200, "Book retrieved", book);
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        [Authorize(Permissions.Book.Update)]
        public async Task<ApiResponse> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return new ApiResponse(400, "Bad request");
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return new ApiResponse(404, "Book not found");
                }
                else
                {
                    throw;
                }
            }

            return new ApiResponse(200, "Book updated", book);
        }

        // POST: api/Books
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        [Authorize(Permissions.Book.Create)]
        public async Task<ApiResponse> PostBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return new ApiResponse(200, "Book added", book);
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        [Authorize(Permissions.Book.Delete)]
        public async Task<ApiResponse> DeleteBook(int id)
        {
            Book book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return new ApiResponse(404, "Book not found");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return new ApiResponse(200, "Book deleted", book);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}