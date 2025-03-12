using AutoMapper;
using LinguisticsApp.Application.Common.Commands.CognateSetCommands;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.Commands.PhonemeInventoryCommands;
using LinguisticsApp.Application.Common.Commands.SemanticShiftCommands;
using LinguisticsApp.Application.Common.Commands.SoundShiftRuleCommands;
using LinguisticsApp.Application.Common.DTOs.CognateSetDTOs;
using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.DTOs.PhonemeInventoryDTOs;
using LinguisticsApp.Application.Common.DTOs.SemanticShiftDTOs;
using LinguisticsApp.Application.Common.DTOs.SoundShiftRuleDTOs;
using LinguisticsApp.Application.Common.DTOs.UserDTOs;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.DomainModel.ValueObjects;


namespace LinguisticsApp.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            /// User 
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Username.Value))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.GetType().Name))
                .Include<Admin, AdminDto>()
                .Include<Researcher, ResearcherDto>();

            CreateMap<Admin, AdminDto>()
                .ForMember(dest => dest.CanModifyRules, opt => opt.MapFrom(src => src.CanModifyRules)); ;

            CreateMap<Researcher, ResearcherDto>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field.ToString()))
                .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution));

            /// PhonemeInventory 
            CreateMap<PhonemeInventory, PhonemeInventoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.StressPattern, opt => opt.MapFrom(src => src.StressPattern.ToString()))
                .ForMember(dest => dest.Consonants, opt => opt.MapFrom(src => src.Consonants))
                .ForMember(dest => dest.Vowels, opt => opt.MapFrom(src => src.Vowels));

            CreateMap<PhonemeInventory, PhonemeInventorySummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.StressPattern, opt => opt.MapFrom(src => src.StressPattern.ToString()))
                .ForMember(dest => dest.ConsonantCount, opt => opt.MapFrom(src => src.Consonants.Count))
                .ForMember(dest => dest.VowelCount, opt => opt.MapFrom(src => src.Vowels.Count));

            CreateMap<Phoneme, PhonemeDto>();

            CreateMap<PhonemeDto, Phoneme>()
                .ConstructUsing(dto => new Phoneme(dto.Value, dto.IsVowel));

            /// SoundShiftRule 
            CreateMap<SoundShiftRule, SoundShiftRuleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            /// CognateSet 
            CreateMap<CognateSet, CognateSetDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.ProtoForm, opt => opt.MapFrom(src => src.ProtoForm.Value))
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field.ToString()));

            /// SemanticShift 
            CreateMap<SemanticShift, SemanticShiftDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.Trigger.ToString()));

            /// LanguageContactEvent 
            CreateMap<LanguageContactEvent, LanguageContactEventDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.LoanwordsAdopted, opt => opt.MapFrom(src => src.LoanwordsAdopted.ToList()));

            /// Command to Entity 
            CreateMap<CreatePhonemeInventoryCommand, PhonemeInventory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new LanguageName(src.Name)))
                .ForMember(dest => dest.StressPattern, opt => opt.MapFrom(src => src.StressPattern))
                .ForMember(dest => dest.Consonants, opt => opt.Ignore())
                .ForMember(dest => dest.Vowels, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.SoundShiftRules, opt => opt.Ignore())
                .ForMember(dest => dest.CognateSets, opt => opt.Ignore());

            CreateMap<CreateSoundShiftRuleCommand, SoundShiftRule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InputPhoneme, opt => opt.MapFrom(src =>
                    new Phoneme(src.InputPhonemeValue, src.InputPhonemeIsVowel)))
                .ForMember(dest => dest.OutputPhoneme, opt => opt.MapFrom(src =>
                    new Phoneme(src.OutputPhonemeValue, src.OutputPhonemeIsVowel)))
                .ForMember(dest => dest.AppliesTo, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            CreateMap<CreateCognateSetCommand, CognateSet>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProtoForm, opt => opt.MapFrom(src => new ProtoForm(src.ProtoForm)))
                .ForMember(dest => dest.ReconstructedFrom, opt => opt.Ignore())
                .ForMember(dest => dest.SemanticShifts, opt => opt.Ignore());

            CreateMap<CreateSemanticShiftCommand, SemanticShift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AffectedCognate, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedEvents, opt => opt.Ignore());

            CreateMap<CreateLanguageContactEventCommand, LanguageContactEvent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SourceLanguage, opt => opt.Ignore())
                .ForMember(dest => dest.TargetLanguage, opt => opt.Ignore())
                .ForMember(dest => dest.LoanwordsAdopted, opt => opt.Ignore())
                .ForMember(dest => dest.CausedShifts, opt => opt.Ignore());
        }
    }
}