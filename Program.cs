// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Security.AccessControl;
using MinabetBotsWeb.scrapper.soccer;

var betsBola = new BetsBola(new HttpClient());

var timer = new Stopwatch();

timer.Start();
Console.WriteLine($"Program Start - {timer.Elapsed}");

Console.Out.WriteLine(String.Join("\n", betsBola.ListEvents().Select(item => item.ToString())));
timer.Stop();
Console.WriteLine($"Program End - {timer.Elapsed}");
