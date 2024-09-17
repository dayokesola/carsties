using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;
using ZstdSharp.Unsafe;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
//seed db
try
{
    await DbInitializer.InitDb(app);    
}
catch(Exception e)
{
    Console.WriteLine(e.Message);
}
app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));