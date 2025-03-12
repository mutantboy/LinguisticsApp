using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.UserCommands
{
    public class UpdateResearcherCommand : UpdateUserCommand
    {
        public string Institution { get; set; } = string.Empty;
        public ResearchField Field { get; set; }
    }
}
