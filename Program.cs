using System.Diagnostics.CodeAnalysis;
using System.Text;
using MinabetBotsWeb.scrapper;
using MinabetBotsWeb.scrapper.soccer;
using Newtonsoft.Json;

namespace MinabetBotsWeb;

public class Program
{
    private static HttpClient HttpClient = new();
    public static string eventEndpoint;
    public static ProgramConfig ProgramConfig;

    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: HtmlAgilityPack.HtmlNode")]
    public static void Main(string[] args) {
        // See https://aka.ms/new-console-template for more information

        var teamDb = new TeamDb(minRatio:0.18, changeFire:3);
        var configPath = args[0];
        Console.Out.WriteLine($"Checando se existe a config: '{configPath}'");

        if (File.Exists(configPath)) {
            Console.Out.WriteLine("Arquivo de configuração existe. Carregando...");
            var text = File.ReadAllText(configPath);
            ProgramConfig = JsonConvert.DeserializeObject<ProgramConfig>(text)!;
        } else {
            Console.Out.WriteLine("Arquivo de configuração não existe. Usando configurações padrões...");
            ProgramConfig = new();
        }

        if (CheckURL(ProgramConfig.DjangoUrl)) {
            Console.Out.WriteLine("Site Django confirmado");
        } else {
            Console.Out.WriteLine("Falha ao checar o site.");
            Environment.Exit(-1);
            return;
        }
        eventEndpoint = ProgramConfig.SendEventsToList ? "eventsList" : "events";

        var combinator = new Combinator(ProgramConfig.EventType, teamDb);
        Console.Out.WriteLine("Carregado configuração:");
        Console.Out.WriteLine(ProgramConfig.ToString());
        File.WriteAllText(configPath, JsonConvert.SerializeObject(ProgramConfig));

        combinator.OnNewSurebet += async (_, combination) => {

            if (ProgramConfig.SendEventsToList) {
                try {
                    var jsonData = new StringContent(JsonConvert.SerializeObject(combination),
                        Encoding.UTF8,
                        "application/json");
                    jsonData.Headers.Add("bot-id", "aYvagG1zygWWKz0duUSj");

                    await HttpClient.PostAsync($"{ProgramConfig.DjangoUrl}/bot/{eventEndpoint}",
                        jsonData
                    );
                }
                catch (Exception e) {
                    Console.WriteLine("Falha ao enviar as combinações");
                }
            } else {
                var failCount = 0;
                var total = combination.Count;
                combination.ForEach(async (item) => {
                    try {
                        var jsonData = new StringContent(JsonConvert.SerializeObject(item),
                            Encoding.UTF8,
                            "application/json");
                        jsonData.Headers.Add("bot-id", "aYvagG1zygWWKz0duUSj");

                        await HttpClient.PostAsync($"{ProgramConfig.DjangoUrl}/bot/{eventEndpoint}",
                            jsonData
                        );
                    }
                    catch (Exception err) {
                        failCount++;
                    }
                });

                if (failCount > 0) {
                    await Console.Out.WriteLineAsync($"Falha ao enviar: {failCount}/{total}");
                }
            }
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
                }
                catch (Exception err) {
                    Console.Out.WriteLine("Falha ao pegar os dados");
                    Console.Out.WriteLine($"Erro: {err.Message}");
                    Console.Out.WriteLine($"StackTrace:\n{err.StackTrace}");
                }

                Console.Out.WriteLine($"Processado {betApi.WebSiteName}");
                Thread.Sleep(ProgramConfig.ThreadSleepMs);
            });

        }
    }

    public static bool CheckURL(string url) {
        return (HttpClient.GetAsync(url).Result).IsSuccessStatusCode;
    }
}

public class ProgramConfig
{

    public ProgramConfig() {
        DjangoUrl = "http://localhost:8000";
        EventType = "soccer";
        ThreadSleepMs = 4000;
        SendEventsToList = true;
    }

    public ProgramConfig(string djangoUrl, string eventType, bool sendEventsToList) {
        DjangoUrl = djangoUrl;
        EventType = eventType;
        SendEventsToList = sendEventsToList;
    }
    public string DjangoUrl { get; set; }
    public string EventType { get; set; }
    public int ThreadSleepMs { get; set; }
    public bool SendEventsToList { get; set; }

    public override string ToString() {
        return $"{nameof(DjangoUrl)}: {DjangoUrl}, {nameof(EventType)}: {EventType}, {nameof(ThreadSleepMs)}: {ThreadSleepMs}";
    }
}