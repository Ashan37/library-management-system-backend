using library_management_system_backend.Data;
using library_management_system_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public BookController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet("getAllBooks")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            return Ok(await _appDbContext.Books.ToListAsync());
        }

        [HttpGet("getBook/{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _appDbContext.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            return Ok(book);
        }

        [HttpPost("addBook")]
        public async Task<ActionResult<Book>> AddBook(Book objBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _appDbContext.Books.Add(objBook);
            await _appDbContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetBook), new { id = objBook.id }, objBook);
        }

        [HttpPut("updateBook/{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book objBook)
        {
            if (id != objBook.id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _appDbContext.Entry(objBook).State = EntityState.Modified;
            
            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _appDbContext.Books.AnyAsync(b => b.id == id);
                if (!exists)
                {
                    return NotFound($"Book with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("deleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _appDbContext.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            _appDbContext.Books.Remove(book);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
