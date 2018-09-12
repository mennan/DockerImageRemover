using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace DockerImageRemover
{
    class Program
    {
        private static string repositoryUrl = "";
        private static string imageName = "";
        private static string userName = "";
        private static string password = "";

        static async Task Main(string[] args)
        {
            "Enter your Docker Image Repository Url: ".ToConsole();
            repositoryUrl = Console.ReadLine();

            "Enter image name: ".ToConsole();
            imageName = Console.ReadLine();

            "Username: ".ToConsole();
            userName = Console.ReadLine();

            "Password: ".ToConsole();
            password = Console.ReadLine();

            var tags = await GetTags();

            if (tags != null && tags.Tags?.Count > 0)
            {
                foreach (var tag in tags.Tags)
                {
                    var digest = await GetImageDigest(tag);

                    if (!string.IsNullOrEmpty(digest))
                    {
                        var removeStatus = await RemoveImage(digest);

                        if (removeStatus)
                        {
                            $"Removed {tag} tag successfully on {imageName}.".ToConsole(ConsoleColor.Green);
                        }
                        else
                        {
                            $"Removed process failed of {tag} tag.".ToConsole(ConsoleColor.Red);
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        private static async Task<DockerTag> GetTags()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.docker.distribution.manifest.v2+json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));

                var response = await client.GetAsync($"{repositoryUrl}/v2/{imageName}/tags/list");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tags = JsonConvert.DeserializeObject<DockerTag>(content);

                    return tags;
                }
            }

            return default(DockerTag);
        }

        private static async Task<string> GetImageDigest(string tagName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.docker.distribution.manifest.v2+json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));

                var response = await client.GetAsync($"{repositoryUrl}/v2/{imageName}/manifests/{tagName}");

                if (response.IsSuccessStatusCode)
                {
                    if (response.Headers.TryGetValues("Docker-Content-Digest", out var digestValues))
                    {
                        return digestValues.FirstOrDefault();
                    }
                }
            }

            return null;
        }

        private static async Task<bool> RemoveImage(string digest)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.docker.distribution.manifest.v2+json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));

                var response = await client.DeleteAsync($"{repositoryUrl}/v2/{imageName}/manifests/{digest}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
