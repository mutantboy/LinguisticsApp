using LinguisticsApp.DomainModel.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands
{
    public class CreateLanguageContactEventCommand : IRequest<Guid>
    {
        [Required]
        public ContactType Type { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string GrammaticalInfluence { get; set; } = string.Empty;

        [Required]
        public Guid SourceLanguageId { get; set; }

        [Required]
        public Guid TargetLanguageId { get; set; }

        public List<string> LoanwordsAdopted { get; set; } = new List<string>();
    }
}
