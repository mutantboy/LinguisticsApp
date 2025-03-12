using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.DTOs.UserDTOs
{
    public class AdminDto : UserDto
    {
        public bool CanModifyRules { get; set; }
    }
}
