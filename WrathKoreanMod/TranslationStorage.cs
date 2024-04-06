using Newtonsoft.Json;

namespace WrathKoreanMod;
internal class TranslationStorage
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("translated")]
    public int Translated { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("data")]
    private Dictionary<string, string> Data { get; set;}

    internal bool TryGetValue(string key, out string translated)
    {
        return Data.TryGetValue(key, out translated);
    }
}
