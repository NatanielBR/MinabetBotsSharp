// See https://aka.ms/new-console-template for more information

using MinabetBotsWeb.scrapper.Betfair.Soccer;
using MinabetBotsWeb.scrapper.soccer;
using System.Diagnostics;

var betsBola = new BetsBola(new HttpClient());
var betano = new Betano(new HttpClient());
var pansudo = new Pansudo(new HttpClient());
var betFair = new ScraperBetfairSoccer(new HttpClient());
var timer = new Stopwatch();

timer.Start();
Console.WriteLine($"Program Start - {timer.Elapsed}");
//Console.Out.WriteLine(String.Join("\n", betsBola.ListEvents().Select(item => item.ToString())));
//Console.Out.WriteLine(String.Join("\n", betano.ListEvents().Select(item => item.ToString())));
//Console.Out.WriteLine(String.Join("\n", pansudo.ListEvents().Select(item => item.ToString())));
//Console.Out.WriteLine(String.Join("\n", betFair.ListEvents().Select(item => item.ToString())));
Console.Out.WriteLine(String.Join("\n", betFair.ListEvents().Select(item => item.ToString())));
timer.Stop();
Console.WriteLine($"Program End - {timer.Elapsed}");
