using System.Text.Json;
using TechTalk.SpecFlow;

[Binding]
public class CopyCardsSteps
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _token;
    private HttpResponseMessage _response;

    public CopyCardsSteps()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
        _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    }

    [Given(@"I have a source board with id ""(.*)""")]
    public void GivenIHaveASourceBoardWithId(string sourceBoardId)
    {
        ScenarioContext.Current["sourceBoardId"] = sourceBoardId;
    }

    [Given(@"I have a destination board with id ""(.*)""")]
    public void GivenIHaveADestinationBoardWithId(string destinationBoardId)
    {
        ScenarioContext.Current["destinationBoardId"] = destinationBoardId;
    }

    [Given(@"I have a card with id ""(.*)"" on the source board")]
    public void GivenIHaveACardWithIdOnTheSourceBoard(string cardId)
    {
        ScenarioContext.Current["cardId"] = cardId;
    }

    [When(@"I copy the card to the destination board")]
    public async Task WhenICopyTheCardToTheDestinationBoard()
    {
        string sourceBoardId = (string)ScenarioContext.Current["sourceBoardId"];
        string destinationBoardId = (string)ScenarioContext.Current["destinationBoardId"];
        string cardId = (string)ScenarioContext.Current["cardId"];

        var requestUri = $"https://api.trello.com/1/cards/{cardId}/copy?key={_apiKey}&token={_token}&idBoard={destinationBoardId}";

        _response = await _httpClient.PostAsync(requestUri, null);
    }

    [Then(@"the card should exist on the destination board")]
    public async Task ThenTheCardShouldExistOnTheDestinationBoard()
    {
        _response.EnsureSuccessStatusCode();
        var content = await _response.Content.ReadAsStringAsync();
        var card = JsonDocument.Parse(content).RootElement;
        string destinationBoardId = (string)ScenarioContext.Current["destinationBoardId"];

        Assert.That(card.GetProperty("idBoard").GetString(), Is.EqualTo(destinationBoardId));
    }
}