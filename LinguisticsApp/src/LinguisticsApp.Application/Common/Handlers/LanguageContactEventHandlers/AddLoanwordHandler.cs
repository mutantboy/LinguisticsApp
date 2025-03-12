using System.Threading;
using System.Threading.Tasks;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.Exceptions;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.RichTypes;
using MediatR;

namespace LinguisticsApp.Application.Common.Handlers.LanguageContactEventHandlers
{
    public class AddLoanwordHandler : IRequestHandler<AddLoanwordCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddLoanwordHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(AddLoanwordCommand request, CancellationToken cancellationToken)
        {
            var contactEvent = await _unitOfWork.LanguageContactEvents.GetByIdAsync(
                new LanguageContactEventId(request.ContactEventId)) ?? throw new NotFoundException("LanguageContactEvent", request.ContactEventId);
            contactEvent.AddLoanword(request.Loanword);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}