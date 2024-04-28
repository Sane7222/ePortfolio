using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace ePortfolio.Pages
{
    public class ProjectsModel : PageModel {
        private readonly IMemoryCache _cache;

        public ProjectsModel() {
            _cache = APIClient.Cache;
        }

        public async Task OnGet() {
            await APIClient.OnGet();
            ViewData["CachedRepos"] = _cache.Get("GitHubRepos");
        }
    }
}
