namespace MinabetBotsWeb.scrapper; 

public abstract class BetApi {

    public string WebSiteName;
    protected string urlHome;
    protected HttpClient client;

    public BetApi(string webSiteName, string urlHome, HttpClient client) {
        this.WebSiteName = webSiteName;
        this.urlHome = urlHome;
        this.client = client;
    }

    public abstract List<SportEvent> ListEvents();

}