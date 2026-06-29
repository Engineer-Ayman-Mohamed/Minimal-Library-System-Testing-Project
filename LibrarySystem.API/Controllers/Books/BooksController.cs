using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Books;

/// <summary>
///     Handles CRUD operations for the Books resource.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BooksController" /> class.
    /// </summary>
    public BooksController(IBookService bookService, IMapper mapper)
    {
        _bookService = bookService;
        _mapper = mapper;
    }

    /// <summary>Returns all books, optionally filtered by availability.</summary>
    /// <param name="available">When true, returns only books with available copies.</param>
    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAllBooks([FromQuery] bool? available)
    {
        var booksList = available == true
            ? await _bookService.GetAvailableBooksAsync()
            : await _bookService.GetAllAsync();
        return Ok(_mapper.Map<List<BookDto>>(booksList));
    }
    
    /// <summary>Returns a single book by its ID.</summary>
    /// <param name="id">The book identifier.</param>
    /// <returns>200 with the book, or 404 if not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return Ok(_mapper.Map<BookDto>(book));
    }
    
    /// <summary>Creates a new book.</summary>
    /// <param name="dto">The book creation payload.</param>
    /// <returns>201 with the created book, or 409 if the ISBN already exists.</returns>
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto dto)
    {
        if (await _bookService.ExistsByISBNAsync(dto.ISBN))
            return Conflict(new { message = "A book with this ISBN already exists." });

        var book = _mapper.Map<Book>(dto);
        var created = await _bookService.CreateAsync(book.Title, book.Author, book.ISBN, book.TotalCopies);

        var result = _mapper.Map<BookDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates an existing book.</summary>
    /// <param name="id">The book identifier.</param>
    /// <param name="dto">The updated book data.</param>
    /// <returns>200 with the updated book, or 404 if not found.</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<BookDto>> Update(int id, [FromBody] UpdateBookDto dto)
    {
        try
        {
            var updated = await _bookService.UpdateAsync(id, dto.Title, dto.Author, dto.ISBN, dto.TotalCopies);
            return Ok(_mapper.Map<BookDto>(updated));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a book by its ID.</summary>
    /// <param name="id">The book identifier.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _bookService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}