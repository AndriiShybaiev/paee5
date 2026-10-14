using System;
using Newtonsoft.Json;

namespace Mis_Listas.Models
{
    public class cTask
    {
        [JsonProperty("text")]
        public string text { get; set; } = string.Empty;

        [JsonIgnore]
        public string name
        {
            get => text;
            set => text = value;
        }

        [JsonProperty("date")]
        public DateTime date { get; set; } = DateTime.Now;

        [JsonProperty("visible")]
        public bool visible { get; set; } = true;

        [JsonProperty("expired")]
        public DateTime? expired { get; set; } = null;

        [JsonProperty("finished")]
        public bool finished { get; set; } = false;

        [JsonIgnore]
        public bool active
        {
            get => visible && !finished;
            set => visible = value;
        }

        [JsonIgnore]
        public bool IsOverdue => !finished && expired.HasValue && expired.Value.Date < DateTime.Today;
    }
}
