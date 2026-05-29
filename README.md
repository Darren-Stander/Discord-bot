# 🤖 Discord AI Summarizer Bot

A C# .NET Discord bot that integrates with the Google Gemini API to generate concise summaries of channel conversations. 

This project was built to demonstrate third-party REST API integration, asynchronous programming (non-blocking tasks), and JSON data parsing.

## 📋 Prerequisites
Before you start, you will need:
* **Visual Studio** (or any C# IDE)
* **.NET 8.0 SDK** installed
* A Discord account
* A Google account

## 🔑 Getting Your API Keys
This bot requires two free API keys to function. **Never share these keys or upload them to GitHub.**

### 1. Discord Bot Token
1. Go to the [Discord Developer Portal](https://discord.com/developers/applications).
2. Click **New Application** and give it a name.
3. Navigate to the **Bot** tab on the left menu.
4. Scroll down to **Privileged Gateway Intents** and turn **ON** the **Message Content Intent** (this allows the bot to read the chat).
5. Click **Reset Token** and copy the generated password. 

### 2. Google Gemini API Key
1. Go to [Google AI Studio](https://aistudio.google.com/).
2. Sign in with your Google account.
3. Click **Get API Key** on the left menu.
4. Create a new key and copy it.

## 💻 Installation & Setup

**Step 1: Clone the repository**
Download or clone this repository to your local machine and open the project in Visual Studio.

**Step 2: Add your keys**
To keep your credentials secure, this bot reads keys from local text files rather than hardcoded variables.
1. In the root folder of the project, create a text file named `token.txt` and paste your Discord token inside.
2. Create another text file named `gemini.txt` and paste your Google API key inside.

**Step 3: Update file properties**
Visual Studio needs to know to include these files when it builds the app.
1. In the Solution Explorer, click on `token.txt`.
2. In the Properties window, change **Copy to Output Directory** to **Copy if newer**.
3. Repeat this exact step for `gemini.txt`.

**Step 4: Secure your files**
If you plan to fork or commit changes to this project, ensure both `token.txt` and `gemini.txt` are listed in your `.gitignore` file so they are never uploaded online.

## 🚀 Usage
1. Hit **Start** in Visual Studio to run the console application.
2. Invite the bot to your Discord server using the OAuth2 URL Generator in the Discord Developer Portal (ensure it has `Send Messages` and `View Channels` permissions).
3. In any channel where the bot is present, type the command:
   `!summarize`
4. The bot will instantly offload the request to a background thread, fetch the last 10 messages, and return a neatly formatted AI summary.
