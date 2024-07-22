using System.Text.Json;
using TechTalk.SpecFlow;


[Binding]
public class DeleteLabelSteps
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _token;
    private HttpResponseMessage _response;

    public DeleteLabelSteps()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
        _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    }

    [Given(@"I have a board with id ""(.*)"" and a card with id ""(.*)""")]
    public void GivenIHaveABoardWithIdAndACardWithId(string boardId, string cardId)
    {
        ScenarioContext.Current["boardId"] = boardId;
        ScenarioContext.Current["cardId"] = cardId;
    }

    [Given(@"the card has a label with id ""(.*)""")]
    public void GivenTheCardHasALabelWithId(string labelId)
    {
        ScenarioContext.Current["labelId"] = labelId;
    }

    [When(@"I delete the label from the card")]
    public async Task WhenIDeleteTheLabelFromTheCard()
    {
        string cardId = (string)ScenarioContext.Current["cardId"];
        string labelId = (string)ScenarioContext.Current["labelId"];
        var requestUri = $"https://api.trello.com/1/cards/{cardId}/idLabels/{labelId}?key={_apiKey}&token={_token}";

        _response = await _httpClient.DeleteAsync(requestUri);
    }

    [Then(@"the label should no longer exist on the card")]
    public async Task ThenTheLabelShouldNoLongerExistOnTheCard()
    {
        _response.EnsureSuccessStatusCode();
        string cardId = (string)ScenarioContext.Current["cardId"];
        var requestUri = $"https://api.trello.com/1/cards/{cardId}?key={_apiKey}&token={_token}";
        var response = await _httpClient.GetAsync(requestUri);
        var content = await response.Content.ReadAsStringAsync();
        var card = JsonDocument.Parse(content).RootElement;
        var labels = card.GetProperty("idLabels").EnumerateArray().Select(l => l.GetString()).ToList();
        var labelId = ScenarioContext.Current["labelId"].ToString();
        Assert.That(labels, Does.Not.Contain(labelId));
    }
}
