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
            if (!_cache.TryGetValue("GitHubRepos", out List<Repository>? cachedRepos)) {
                cachedRepos = await FetchReposGraphQL();

                if (cachedRepos != null) {
                    _cache.Set("GitHubRepos", cachedRepos, TimeSpan.FromMinutes(20));
                }
            }

            ViewData["CachedRepos"] = cachedRepos;
        }

        private async Task<List<Repository>> FetchReposGraphQL()
        {
            List<Repository> repositories = new();

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
                            languages(first: 5) {
                                nodes {
                                    name
                                }
                            }
                            repositoryTopics(first: 15) {
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
                    var jsonDocument = JsonDocument.Parse(responseData);

                    repositories = ParseRepositories(jsonDocument);
                }
            }
            return repositories;
        }

        private static List<Repository> ParseRepositories(JsonDocument jsonDocument)
        {
            List<Repository> repositories = new();

            HashSet<string> filters = new() {
                "web-development",
                "back-end-development",
                "data-science",
                "cloud"
            };

            var repoNodes = jsonDocument.RootElement.GetProperty("data").GetProperty("viewer").GetProperty("repositories").GetProperty("nodes");

            foreach (var node in repoNodes.EnumerateArray()) {

                var topicNodes = node.GetProperty("repositoryTopics").GetProperty("nodes");

                foreach (var topic in topicNodes.EnumerateArray()) {
                    if (filters.Contains(topic.GetProperty("topic").GetProperty("name").ToString())) {
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

                        repo.ProcessName();
                        repo.ProcessTopics();
                        repositories.Add(repo);
                        break;
                    }
                }
            }
            return repositories;
        }
    }

    public class Repository {
        public required string Name { get; set; } = string.Empty;
        public required string Url { get; set; } = string.Empty;
        public required string Description { get; set; } = string.Empty;
        public required List<string> Languages { get; set; } = new List<string>();
        public required List<string> Topics { get; set; } = new List<string>();

        public void ProcessName () {
            Name = Name.Replace('-', ' ');
        }

        public void ProcessTopics () {
            for (int i = 0; i < Topics.Count; i++) {
                Topics[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Topics[i].Replace('-', ' '));
            }
        }
    }
}
