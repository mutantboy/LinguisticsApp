using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.Exceptions;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using MediatR;

namespace LinguisticsApp.Application.Common.Handlers.LanguageContactEventHandlers
{
    public class CreateLanguageContactEventHandler : IRequestHandler<CreateLanguageContactEventCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateLanguageContactEventHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateLanguageContactEventCommand request, CancellationToken cancellationToken)
        {
            var sourceLanguage = await _unitOfWork.PhonemeInventories.GetByIdAsync(new PhonemeInventoryId(request.SourceLanguageId)) ?? throw new NotFoundException("Source language", request.SourceLanguageId);
            var targetLanguage = await _unitOfWork.PhonemeInventories.GetByIdAsync(new PhonemeInventoryId(request.TargetLanguageId)) ?? throw new NotFoundException("Target language", request.TargetLanguageId);

            var contactEvent = new LanguageContactEvent(
                LanguageContactEventId.New(),
                request.Type,
                request.GrammaticalInfluence,
                sourceLanguage,
                targetLanguage);

            foreach (var loanword in request.LoanwordsAdopted)
            {
                contactEvent.AddLoanword(loanword);
            }

            await _unitOfWork.LanguageContactEvents.AddAsync(contactEvent);
            await _unitOfWork.SaveChangesAsync();

            return contactEvent.Id.Value;
        }
    }
}