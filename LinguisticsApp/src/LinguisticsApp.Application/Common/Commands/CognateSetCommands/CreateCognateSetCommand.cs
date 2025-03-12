using LinguisticsApp.DomainModel.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.CognateSetCommands
{
    public class CreateCognateSetCommand
    {
        [Required]
        public string ProtoForm { get; set; } = string.Empty;

        [Required]
        public SemanticField Field { get; set; }

        [Required]
        public Guid ReconstructedFromId { get; set; }

        public Dictionary<string, string> AttestedExamples { get; set; } = new Dictionary<string, string>();
    }
}
