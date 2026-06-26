using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Books;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;
    public BooksController(IBookService bookService, IMapper mapper)
    {
        _bookService = bookService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAllBooks([FromQuery] bool? available)
    {
        var booksList = available == true
            ? await _bookService.GetAvailableBooksAsync()
            : await _bookService.GetAllAsync();
        return Ok(_mapper.Map<List<BookDto>>(booksList));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null) return NotFound();
        return Ok(_mapper.Map<BookDto>(book));
    }
    
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
}