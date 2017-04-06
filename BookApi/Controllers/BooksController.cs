using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BookApi.Models;
using BooksAPI.Models;
using System.Linq.Expressions;
using BookApi.DTOs;

namespace BookApi.Controllers
{
    [RoutePrefix("api/books")]
    public class BooksController : ApiController
    {
        private BookApiContext db = new BookApiContext();

        private static readonly Expression<Func<Book, BookDTO>> AsBookDTO =
            x => new BookDTO
            {
                Author = x.Author.Name,
                Genre = x.Genre,
                Title = x.Title
            };

        [Route("")]
        // GET: api/Books
        public IQueryable<BookDTO> GetBooks()
        {
            return db.Books.Include(b => b.Author).Select(AsBookDTO);
        }
        [Route("{id:int}")]
        // GET: api/Books/5
        [ResponseType(typeof(BookDTO))]
        public async Task<IHttpActionResult> GetBook(int id)
        {
            BookDTO book = await db.Books.Include(b => b.Author).
                Where( b => b.BookId == id).Select(AsBookDTO).FirstOrDefaultAsync();
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [Route("{id:int}/details")]
        // GET: api/Books/5
        [ResponseType(typeof(BookDetailDto))]
        public async Task<IHttpActionResult> GetBookDetails(int id)
        {
            BookDetailDto book = await (from bk in db.Books.Include(b => b.Author)
                                  where bk.BookId == id
                                  select new BookDetailDto
                                  {
                                      Author = bk.Author.Name,
                                      Description = bk.Description,
                                      Genre = bk.Genre,
                                      Price = bk.Price,
                                      PublishDate = bk.PublishDate,
                                      Title = bk.Title
                                  }).FirstOrDefaultAsync();

               
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [Route("{genre}")]
        public IQueryable<BookDTO> GetBooksByGenre(string genre)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .Select(AsBookDTO);
        }

        [Route("~api/authors/{authorId}/books")]
        public IQueryable<BookDTO> GetBooksByAuthor(int authorId)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .Select(AsBookDTO);
        }

        [Route("date/{pubdate:datetime:regex(\\d{4}-\\d{2}-\\d{2})}")]
        [Route("date/{*pubdate:datetime:regex(\\d{4}/\\d{2}/\\d{2})}")]
        public IQueryable<BookDTO> GetBooks(DateTime pubdate)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => DbFunctions.TruncateTime(b.PublishDate)
                    == DbFunctions.TruncateTime(pubdate))
                .Select(AsBookDTO);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        // PUT: api/Books/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != book.BookId)
            {
                return BadRequest();
            }

            db.Entry(book).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Books
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> PostBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Books.Add(book);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = book.BookId }, book);
        }

        // DELETE: api/Books/5
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> DeleteBook(int id)
        {
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();

            return Ok(book);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookExists(int id)
        {
            return db.Books.Count(e => e.BookId == id) > 0;
        }
    }
}