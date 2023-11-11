using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ePortfolio.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string? Name { get; set; }

        [BindProperty]
        [EmailAddress]
        public required string Email { get; set; }

        [BindProperty]
        public required string Message { get; set; }
    }
}
