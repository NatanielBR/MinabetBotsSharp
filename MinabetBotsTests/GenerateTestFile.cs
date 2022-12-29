using MinabetBotsWeb.scrapper;
using MinabetBotsWeb.scrapper.soccer;
using Newtonsoft.Json;

var web = new HttpClient();

var betApis = new List<BetApi> {
    new BetsBola(),
    new Betano(),
    new Pansudo(),
};

betApis.ForEach(item => {
    var filePath = $"{Path.GetTempPath()}{item.WebSiteName}_events_test.json";

    var events = item.ListEvents();
    File.WriteAllText(filePath, JsonConvert.SerializeObject(events));
});