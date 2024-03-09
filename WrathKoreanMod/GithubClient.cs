using System.Net;
using Newtonsoft.Json;

namespace WrathKoreanMod;

public class GithubClient : IDisposable
{
    private const string OWNER = "akintos";
    private const string REPO = "wotr-translation";

    private readonly WebClient _client;

    private string _apiToken;

    public GithubClient()
    {
        _client = new()
        {
            BaseAddress = "https://api.github.com"
        };

#if DEBUG
        string apiToken = Environment.GetEnvironmentVariable("WrathGithubToken");
        if (apiToken is not null)
        {
            SetApiToken(apiToken);
        }
#endif
    }

    public void SetApiToken(string token)
    {
        _apiToken = token;
    }

    private WebClient PrepareWebClient(string accept)
    {
        _client.Headers.Set("User-Agent", "WrathKoreanMod");
        _client.Headers.Set("Accept", accept);
        if (_apiToken is not null)
        {
            _client.Headers.Set("Authorization", $"Bearer {_apiToken}");
        }
        return _client;
    }
    
    public Release GetLatestRelease()
    {
        string apiUrl = $"/repos/{OWNER}/{REPO}/releases/latest";
        return DeserializeJsonRequest<Release>(apiUrl);
    }

    public T DoanloadJsonAsset<T>(Release.Asset asset)
    {
        return DeserializeJsonRequest<T>(asset.browser_download_url);
    }

    public void DownloadAssetFile(Release.Asset asset, string path)
    {
        string assetUrl = $"/repos/{OWNER}/{REPO}/releases/assets/{asset.id}";
        PrepareWebClient("application/octet-stream").DownloadFile(assetUrl, path);
    }

    private T DeserializeJsonRequest<T>(string url)
    {
        string content = PrepareWebClient("application/json").DownloadString(url);
        return JsonConvert.DeserializeObject<T>(content);
    }

    public void Dispose()
    {
        ((IDisposable)_client).Dispose();
    }

    public class Release
    {
        public int id;
        public string tag_name;
        public string name;

        public DateTime created_at;
        public DateTime published_at;

        public List<Asset> assets;

        public class Asset
        {
            public int id;
            public string name;

            public DateTime created_at;
            public DateTime published_at;

            public string browser_download_url;
        }
    }
}
