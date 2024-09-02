using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text;

namespace ePortfolio
{
    public class APIClient(IMemoryCache cache, Settings settings) {
        private IMemoryCache Cache = cache;
        private readonly Settings _settings = settings;

        public async Task OnGet() {
            if (!Cache.TryGetValue("GitHubRepos", out List<Repository>? _)) {
                await FetchReposGraphQL();
            }
        }

        private async Task FetchReposGraphQL() {
            string url = "https://api.github.com/graphql";

            // Your GitHub Personal Access Token
            string? token = _settings.Token;

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

        private void ParseRepositories(JsonDocument jsonDocument) {
            List<Repository> repositories = [];

            HashSet<string> filters = [
                "web-development",
                "back-end-development",
                "data-science",
                "cloud"
            ];

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
            Cache.Set("GitHubRepos", repositories, TimeSpan.FromMinutes(20));
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
