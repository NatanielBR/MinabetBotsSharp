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

    public override List<Event> ListEvents() {
        var urls = ListCampeonatos();
        var events = new List<Event>();
        var actualYear = DateTimeOffset.Now.Year;

        urls.ForEach(item => {
            // exp: https://betsbola.com/Jogos.aspx?idesporte=102&idcampeonato=100003512
            var doc = web.Load($"{urlBase}{item}");

            var leagues = doc.DocumentNode.SelectNodes("//div[@class='pais']");

            if (leagues == null) {
                return;
            }

            leagues.ToList().ForEach(league => {
                var leagueName = WebUtility.HtmlDecode(league.SelectSingleNode("//span[@class='name']").InnerText);

                var eventsEls = league.SelectNodes("//div[@class='containerCards']");

                if (eventsEls == null) {
                    return;
                }

                eventsEls.ToList().ForEach(eventEl => {

                    var eventO = new Event(
                        eventEl
                            .SelectSingleNode("//a[@class='totalOutcomes-button']")
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
                        eventEl.SelectSingleNode("//div[@class='dateAndHour']").let(it => {
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
                        "",
                        "",
                        new EventOdds(
                            0.0d,
                            0.0d,
                            0.0d,
                            0.0d,
                            0.0d
                        ),
                        webSiteName
                        ,
                        ""
                    );

                    events.Add(eventO);
                });
            });
        });

        return events;
    }

    private List<string> ListCampeonatos() {
        var doc = web.Load($"{urlBase}/Campeonatos.aspx");

        if (doc == null) {
            return new();
        }

        var items = doc.DocumentNode.SelectNodes("//div[@class='cardItem']");
        var regex = new Regex("'(.*?)'");

        return items
            .Select(htmlNode => WebUtility.HtmlDecode(htmlNode.GetAttributeValue("onclick", null)))
            .Select(url => regex.Match(url).Value.Replace("'.", "").Replace("'", ""))
            .ToList();
    }
}

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