using AutoMapper;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;

namespace VocabMaster.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<RegisterRequestDto, User>();
            CreateMap<LoginRequestDto, User>();

            // Vocabulary mappings
            CreateMap<Vocabulary, LearnedWord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // LearnedWord mappings
            CreateMap<LearnedWord, LearnedWordDto>();
            CreateMap<AddLearnedWordDto, LearnedWord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Quiz mappings
            CreateMap<QuizQuestion, QuizQuestionDto>();

            // CompletedQuiz mappings
            CreateMap<CompletedQuiz, CompletedQuizDto>()
                .ForMember(dest => dest.Word, opt => opt.MapFrom(src => src.QuizQuestion != null ? src.QuizQuestion.Word : null));
                
            CreateMap<MarkQuizCompletedDto, CompletedQuiz>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.QuizQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());
        }
    }
}