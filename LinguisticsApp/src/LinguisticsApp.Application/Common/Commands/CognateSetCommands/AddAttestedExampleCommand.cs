using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Commands.CognateSetCommands
{
    public class AddAttestedExampleCommand
    {
        [Required]
        public string Language { get; set; } = string.Empty;

        [Required]
        public string Form { get; set; } = string.Empty;
    }
}
