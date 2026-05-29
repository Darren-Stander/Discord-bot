using Discord;
using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;

// Start of the file and the code //


// Creating the bot client
var config = new DiscordSocketConfig()
{
    // This will make the bot use the latest features of the Discord API, you can remove this if you want to use older features
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
};
var client = new DiscordSocketClient(config);

// Setting up the simple log so we can see what the bo tis doing in VS
client.Log += (msg) =>
{

    Console.WriteLine(msg);
    return Task.CompletedTask;

};

// The bot will now listen for messages
client.MessageReceived += HandleCommandAsync;

async Task HandleCommandAsync(SocketMessage message)
{
    // This here prevents the bot from being spammed to prevent infinite spam loops
    if (message.Author.IsBot) return;

    // This checks if the user typed our calling command
    if (message.Content == "!ping")
    {
        // The bot will now repsond to your message
        await message.Channel.SendMessageAsync("Pong!");
    }

    if (message.Content == "!drop")
    {
        string[] lootPool = { "vector .45 ACP", "P416", "LVOA-C", "Nemesis", "Eagle Bearer" };

        // Now we create a RNG
        Random rand = new Random();

        // Now we are picking a random item from the array above
        int randomIndex = rand.Next(lootPool.Length);

        // Now it grabs the random item from the loot pool
        string rewardItem = lootPool[randomIndex];

        var embedBuilder = new EmbedBuilder()
            .WithTitle("📦 Supply Drop Opened!")
            .WithDescription($"Agent, you successfully extracted a **{rewardItem}**.")
            .WithColor(Color.Orange)    // For Items with higher rarity
            .AddField("Conition", "Pristine", true)
            .AddField("Rarity", "High-End", true)
            .WithFooter("Division Tech Network")
            .WithCurrentTimestamp();

        await message.Channel.SendMessageAsync(text: "", embed: embedBuilder.Build());
    }

    if (message.Content == "!summarize")
    {
        // the bot will now send a message to the channel letting the user know that it is working
        var loadingMsg = await message.Channel.SendMessageAsync("⏳ *Gathering the last 10 messages...*");

        // This code now fetches the last 10 messages from the channel
        var pastMessages = await message.Channel.GetMessagesAsync(10).FlattenAsync();

        // This gathers all the texts in a document so that we can give it to the Gemini
        string conversationLog = "";

        // Now we will loop through the messages thats downloaded

        foreach (var msg in pastMessages)
        {
            // we now get the messages but skip the message from the bot and the blank message above
            if (msg.Author.IsBot || string.IsNullOrWhiteSpace(msg.Content)) continue;

            // Now we will format it so Gemini knows exactly who said what
            conversationLog += $"{msg.Author.Username}: {msg.Content}\n";

            // This is how we call our Gemini API, you can replace this with your own API call if you want to use a different model
            string keyPath = "gemini.txt";

            // This is to check if there are any errors with calling the API
            if (!File.Exists(keyPath))
            {
                await message.Channel.SendMessageAsync("⚠️ **Error:** API Key file is missing. The bot cannot contact the AI.");
                Console.WriteLine("CRITICAL: gemini.txt not found. Make sure 'Copy to Output Directory' is set to 'Copy if newer'.");
                return; // This stops the command from trying to continue
            }

            //Read the key (Trim ensures we remove any accidental spaces or new lines you might have copied)
            string geminiKey = File.ReadAllText(keyPath).Trim();

            //Make sure the file isn't completely empty
            if (string.IsNullOrWhiteSpace(geminiKey))
            {
                await message.Channel.SendMessageAsync("⚠️ **Error:** API Key file is empty.");
                return;
            }

            // Now we will create the HTTP client and the request to send to Gemini
            using var httpClient = new HttpClient();
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={geminiKey}";

            //  We will now create a package for the data into a format that gemini will understand
            var requestBody = new
            {
                contents = new[]
                {
                     new { parts = new[] { new { text = $"Please summarize the following chat log into 3 concise bullet points:\n\n{conversationLog}" } } }
                }
            };


            // Now we will convert the JSON string into a universal format that all web API can understand
            string jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            //Now we will send the request to AI and get the response
            var response = await httpClient.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();
            // --- DEBUGGING: Print the exact response to Visual Studio so we can see what Google said ---
            Console.WriteLine("\n--- RAW GOOGLE API RESPONSE ---");
            Console.WriteLine(responseString);
            Console.WriteLine("-------------------------------\n");

            // 6. Parse the JSON safely
            using JsonDocument doc = JsonDocument.Parse(responseString);

            // Defensive Check 1: Did Google send us an error object?
            if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement))
            {
                // Extract Google's specific error message
                string errorMessage = errorElement.GetProperty("message").GetString();
                await loadingMsg.ModifyAsync(x => x.Content = $"⚠️ **Google API Error:** {errorMessage}");
                return; // Stop the code here
            }

            //Navigate through Google's JSON response to find just the text we want
            if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates))
            {
                string summaryText = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                //Build a professional Embed for the final results
                var embed = new EmbedBuilder()
                            .WithTitle("🤖 AI Channel Summary")
                            .WithDescription(summaryText)
                            .WithColor(Color.Blue)
                            .WithFooter("Powered by Gemini AI")
                            .WithCurrentTimestamp()
                            .Build();

                await loadingMsg.ModifyAsync(x =>
                {
                    x.Content = "";
                    x.Embed = embed;
                });
            }
            else
            {
                // If there's no error object BUT also no candidates (usually a safety filter block)
                await loadingMsg.ModifyAsync(x => x.Content = "⚠️ **Error:** The AI responded, but returned no summary. It may have been blocked by safety filters.");
            }
        }
    }
}

// You add your token in here
string token = File.ReadAllText("token.txt");

// Now we login and start the bot using async/await to not freeze the program
await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

// This will keep the program running forever, without this line the console will close once ASAP
await Task.Delay(-1);


// End of the file and the application //