using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ePortfolio.Pages {
    public class IndexModel : PageModel {
        private readonly IMemoryCache _cache;
        private readonly Settings _settings;

        public IndexModel(IMemoryCache cache, IOptionsSnapshot<Settings> options) {
            _cache = cache;
            _settings = options.Value;
        }

        public void OnGet() {
            _ = new APIClient(_cache, _settings).OnGet();
        }
    }
}
