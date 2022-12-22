namespace MinabetBotsWeb.scrapper; 

public abstract class BetApi {

    protected string webSiteName;
    protected string urlHome;
    protected HttpClient client;

    public BetApi(string webSiteName, string urlHome, HttpClient client) {
        this.webSiteName = webSiteName;
        this.urlHome = urlHome;
        this.client = client;
    }

    public abstract List<Event> ListEvents();

}