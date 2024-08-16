namespace DexCexMevBot.Modules.Orberbooks.CexClients.Proxy;

public class RotatingProxyList
{
    private List<string> clients = new List<string>();
    private int currentIndex = 0;
    
    public RotatingProxyList(List<string> clients)
    {
        this.clients = clients;
        currentIndex = 0;
    }
    
    public string GetNextProxy()
    {
        if (currentIndex == clients.Count)
        {
            currentIndex = 0;
        }

        return clients[currentIndex++];
    }

    public void DeleteInactiveProxy(string proxy) => 
        clients.RemoveAll(el => el == proxy);
}