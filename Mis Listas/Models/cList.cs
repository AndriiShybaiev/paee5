using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mis_Listas.Models
{
    public class cList
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string text
        {
            get => string.IsNullOrEmpty(_text) ? name : _text;
            set
            {
                _text = value;
                if (string.IsNullOrEmpty(name)) name = value;
            }
        }
        private string _text = string.Empty;

        [JsonProperty("visible")]
        public bool visible { get; set; } = true;

        [JsonProperty("color")]
        public string color { get; set; } = "Orange";

        [JsonProperty("date")]
        public DateTime date { get; set; } = DateTime.Now;

        [JsonProperty("tasks")]
        public List<cTask> tasks { get; set; } = new List<cTask>();
    }
}
