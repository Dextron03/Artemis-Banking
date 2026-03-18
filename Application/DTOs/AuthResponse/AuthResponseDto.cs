using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AuthResponse
{
    public class AuthResponseDto
    {
        public bool HasError { get; set; } = false;   
        public string Error { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IList<string> Roles { get; set; }
    }
}
