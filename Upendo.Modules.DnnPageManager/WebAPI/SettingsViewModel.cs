using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Upendo.Modules.DnnPageManager.Controller
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsViewModel
    {
        public SettingsViewModel()
        {
        }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("keywords")]
        public string Keywords { get; set; }
    }
}
