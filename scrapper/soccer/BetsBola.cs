using System.Globalization;
using System.Net;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace MinabetBotsWeb.scrapper.soccer;

public class BetsBola : BetApi {
    private HtmlWeb web = new();
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

                            var hour = it.SelectSingleNode("//span[@class='hour']").InnerText;
                            
                            // TODO: Tem que resolver isso
                            return DateTimeOffset.Now;
                            // return DateTimeOffset.ParseExact(
                            //     $"{date}/{actualYear} {hour}:00 -3",
                            //     "dd/MMM/yyyy hh:mm:ss t",
                            //     new CultureInfo("pt-BR"));
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