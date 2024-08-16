using System.Net;
using DexCexMevBot.Modules.Orberbooks.CexClients.Models;

namespace DexCexMevBot.Modules.Orberbooks.CexClients.Abstractions;

public abstract class AbstractCexClient
{
    protected abstract string BuildRequestLink(string symbol);

    protected abstract OrderBook ParseResponse(string data);

    public async Task<OrderBook> GetOrderBookAsync(string symbol, string proxyUrl)
    {
        var proxy = new WebProxy(proxyUrl);
        proxy.Credentials = new NetworkCredential("z5kWAjEgWY1y", "I1RqGEvGoCVm");

        var handler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = !string.IsNullOrWhiteSpace(proxyUrl)
        };

        using var httpClient = new HttpClient(handler);
        var requestUrl = BuildRequestLink(symbol);

        var response = await RequestTWithTimeout(
            httpClient.GetAsync(requestUrl),
            10000,
            $"Connection timeout {requestUrl}"
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unable to get orderbook {requestUrl}");
        }

        var responseData = await response.Content.ReadAsStringAsync();
        return ParseResponse(responseData);
    }

    private async Task<T> RequestTWithTimeout<T>(Task<T> task, int timeoutMilliseconds, string timeoutMessage)
    {
        var timeoutTask = Task.Delay(timeoutMilliseconds);
        if (await Task.WhenAny(task, timeoutTask) == task)
        {
            return await task;
        }

        throw new TimeoutException(timeoutMessage);
    }
}