using Newtonsoft.Json;

namespace DexCexMevBot.Modules.Orberbooks.CexClients.Proxy;

public class ProxyProvider
{
    private const string CLOUD_PROXY_URL = "http://10.50.27.29:8000";
    
    public async Task<List<string>> GetActualProxyListAsync(string exchangeId)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(CLOUD_PROXY_URL);
        var responseData = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<IpList>(responseData);
        return result.Ips;
    }
}

public class IpList
{
    [JsonProperty("ips")]
    public List<string> Ips { get; set; }
}