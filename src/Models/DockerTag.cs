using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerImageRemover
{
    public class DockerTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }
}