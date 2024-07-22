using System.Text.Json;
using TechTalk.SpecFlow;

namespace API_Task.Steps;

[Binding]
public class DeleteLabelStepDefinitions
{
    private HttpClient _httpClient;
    private string _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
    private string _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
    private string _baseUrl = "https://api.trello.com/1";
    private string _boardId;
    private string _listId;
    private string _cardId;
    private string _labelId;

    public DeleteLabelStepDefinitions()
    {
        _httpClient = new HttpClient();
    }

    [Given(@"I have a Trello account 2")]
    public void GivenIHaveATrelloAccount()
    {
        Assert.IsNotEmpty(_apiKey);
        Assert.IsNotEmpty(_token);
    }

    [Given(@"I have a board named ""(.*)"" with a card named ""(.*)"" 2")]
    public async Task GivenIHaveABoardNamedWithACardNamed(string boardName, string cardName)
    {
        _boardId = await CreateBoard(boardName);
        _listId = await CreateList(_boardId, "To Do");
        _cardId = await CreateCard(_listId, cardName);
        _labelId = await CreateLabel(_boardId, "Urgent");
        await AddLabelToCard(_cardId, _labelId);
    }

    [When(@"I delete the label ""(.*)"" from ""(.*)""")]
    public async Task WhenIDeleteTheLabelFrom(string labelName, string cardName)
    {
        await RemoveLabelFromCard(_cardId, _labelId);
    }

    [Then(@"(.*) should no longer have the label ""(.*)""")]
    public async Task ThenShouldNoLongerHaveTheLabel(string cardName, string labelName)
    {
        var card = await GetCard(_cardId);
        var labels = card.GetProperty("labels").EnumerateArray();
        foreach (var label in labels)
        {
            Assert.AreNotEqual(label.GetProperty("name").GetString(), labelName);
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

    private async Task<string> CreateLabel(string boardId, string labelName)
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/labels?name={labelName}&color=red&idBoard={boardId}&key={_apiKey}&token={_token}", null);
        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("id").GetString();
    }

    private async Task AddLabelToCard(string cardId, string labelId)
    {
        await _httpClient.PostAsync($"{_baseUrl}/cards/{cardId}/idLabels?value={labelId}&key={_apiKey}&token={_token}", null);
    }

    private async Task RemoveLabelFromCard(string cardId, string labelId)
    {
        await _httpClient.DeleteAsync($"{_baseUrl}/cards/{cardId}/idLabels/{labelId}?key={_apiKey}&token={_token}");
    }

    private async Task<JsonElement> GetCard(string cardId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/cards/{cardId}?key={_apiKey}&token={_token}");
        var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return jsonDocument.RootElement;
    }
}