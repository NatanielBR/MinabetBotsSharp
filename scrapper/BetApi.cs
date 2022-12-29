namespace MinabetBotsWeb.scrapper; 

public abstract class BetApi {

    public string WebSiteName;
    protected string urlHome;
    protected HttpClient client = new ();

    public BetApi(string webSiteName, string urlHome) {
        this.WebSiteName = webSiteName;
        this.urlHome = urlHome;
    }

    public abstract List<SportEvent> ListEvents();

}