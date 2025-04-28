using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

namespace TheGodfather.Services;

public class OpenWebUiService
{
    private readonly SemaphoreSlim sem;
    
    private readonly string? url;
    private readonly string? key;
    private readonly string? model;
    private readonly Message? sysprompt;

    public bool IsDisabled => 
        string.IsNullOrWhiteSpace(url) 
     && string.IsNullOrWhiteSpace(key) 
     && string.IsNullOrWhiteSpace(model);
    
    public OpenWebUiService(BotConfig cfg)
    {
        this.url = cfg.OpenWebUiUrl;
        this.key = cfg.OpenWebUiKey;
        this.model = cfg.OpenWebUiModel;
        this.sysprompt = CreateSystemPromptMessage(cfg.OpenWebUiSystemPrompt);
        this.sem = new SemaphoreSlim(1, 1);
    }

    private static Message? CreateSystemPromptMessage(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) 
            return null;
        return new Message {
            Role = "system",
            Content = prompt,
        };
    }

    public string GetIconUrl() 
        => "https://docs.openwebui.com/images/logo.png";

    public string GetBanner() 
        => this.IsDisabled ? "No OpenWebUI service" : $"{this.model} @ {this.url}";

    public async Task<string?> ChatAsync(string query)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;
        
        string endpoint = $"{this.url}/api/chat/completions";
        var messages = new List<Message>();
        if (this.sysprompt is not null)
            messages.Add(this.sysprompt);
        messages.Add(new Message { Role = "user", Content = query });
        var data = new ChatPostRequestData {
            Model = this.model!,
            Messages = messages,
        };
        
        await this.sem.WaitAsync();
        try {
            HttpResponseMessage responseMsg = await HttpService.PostJsonAsync(endpoint, data, this.key).ConfigureAwait(false);
            if (!responseMsg.IsSuccessStatusCode) {
                Log.Error("OpenWebUI API returned code: {OpenWebUiResponse.ResponseCode}", responseMsg.StatusCode);
                return null;
            }
            
            string responseJson = await responseMsg.Content.ReadAsStringAsync().ConfigureAwait(false);
            ChatPostRequestResponse? response = JsonConvert.DeserializeObject<ChatPostRequestResponse>(responseJson);
            if (response is null)
                return null;

            if (response.Object != "chat.completion") {
                Log.Error("OpenWebUI API returned unexpected object: {OpenWebUiResponse.Object}", response.Object);
                return null;
            }

            Choice? responseChoice = response.Choices.FirstOrDefault();
            if (responseChoice is null) {
                Log.Error("OpenWebUI API returned zero response choices");
                return null;
            }
            
            return responseChoice.Message.Content;
        } catch (Exception e) when (e is HttpRequestException or JsonSerializationException) {
            Log.Error(e, "Failed to fetch/deserialize OpenWebUI API response");
        } finally {
            this.sem.Release();
        }

        return null;
    }
    
    private class ChatPostRequestData
    {
        [JsonProperty("model")]
        public required string Model { get; set; }
        
        [JsonProperty("messages")]
        public required List<Message> Messages { get; set; }
    }
    
    private class Message
    {
        [JsonProperty("role")]
        public required string Role { get; set; }
        
        [JsonProperty("content")]
        public required string Content { get; set; }
    }
    
    private class ChatPostRequestResponse
    {
        [JsonProperty("id")]
        public required string Id { get; set; }
        
        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("choices")]
        public required List<Choice> Choices { get; set; }
    }
    
    private class Choice
    {
        [JsonProperty("index")]
        public required int Index { get; set; }

        [JsonProperty("message")]
        public required Message Message { get; set; }
    }
}