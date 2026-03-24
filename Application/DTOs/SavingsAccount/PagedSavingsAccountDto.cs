using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class PagedSavingsAccountDto
    {
        public List<SavingsAccountListDto> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public string? SearchIdentityNumber { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterType { get; set; }
    }
}
