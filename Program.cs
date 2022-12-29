using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using MinabetBotsWeb.scrapper;
using MinabetBotsWeb.scrapper.soccer;
using Newtonsoft.Json;

namespace MinabetBotsWeb;

public class Program
{
    private static HttpClient HttpClient = new();
    
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: HtmlAgilityPack.HtmlNode")]
    public static void Main(string[] args) {
        // See https://aka.ms/new-console-template for more information

        var teamDb = new TeamDb(minRatio: 0.18, changeFire: 3);
        var text = "";

        if (File.Exists("minabetbot_config.json")) {
            text = File.ReadAllText("minabetbot_config.json");
        }

        var programConfig = JsonConvert.DeserializeObject<ProgramConfig>(text) ?? new ProgramConfig();
        var combinator = new Combinator(programConfig.EventType, teamDb);
        Console.Out.WriteLine("Carregado configuração:");
        Console.Out.WriteLine(programConfig.ToString());

        combinator.OnNewSurebet += async (_, combination) => {
            var jsonData = new StringContent(JsonConvert.SerializeObject(combination),
                Encoding.UTF8,
                "application/json");
            jsonData.Headers.Add("bot-id", "aYvagG1zygWWKz0duUSj");

            // var result = 
            await HttpClient.PostAsync($"{programConfig.DjangoUrl}/bot/events",
                jsonData
            );
            // Console.Out.WriteLine($"Team Home: {combination.EventJson.TeamHomeName}");
            // Console.Out.WriteLine($"Team Away: {combination.EventJson.TeamAwayName}");
            // Console.Out.WriteLine($"Surebet: {combination.Surebet}");
            //
            // combination.Combinations.ForEach(item => {
            //     Console.Out.WriteLine($"\tLabel: {item.Label}");
            //     Console.Out.WriteLine($"\tOdd: {item.Odds}");
            // });
        };

        var betApis = new List<BetApi> {
            new BetsBola(),
            new Betano(),
            new Pansudo(),
        };

        while (true) {
            Console.Out.WriteLine("Processando eventos...");

            betApis.ForEach(betApi => {
                Console.Out.WriteLine($"Processando {betApi.WebSiteName}");

                try {
                    var elements = betApi.ListEvents();

                    Console.Out.WriteLine($"Pegue: {elements.Count} itens");
                    teamDb.PutAll(elements);
                } catch (Exception err) {
                    Console.Out.WriteLine("Falha ao pegar os dados");
                    Console.Out.WriteLine($"Erro: {err.Message}");
                    Console.Out.WriteLine($"StackTrace:\n{err.StackTrace}");
                }

                Console.Out.WriteLine($"Processado {betApi.WebSiteName}");
                Thread.Sleep(4000);
            });

        }
    }
}

public class ProgramConfig {
    public string DjangoUrl { get; set; }
    public string EventType { get; set; }

    public ProgramConfig() {
        DjangoUrl = "http://localhost:8000";
        EventType = "soccer";
    }

    public override string ToString() {
        return $"{nameof(DjangoUrl)}: {DjangoUrl}, {nameof(EventType)}: {EventType}";
    }
}