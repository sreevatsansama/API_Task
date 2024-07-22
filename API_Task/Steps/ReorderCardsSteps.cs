using System.Text.Json;
using TechTalk.SpecFlow;

[Binding]
public class ReorderCardsSteps
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _token;
    private HttpResponseMessage _response;

    public ReorderCardsSteps()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
        _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    }

    [Given(@"I have a board with id ""(.*)"" and a list with id ""(.*)""")]
    public void GivenIHaveABoardWithIdAndAListWithId(string boardId, string listId)
    {
        ScenarioContext.Current["boardId"] = boardId;
        ScenarioContext.Current["listId"] = listId;
    }

    [Given(@"I have cards with ids ""(.*)"" and ""(.*)"" on the list")]
    public void GivenIHaveCardsWithIdsAndOnTheList(string cardId1, string cardId2)
    {
        ScenarioContext.Current["cardId1"] = cardId1;
        ScenarioContext.Current["cardId2"] = cardId2;
    }

    [When(@"I move ""(.*)"" before ""(.*)""")]
    public async Task WhenIMoveCardId1BeforeCardId2(string cardId1, string cardId2)
    {
        string listId = (string)ScenarioContext.Current["listId"];
        var requestUri = $"https://api.trello.com/1/cards/{cardId1}/pos?key={_apiKey}&token={_token}&value=top";
        _response = await _httpClient.PutAsync(requestUri, null);
    }

    [Then(@"(.*) should be positioned before (.*) on the list")]
    public async Task ThenCardId1ShouldBePositionedBeforeCardId2OnTheList(string cardId1, string cardId2)
    {
        _response.EnsureSuccessStatusCode();
        var requestUri = $"https://api.trello.com/1/lists/{ScenarioContext.Current["listId"]}/cards?key={_apiKey}&token={_token}";
        var response = await _httpClient.GetAsync(requestUri);
        var content = await response.Content.ReadAsStringAsync();
        var cards = JsonDocument.Parse(content).RootElement.EnumerateArray();

        bool foundCard1 = false;
        foreach (var card in cards)
        {
            if (card.GetProperty("id").GetString() == cardId1)
            {
                foundCard1 = true;
            }
            if (card.GetProperty("id").GetString() == cardId2)
            {
                Assert.That(foundCard1, NUnit.Framework.Is.True);
                break;
            }
        }
    }
}
