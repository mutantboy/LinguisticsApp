using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands
{
    public class AddLoanwordCommand
    {
        [Required]
        public string Loanword { get; set; } = string.Empty;
    }
}
