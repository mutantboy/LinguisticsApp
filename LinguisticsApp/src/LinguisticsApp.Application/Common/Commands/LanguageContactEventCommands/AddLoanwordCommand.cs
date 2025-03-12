using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands
{
    public partial class AddLoanwordCommand : IRequest
    {
        public Guid ContactEventId { get; set; }
        [Required]
        public string Loanword { get; set; } = string.Empty;
    }
}
