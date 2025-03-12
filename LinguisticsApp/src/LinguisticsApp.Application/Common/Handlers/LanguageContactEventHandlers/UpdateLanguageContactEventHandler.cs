using System.Threading;
using System.Threading.Tasks;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.Exceptions;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.RichTypes;
using MediatR;

namespace LinguisticsApp.Application.Common.Handlers.LanguageContactEventHandlers
{
    public class UpdateLanguageContactEventHandler : IRequestHandler<UpdateLanguageContactEventCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateLanguageContactEventHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateLanguageContactEventCommand request, CancellationToken cancellationToken)
        {
            var contactEvent = await _unitOfWork.LanguageContactEvents.GetByIdAsync(
                new LanguageContactEventId(request.Id)) ?? throw new NotFoundException("LanguageContactEvent", request.Id);

            if (request.Type.HasValue)
                contactEvent.UpdateType(request.Type.Value);

            if (!string.IsNullOrEmpty(request.GrammaticalInfluence))
                contactEvent.UpdateGrammaticalInfluence(request.GrammaticalInfluence);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}