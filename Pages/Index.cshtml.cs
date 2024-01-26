using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePortfolio.Pages {
    public class IndexModel : PageModel {
        public void OnGet() {
            _ = APIClient.OnGet();
        }
    }
}
