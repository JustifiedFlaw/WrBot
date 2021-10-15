using System;
using Newtonsoft.Json;

namespace RestEase.Models.Src
{
    public class Times
    {
        [JsonProperty("primary_t")]
        public decimal PrimarySeconds { get; set; }

        public TimeSpan PrimaryTimeSpan 
        { 
            get
            {
                var seconds = (int)Math.Floor(this.PrimarySeconds);
                var milliseconds = (int)((this.PrimarySeconds - seconds) * 1000);

                return new TimeSpan(0, 0, 0, seconds, milliseconds);
            }
        }
    }
}