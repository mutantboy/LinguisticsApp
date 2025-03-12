using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.UserDTOs
{
    public class ResearcherDto : UserDto
    {
        public string Institution { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
    }
}
