using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text;

namespace ePortfolio
{
    public static class APIClient {
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private static readonly IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static IMemoryCache Cache { get { return _cache; } }

        public static async Task OnGet() {
            if (!_cache.TryGetValue("GitHubRepos", out List<Repository>? _)) {
                await FetchReposGraphQL();
            }
        }

        private static async Task FetchReposGraphQL() {
            string url = "https://api.github.com/graphql";

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
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            // Make the GraphQL request
            var response = await client.PostAsync(url, content); // Bottleneck

            if (response.IsSuccessStatusCode) {
                var responseData = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseData);
                ParseRepositories(jsonDocument);
            }
        }

        private  static void ParseRepositories(JsonDocument jsonDocument) {
            List<Repository> repositories = new();

            HashSet<string> filters = new() {
                "web-development",
                "back-end-development",
                "data-science",
                "cloud"
            };

            var repoNodes = jsonDocument.RootElement.GetProperty("data").GetProperty("viewer").GetProperty("repositories").GetProperty("nodes");

            // Iterate over API response
            foreach (var node in repoNodes.EnumerateArray()) {

                var topicNodes = node.GetProperty("repositoryTopics").GetProperty("nodes");

                // Build repositories
                foreach (var topic in topicNodes.EnumerateArray()) {
                    if (filters.Contains(topic.GetProperty("topic").GetProperty("name").ToString())) {
                        var repo = new Repository {
                            Name = node.GetProperty("name").ToString(),
                            Description = node.GetProperty("description").ToString(),
                            Url = node.GetProperty("url").ToString(),
                        };

                        repo.ProcessName();
                        repositories.Add(repo);
                        break;
                    }
                }
            }
            _cache.Set("GitHubRepos", repositories, TimeSpan.FromMinutes(20));
        }
    }

    public class Repository {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required string Description { get; set; }

        public void ProcessName() {
            Name = Name.Replace('-', ' ');
        }
    }
}
