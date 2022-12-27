// See https://aka.ms/new-console-template for more information

using MinabetBotsWeb;
using MinabetBotsWeb.scrapper;
using MinabetBotsWeb.scrapper.soccer;

var teamDb = new TeamDb(changeFire: 3);
var combinator = new Combinator("soccer", teamDb);
var web = new HttpClient();

combinator.OnNewSurebet += (_, combination) => {

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
    new BetsBola(web),
    new Betano(web),
    new Pansudo(web),
};

while (true) {
    Console.Out.WriteLine("Processando eventos...");
    
    betApis.ForEach(betApi => {
        Console.Out.WriteLine($"Processando {betApi.WebSiteName}");

        var elements = betApi.ListEvents();
        teamDb.PutAll(elements);
        
        Console.Out.WriteLine($"Processado {betApi.WebSiteName}");
        Thread.Sleep(4000);
    });
    
}