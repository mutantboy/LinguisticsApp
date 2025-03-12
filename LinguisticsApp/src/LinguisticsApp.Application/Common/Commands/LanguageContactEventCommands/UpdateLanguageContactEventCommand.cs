using LinguisticsApp.DomainModel.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands
{
    public partial class UpdateLanguageContactEventCommand : IRequest
    {
        public Guid Id { get; set; } ///Property to hold the ID from the route
        public ContactType? Type { get; set; }
        public string? GrammaticalInfluence { get; set; }
    }
}
