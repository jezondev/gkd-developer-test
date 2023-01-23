// See https://aka.ms/new-console-template for more information


using System.IO;
using System.Text.Json;
using Bootstrapper;
using Core;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (args.Length < 2)
{
    Console.WriteLine("\nYou must specify engine configuration and empire plan");
    return;
}
var engineConfigurationJsonPath = args[0];
var empirePlanJsonPath = args[1];

#region -> LOADING EMPIRE PLAN /////////////////////////////////////////////////////////////////////
var empirePlanFile = new FileInfo(empirePlanJsonPath);

if (!empirePlanFile.Exists) {
    Console.WriteLine("/!\\ Empire plan file not found!");
    return;
}

EmpirePlan? empirePlan = null;
try
{
    var empirePlanJsonStr = File.ReadAllText(empirePlanJsonPath);
    Console.WriteLine("-> Loaded empire plan:");
    Console.WriteLine(empirePlanJsonStr);

    empirePlan = JsonSerializer.Deserialize<EmpirePlan>(empirePlanJsonStr);
}
catch (Exception e)
{
    Console.WriteLine("/!\\ Error while loading empire plan:");
    Console.WriteLine(e);
    return;
}
#endregion

#region -> STEP 3 -> BUILDING APP CONTEXT /////////////////////////////////////////////////////////////////////
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddApplicationServices(engineConfigurationJsonPath))
    .Build();
#endregion


var computer = host.Services.GetRequiredService<ITravelService>();
var result = await computer.EvaluateTravelOddsAsync(empirePlan);

Console.WriteLine($"{result.BestOdds}");