using System;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config; 
    private readonly string _baseurl;
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    { 
        _httpClient = httpClient;
        _config = config;
        _baseurl = _config["AuctionServiceUrl"];
        
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>($"{_baseurl}/api/auctions?date={lastUpdated}");
    }

}
