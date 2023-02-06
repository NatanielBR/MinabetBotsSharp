using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace MinabetBotsWeb.scrapper.soccer;

public class BetsBola : BetApi {
    private HtmlWeb web = new();
    private CultureInfo brazilCulture = new("pt-BR");
    private string urlBase = "https://betsbola.com/sistema_v2/usuarios/simulador/desktop/";

    public BetsBola(HttpClient client)
        : base("BetsBola", "https://betsbola.com", client) {
    }

    public override List<SportEvent> ListEvents() {
        var urls = ListCampeonatos();
        var events = new List<SportEvent>();

        // Para cada url
        urls.ForEach(item => {
            // exp: https://betsbola.com/Jogos.aspx?idesporte=102&idcampeonato=100003512
            var doc = web.Load($"{urlBase}{item}");

            var leagues = doc.DocumentNode.SelectNodes(".//div[@class='pais']");

            if (leagues == null) {
                return;
            }

            // Pra cada liga, normalmente é só um mas em paginas como "Jogos de Sexta feira" irá ter varias
            leagues.ToList().ForEach(league => {
                var leagueName = WebUtility.HtmlDecode(league.SelectSingleNode(".//span[@class='name']").InnerText);

                var eventsEls = league.SelectNodes(".//div[@class='containerCards']");

                if (eventsEls == null) {
                    return;
                }

                eventsEls.ToList().ForEach(eventEl => {
                    var teamNames = eventEl.SelectNodes(".//div[@class='nameTeam']").Select(item => item.InnerText).ToList();

                    // Odds 1x2
                    var oddsValues = eventEl.SelectNodes(".//a[@class='odd']")
                        .Select(item => Double.Parse(item.InnerText.Replace(",", "."))).ToList();

                    try {
                        var eventO = new SportEvent(
                            eventEl
                                .SelectSingleNode(".//a[@class='totalOutcomes-button']")
                                .Attributes["href"].Value.let(it => {
                                    var last = it.LastIndexOf("=", StringComparison.Ordinal);

                                    return it[last..];
                                }),
                            "",
                            item.let(s => {
                                var last = item.LastIndexOf("=", StringComparison.Ordinal);

                                return s[last..];
                            }),
                            leagueName,
                            eventEl.SelectSingleNode(".//div[@class='dateAndHour']").let(it => {
                                var date = it.SelectSingleNode("//span[@class='date']").InnerText;
                                MonthToNumber.TryParse(date.Split("/")[1], true, out MonthToNumber monthNumber);

                                // Irá transformar o nome do mês para o seu numero, o nome do mês esta em português
                                date = $"{date.Split("/")[0]}/{(byte)monthNumber}";

                                var hour = it.SelectSingleNode("//span[@class='hour']").InnerText;

                                return DateTime.ParseExact($"{date} {hour}", "dd/MM HH:mm",
                                    brazilCulture, DateTimeStyles.None).let(time =>
                                    new DateTimeOffset(time,
                                        TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time").GetUtcOffset(time))
                                );
                            }),
                            teamNames[0],
                            teamNames[1],
                            
                            new(
                                oddsValues[0],
                                oddsValues[2],
                                oddsValues[1],
                                0.0d,
                                0.0d,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null
                            ),
                            webSiteName
                            ,
                            $"{urlBase}{WebUtility.HtmlDecode(eventEl.SelectSingleNode(".//a[@class='totalOutcomes-button']").Attributes["href"].Value[2..])}"
                        );

                        events.Add(eventO);
                    }
                    catch (Exception _) {

                    }
                });
            });
        });

        Console.Out.WriteLine($"Pegando mercado para {events.Count} eventos");

        Parallel.ForEach(events, item => {
            Thread.Sleep(400);
            FillMercadoDeGol(item);
        });

        return events;
    }

    private List<string> ListCampeonatos() {
        var doc = web.Load($"{urlBase}/Campeonatos.aspx");

        if (doc == null) {
            return new();
        }

        var items = doc.DocumentNode.SelectNodes("//div[@class='cardItem']");

        if (items == null) {
            return new();
        }
        var regex = new Regex("'(.*?)'");

        return items
            .Select(htmlNode => WebUtility.HtmlDecode(htmlNode.GetAttributeValue("onclick", null)))
            .Select(url => regex.Match(url).Value.Replace("'.", "").Replace("'", ""))
            .ToList();
    }

    private void FillMercadoDeGol(SportEvent sportEvent) {
        var doc = web.Load(sportEvent.url);


        if (doc == null || sportEvent.odds == null) {
            return;
        }

        var allElements = doc.DocumentNode.SelectNodes("//div[@class='eventdetail-optionItem']");

        if (allElements == null || !doc.DocumentNode.SelectNodes("//div[@class='eventdetail-market']//div[@id='divHeader']//span[@class='name']")
                .Select(item => item.InnerText)
                .Contains("Total de Gols no Jogo")) {
            return;
        }

        var mais25El = allElements.First(item => item.InnerText.Contains("Mais de 2,5"));
        var menos25El = allElements.First(item => item.InnerText.Contains("Menos de 2,5"));

        sportEvent.odds.less2and5odds = Double.Parse(menos25El.SelectSingleNode(".//a[@class='odd']").InnerText);
        sportEvent.odds.more2and5odds = Double.Parse(mais25El.SelectSingleNode(".//a[@class='odd']").InnerText);

    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
enum MonthToNumber : byte {
    JAN = 1,
    FEV = 2,
    MAR = 3,
    ABR = 4,
    MAI = 5,
    JUN = 6,
    JUL = 7,
    AGO = 8,
    SET = 9,
    OUT = 10,
    NOV = 11,
    DEZ = 12
}