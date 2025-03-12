using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands
{
    public class UpdateLanguageContactEventCommand
    {
        public ContactType? Type { get; set; }
        public string? GrammaticalInfluence { get; set; }
    }
}
