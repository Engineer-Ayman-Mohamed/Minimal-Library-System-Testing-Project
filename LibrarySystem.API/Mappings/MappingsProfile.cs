using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.API.Mappings;

public class MappingsProfile : Profile
{
    public MappingsProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateBookDto, Book>();
        
        CreateMap<Member, MemberDto>()
            .ForMember(dest => dest.ActiveLoanCount, opt => opt.Ignore());
        CreateMap<CreateMemberDto, Member>();
        
        CreateMap<Loan, LoanDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book != null ? src.Book.Title : string.Empty))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null ? src.Member.FullName : string.Empty));
        CreateMap<CreateLoanDto, Loan>();
    }
}