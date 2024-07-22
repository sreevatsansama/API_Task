using System.Text.Json;
using TechTalk.SpecFlow;

namespace API_Task.Steps;

[Binding]
public class ReorderCardsStepDefinitions
{
    private HttpClient _httpClient;
    private string _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
    private string _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    private string _baseUrl = "https://api.trello.com/1";
    private string _boardId;
    private string _listId;
    private string _card1Id;
    private string _card2Id;
    private string _card3Id;

    public ReorderCardsStepDefinitions()
    {
        _httpClient = new HttpClient();
    }

    [Given(@"I have a Trello account")]
    public void GivenIHaveATrelloAccount()
    {
        Assert.IsNotEmpty(_apiKey);
        Assert.IsNotEmpty(_token);
    }

    [Given(@"I have a board named ""(.*)"" with a list named ""(.*)""")]
    public async Task GivenIHaveABoardNamedWithAListNamed(string boardName, string listName)
    {
        _boardId = await CreateBoard(boardName);
        _listId = await CreateList(_boardId, listName);
        _card1Id = await CreateCard(_listId, "Card 1");
        _card2Id = await CreateCard(_listId, "Card 2");
        _card3Id = await CreateCard(_listId, "Card 3");
    }

    [When(@"I move ""(.*)"" to the position after ""(.*)"" in the ""(.*)"" list")]
    public async Task WhenIMoveToThePositionAfterInTheList(string cardToMove, string referenceCard, string listName)
    {
        var cardToMoveId = await GetCardIdByName(_listId, cardToMove);
        var referenceCardId = await GetCardIdByName(_listId, referenceCard);
        await ReorderCard(cardToMoveId, referenceCardId);
    }

    [Then(@"the ""(.*)"" list should contain ""(.*)"" in that order")]
    public async Task ThenTheListShouldContainInThatOrder(string listName, string expectedOrder)
    {
        var cards = expectedOrder.Split(", ");
        var cardIds = await GetCardsInList(_listId);
        for (int i = 0; i < cards.Length; i++)
        {
            var cardId = await GetCardIdByName(_listId, cards[i]);
            Assert.AreEqual(cardId, cardIds[i]);
        }
    }

    private async Task<string> CreateBoard(string boardName)
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/boards/?name={boardName}&key={_apiKey}&token={_token}", null);
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("id").GetString();
    }

    private async Task<string> CreateList(string boardId, string listName)
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/boards/{boardId}/lists?name={listName}&key={_apiKey}&token={_token}", null);
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("id").GetString();
    }

    private async Task<string> CreateCard(string listId, string cardName)
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/cards?name={cardName}&idList={listId}&key={_apiKey}&token={_token}", null);
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("id").GetString();
    }

    private async Task<string> GetCardIdByName(string listId, string cardName)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/lists/{listId}/cards?key={_apiKey}&token={_token}");
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        foreach (var card in json.RootElement.EnumerateArray())
        {
            if (card.GetProperty("name").GetString() == cardName)
            {
                return card.GetProperty("id").GetString();
            }
        }
        return string.Empty;
    }

    private async Task ReorderCard(string cardId, string referenceCardId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/cards/{referenceCardId}?key={_apiKey}&token={_token}");
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var position = json.RootElement.GetProperty("pos").GetDouble() + 1;
        await _httpClient.PutAsync($"{_baseUrl}/cards/{cardId}/pos?value={position}&key={_apiKey}&token={_token}", null);
    }

    private async Task<string[]> GetCardsInList(string listId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/lists/{listId}/cards?key={_apiKey}&token={_token}");
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var cardIds = new List<string>();
        foreach (var card in json.RootElement.EnumerateArray())
        {
            cardIds.Add(card.GetProperty("id").GetString());
        }
        return cardIds.ToArray();
    }
}
