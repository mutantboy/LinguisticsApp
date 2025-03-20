using AutoMapper;
using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.Application.Common.Models;
using LinguisticsApp.Application.Common.Queries;
using LinguisticsApp.DomainModel.DomainModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Handlers.LanguageContactEventHandlers
{
    public class GetFilteredLanguageContactEventsQueryHandler : IRequestHandler<GetFilteredLanguageContactEventsQuery, PaginatedList<LanguageContactEventDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFilteredLanguageContactEventsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedList<LanguageContactEventDto>> Handle(GetFilteredLanguageContactEventsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<LanguageContactEvent> query = _unitOfWork.LanguageContactEvents.GetQueryable()
                .Include(lce => lce.SourceLanguage)
                .Include(lce => lce.TargetLanguage);

            if (!string.IsNullOrEmpty(request.GrammaticalInfluenceContains))
                query = query.Where(lce => lce.GrammaticalInfluence.Contains(request.GrammaticalInfluenceContains));

            if (request.Type.HasValue)
                query = query.Where(lce => lce.Type == request.Type.Value);

            if (request.SourceLanguageId.HasValue)
                query = query.Where(lce => lce.SourceLanguage.Id.Value == request.SourceLanguageId.Value);

            if (request.TargetLanguageId.HasValue)
                query = query.Where(lce => lce.TargetLanguage.Id.Value == request.TargetLanguageId.Value);

            if (!string.IsNullOrEmpty(request.LoanwordContains))
                query = query.Where(lce => lce.LoanwordsAdopted.Any(l => l.Contains(request.LoanwordContains)));

            var paginatedEvents = await PaginatedList<LanguageContactEvent>.CreateAsync(
                query.OrderByDescending(lce => lce), request.PageNumber, request.PageSize);

            var dtos = _mapper.Map<List<LanguageContactEventDto>>(paginatedEvents.Items);

            return new PaginatedList<LanguageContactEventDto>(
                dtos,
                paginatedEvents.TotalCount,
                paginatedEvents.PageIndex,
                request.PageSize);
        }
    }
}
