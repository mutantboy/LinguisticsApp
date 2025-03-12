using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.CognateSetCommands
{
    public class UpdateCognateSetCommand
    {
        public string? ProtoForm { get; set; }
        public SemanticField? Field { get; set; }
    }
}
