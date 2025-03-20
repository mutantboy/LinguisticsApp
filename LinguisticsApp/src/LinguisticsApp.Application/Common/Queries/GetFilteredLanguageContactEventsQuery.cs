using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.Models;
using LinguisticsApp.DomainModel.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Queries
{
    public class GetFilteredLanguageContactEventsQuery : PaginationQuery, IRequest<PaginatedList<LanguageContactEventDto>>
    {
        public string? GrammaticalInfluenceContains { get; set; }
        public ContactType? Type { get; set; }
        public Guid? SourceLanguageId { get; set; }
        public Guid? TargetLanguageId { get; set; }
        public string? LoanwordContains { get; set; }
    }
}
