


using Microsoft.Extensions.Configuration;

Console.WriteLine("Update Starting Soon...");
if(Test())Environment.Exit(0);

bool Test()
{
    if (!File.Exists("appsettings.json")) throw new Exception("appsettings not found");
    var a = AppSet("UpdateFrom");
    Console.WriteLine(a);
    return true;
}

static string AppSet(string key)
{
    IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
    return configuration[key];
}