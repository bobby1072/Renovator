using Npm.Renovator.ConsoleApp.Abstract;
using Npm.Renovator.NpmHttpClient.Abstract;
using Npm.Renovator.NpmHttpClient.Models.Request;
using System.Text.Json;

namespace Npm.Renovator.ConsoleApp.Concrete;

internal class ConsoleApp: IConsoleApp
{
    private readonly INpmJsRegistryHttpClient _client;
    public ConsoleApp(INpmJsRegistryHttpClient client)
    {
        _client = client;
    }
    public async Task ExecuteAsync()
    {
        var response = await _client.ExecuteAsync(new NpmJsRegistryRequestBody { Size = 1, Text = "zod" });

        Console.WriteLine(JsonSerializer.Serialize(response));
    }
}