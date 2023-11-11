using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ePortfolio.Pages
{
    public class ProjectsModel : PageModel
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public ProjectsModel(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
        }

        public async Task OnGet()
        {
            if (!_cache.TryGetValue("GitHubRepos", out List<Repository>? cachedRepos) ||
                !_cache.TryGetValue("GitHubTopics", out HashSet<string>? cachedTopics) ||
                !_cache.TryGetValue("GitHubLangs", out HashSet<string>? cachedLangs) ||
                !_cache.TryGetValue("GitHubFrames", out HashSet<string>? cachedFrames)) {

                (cachedRepos, cachedTopics, cachedLangs, cachedFrames) = await FetchReposGraphQL();

                if (cachedRepos != null) {
                    _cache.Set("GitHubRepos", cachedRepos, TimeSpan.FromMinutes(20));
                    _cache.Set("GitHubTopics", cachedTopics, TimeSpan.FromMinutes(20));
                    _cache.Set("GitHubLangs", cachedLangs, TimeSpan.FromMinutes(20));
                    _cache.Set("GitHubFrames", cachedFrames, TimeSpan.FromMinutes(20));
                }
            }

            ViewData["CachedRepos"] = cachedRepos;
            ViewData["CachedTopics"] = cachedTopics;
            ViewData["CachedLangs"] = cachedLangs;
            ViewData["CachedFrames"] = cachedFrames;
        }

        private async Task<(List<Repository>, HashSet<string>, HashSet<string>, HashSet<string>)> FetchReposGraphQL()
        {
            List<Repository> repositories = new();
            List<string> topics = new();
            List<string> languages = new();
            List<string> frames = new();

            string apiUrl = "https://api.github.com/graphql";

            // Your GitHub Personal Access Token
            string? token = _configuration["MyConfig:ApiKey"];

            // GraphQL query to get user's repositories
            string query = @"
            {
                viewer {
                    repositories(first: 20) {
                        nodes {
                            name
                            description
                            url
                            languages(first: 10) {
                                nodes {
                                    name
                                }
                            }
                            repositoryTopics(first: 10) {
                                nodes {
                                    topic {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }
            }";

            // Create the GraphQL request
            var requestPayload = new { query };
            var requestJson = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Create and configure the HttpClient
            using (var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

                // Make the GraphQL request
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode) {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"{responseData}");
                    var jsonDocument = JsonDocument.Parse(responseData);

                    (repositories, topics, languages, frames) = ParseRepositories(jsonDocument);

                    topics.Sort();
                    languages.Sort();
                    frames.Sort();
                }
            }
            return (repositories, new HashSet<string>(topics), new HashSet<string>(languages), new HashSet<string>(frames));
        }

        private static (List<Repository>, List<string>, List<string>, List<string>) ParseRepositories(JsonDocument jsonDocument)
        {
            List<Repository> repositories = new();
            List<string> topics = new();
            List<string> languages = new();
            List<string> frames = new();

            // Extract repository information from the JSON response
            var repoNodes = jsonDocument.RootElement
                .GetProperty("data")
                .GetProperty("viewer")
                .GetProperty("repositories")
                .GetProperty("nodes");

            foreach (var node in repoNodes.EnumerateArray()) {
                var repo = new Repository {
                    Name = node.GetProperty("name").ToString(),
                    Description = node.GetProperty("description").ToString(),
                    Url = node.GetProperty("url").ToString(),
                    Languages = node.GetProperty("languages")
                        .GetProperty("nodes")
                        .EnumerateArray()
                        .Select(langNode => langNode.GetProperty("name").ToString())
                        .ToList(),
                    Topics = node.GetProperty("repositoryTopics")
                        .GetProperty("nodes")
                        .EnumerateArray()
                        .Select(topicNode => topicNode.GetProperty("topic").GetProperty("name").ToString())
                        .ToList(),
                };

                repo.ProcessTopics();

                foreach (var topic in repo.Topics) {
                    topics.Add(topic);
                }

                foreach (var language in repo.Languages) {
                    if (language != "Makefile") {
                        languages.Add(language);
                    }
                }

                foreach (var frame in repo.Frameworks) {
                    frames.Add(frame);
                }

                repositories.Add(repo);
            }

            return (repositories, topics, languages, frames);
        }
    }

    public class Repository {
        public required string Name { get; set; } = string.Empty;
        public required string Url { get; set; } = string.Empty;
        public required string Description { get; set; } = string.Empty;
        public required List<string> Languages { get; set; } = new List<string>();
        public required List<string> Topics { get; set; } = new List<string>();
        public List<string> Frameworks { get; set; } = new List<string>();

        public void ProcessTopics () {
            for (int i = Topics.Count - 1; i >= 0; i--) {
                Topics[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Topics[i].Replace('-', ' '));

                // Check if the topic starts with "Framework"
                if (Topics[i].StartsWith("Framework", StringComparison.OrdinalIgnoreCase)) {
                    string framework = Topics[i].Substring("Framework ".Length);

                    Frameworks.Add(framework);
                    Topics.RemoveAt(i);
                }
            }
        }
    }
}
