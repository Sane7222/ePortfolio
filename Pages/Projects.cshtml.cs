using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ePortfolio.Pages
{
    public class ProjectsModel : PageModel {
        private readonly IMemoryCache _cache;
        private readonly Settings _settings;

        public ProjectsModel(IMemoryCache cache, IOptionsSnapshot<Settings> options) {
            _cache = cache;
            _settings = options.Value;
        }

        public async Task OnGet() {
            await new APIClient(_cache, _settings).OnGet();
            ViewData["CachedRepos"] = _cache.Get("GitHubRepos");
        }
    }
}
