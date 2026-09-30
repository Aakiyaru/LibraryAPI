using Library.Application.Dtos;
using Library.Applictation.Dtos;
using Library.Applictation.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Create([FromBody] CreateBookRequest request)
        {
            var created = await _bookService.CreateBookAsync(request);

            return CreatedAtAction(nameof(GetById), new {id =  created.Id}, created);
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] BookQueryParameters parameters)
        {
            var result = await _bookService.GetBooksAsync(parameters);
            return Ok(result);
        }
    }
}
