using Discord;
using Discord.WebSocket;

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
}

// You add your token in here
string token = File.ReadAllText("token.txt");

// Now we login and start the bot using async/await to not freeze the program
await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

// This will keep the program running forever, without this line the console will close once ASAP
await Task.Delay(-1);