using System.Text.Json;
using TechTalk.SpecFlow;

namespace API_Task.Steps;

[Binding]
public class CopyCardsStepDefinitions
{
    private HttpClient _httpClient;
    private string _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
    private string _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    private string _baseUrl = "https://api.trello.com/1";
    private string _boardAId;
    private string _boardBId;
    private string _cardId;

    public CopyCardsStepDefinitions()
    {
        _httpClient = new HttpClient();
    }

    [Given(@"I have a Trello account 1")]
    public void GivenIHaveATrelloAccount()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_apiKey, Is.Not.Empty);
            Assert.That(_token, Is.Not.Empty);
        });
    }

    [Given(@"I have a board named ""(.*)"" with a card named ""(.*)"" 1")]
    public async Task GivenIHaveABoardNamedWithACardNamed(string boardName, string cardName)
    {
        _boardAId = await CreateBoard(boardName);
        var listId = await CreateList(_boardAId, "To Do");
        _cardId = await CreateCard(listId, cardName);
    }

    [Given(@"I have a board named ""(.*)""")]
    public async Task GivenIHaveABoardNamed(string boardName)
    {
        _boardBId = await CreateBoard(boardName);
    }

    [When(@"I copy the card ""(.*)"" from ""(.*)"" to ""(.*)""")]
    public async Task WhenICopyTheCardFromTo(string cardName, string sourceBoardName, string targetBoardName)
    {
        await CopyCard(_cardId, _boardBId);
    }

    [Then(@"(.*) should exist on ""(.*)""")]
    public async Task ThenShouldExistOn(string cardName, string boardName)
    {
        var boardId = boardName == "Board A" ? _boardAId : _boardBId;
        var listId = await GetListIdByBoardId(boardId);
        var cardId = await GetCardIdByName(listId, cardName);
        Assert.That(cardId, Is.Not.Empty);
    }

    [Then(@"(.*) should still exist on ""(.*)""")]
    public async Task ThenShouldStillExistOn(string cardName, string boardName)
    {
        var boardId = boardName == "Board A" ? _boardAId : _boardBId;
        var listId = await GetListIdByBoardId(boardId);
        var cardId = await GetCardIdByName(listId, cardName);
        Assert.That(cardId, Is.Not.Empty);
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

    private async Task<string> GetListIdByBoardId(string boardId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/boards/{boardId}/lists?key={_apiKey}&token={_token}");
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        foreach (var list in json.RootElement.EnumerateArray())
        {
            return list.GetProperty("id").GetString();
        }
        return string.Empty;
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

    private async Task CopyCard(string cardId, string targetBoardId)
    {
        await _httpClient.PostAsync($"{_baseUrl}/cards?copy=true&idCardSource={cardId}&idBoard={targetBoardId}&key={_apiKey}&token={_token}", null);
    }
}
