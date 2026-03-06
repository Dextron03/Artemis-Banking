using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public string FirtName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        

    }
}