// ============================================================================
// AI CLIENT - TALKS TO ARTIFICIAL INTELLIGENCE TO FIX BROKEN TESTS
// ============================================================================
// This file asks AI (Ollama or OpenAI) to suggest new element locators
// when the original locator fails to find an element on the page.
// ============================================================================

using System.Text;
using System.Text.Json;

namespace SeleniumSelfHealing.Utilities;

/// <summary>
/// AI Client: Communicates with AI to get smart suggestions for fixing broken element locators
/// Think of this as calling a robot expert who looks at your webpage and tells you how to find elements
/// </summary>
public class LlmClient
{
    // STEP 1: Store configuration (settings like which AI to use, API key, etc.)
    private readonly LlmConfig _config;

    // STEP 2: Create web client to send requests to AI over the internet
    private readonly HttpClient _httpClient;

    /// <summary>
    /// CONSTRUCTOR: Sets up the AI client when the program starts
    /// </summary>
    /// <param name="config">Settings that tell us which AI to use and how to connect</param>
    public LlmClient(LlmConfig config)
    {
        // Save the configuration so we can use it later
        _config = config;

        // Create a new HTTP client (tool to make internet requests)
        _httpClient = new HttpClient
        {
            // Set timeout to 30 seconds - if AI doesn't respond in 30 seconds, give up
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// MAIN METHOD: Ask AI for a better way to find an element
    /// This is called when Selenium can't find an element using the original locator
    /// </summary>
    /// <param name="pageSource">The HTML code of the current webpage</param>
    /// <param name="failedLocator">The locator that didn't work (e.g., "By.Id: username")</param>
    /// <param name="elementDescription">Human-readable description of what we're looking for (e.g., "Username input field")</param>
    /// <returns>A new locator string suggested by AI (e.g., "//input[@id='user']")</returns>
    public async Task<string> GetSuggestedLocator(string pageSource, string failedLocator, string elementDescription)
    {
        // STEP 1: Build a question to ask the AI
        // This creates a message explaining what went wrong and asking for help
        var prompt = BuildPrompt(pageSource, failedLocator, elementDescription);

        // STEP 2: Check which AI service we're using (Local Ollama or Cloud OpenAI)
        if (_config.Provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            // Use LOCAL AI (Ollama running on your computer - FREE)
            return await CallLocalLlm(prompt);
        }

        // Otherwise, use CLOUD AI (OpenAI/ChatGPT - PAID)
        return await CallOpenAi(prompt);
    }

    /// <summary>
    /// BUILD PROMPT: Create a detailed question to ask the AI
    /// This explains the problem to AI in a way it can understand
    /// </summary>
    /// <param name="pageSource">Full HTML of the webpage</param>
    /// <param name="failedLocator">The locator that failed</param>
    /// <param name="elementDescription">What element we're trying to find</param>
    /// <returns>A formatted question/prompt for the AI</returns>
    private string BuildPrompt(string pageSource, string failedLocator, string elementDescription)
    {
        // Create a detailed message asking AI for help
        // The AI will read this and suggest a new locator
        return $@"You are a Selenium test automation expert. 

PROBLEM:
The following locator failed to find the element: {failedLocator}

WHAT I'M LOOKING FOR:
{elementDescription}

CURRENT PAGE HTML:
{TruncateHtml(pageSource, 3000)}

TASK:
Please suggest a new XPath or CSS selector that would successfully find this element. 
Return ONLY the selector string, nothing else - no explanations, no markdown.

EXAMPLES OF GOOD RESPONSES:
//input[@id='username']
#searchInput
input[name='search']";
    }

    /// <summary>
    /// TRUNCATE HTML: Cut down the webpage HTML if it's too long
    /// AI can't read millions of characters, so we limit to 3000 characters
    /// </summary>
    /// <param name="html">The full HTML of the webpage</param>
    /// <param name="maxLength">Maximum number of characters to keep (default: 3000)</param>
    /// <returns>Shortened HTML if it was too long, or original if it was short enough</returns>
    private string TruncateHtml(string html, int maxLength)
    {
        // If HTML is short enough, return it as-is
        if (html.Length <= maxLength)
            return html;

        // If HTML is too long, cut it and add a note
        return html.Substring(0, maxLength) + "\n... [HTML truncated - was too long]";
    }

    /// <summary>
    /// CALL LOCAL LLM: Send request to Ollama (AI running on your computer)
    /// This is FREE but requires Ollama to be installed and running
    /// </summary>
    /// <param name="prompt">The question we're asking the AI</param>
    /// <returns>AI's suggested locator string</returns>
    private async Task<string> CallLocalLlm(string prompt)
    {
        try
        {
            // STEP 1: Package the request for Ollama
            // Create a JSON object with all the information Ollama needs
            var requestBody = new
            {
                model = _config.Model,              // Which AI model to use (e.g., "qwen3-coder:480b-cloud")
                prompt = prompt,                     // The question we're asking
                stream = false,                      // Don't stream the response, give us everything at once
                options = new
                {
                    temperature = _config.Temperature,  // How creative should AI be? (0.1 = very consistent)
                    num_predict = _config.MaxTokens     // Maximum length of AI's response (1000 characters)
                }
            };

            // STEP 2: Convert our request to JSON format (language computers understand)
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),  // Convert to JSON
                Encoding.UTF8,                          // Use UTF-8 encoding (standard text format)
                "application/json"                      // Tell server we're sending JSON
            );

            // STEP 3: Send the request to Ollama (usually at http://localhost:11434)
            var response = await _httpClient.PostAsync($"{_config.BaseUrl}/api/generate", content);

            // STEP 4: Check if request was successful (throws error if it failed)
            response.EnsureSuccessStatusCode();

            // STEP 5: Read Ollama's response
            var responseBody = await response.Content.ReadAsStringAsync();

            // STEP 6: Parse the JSON response to get the actual suggestion
            var jsonDoc = JsonDocument.Parse(responseBody);

            // STEP 7: Extract the "response" field which contains AI's suggested locator
            return jsonDoc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            // If ANYTHING goes wrong (Ollama not running, network error, etc.)
            // Print error message and return empty string (no suggestion)
            Console.WriteLine($"❌ Local AI Error: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// CALL OPENAI: Send request to OpenAI/ChatGPT (Cloud AI service)
    /// This is PAID but very accurate and fast
    /// Requires an API key from OpenAI
    /// </summary>
    /// <param name="prompt">The question we're asking the AI</param>
    /// <returns>AI's suggested locator string</returns>
    private async Task<string> CallOpenAi(string prompt)
    {
        try
        {
            // STEP 1: Package the request for OpenAI
            // OpenAI uses a different format than Ollama
            var requestBody = new
            {
                model = _config.Model,              // Which model (e.g., "gpt-4o", "gpt-3.5-turbo")
                messages = new[]                     // OpenAI expects messages in chat format
                {
                    new {
                        role = "user",               // We are the user asking a question
                        content = prompt             // This is our question
                    }
                },
                temperature = _config.Temperature,   // How creative (0.1 = consistent answers)
                max_tokens = _config.MaxTokens       // Maximum response length
            };

            // STEP 2: Convert request to JSON
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),  // Make it JSON
                Encoding.UTF8,                          // Standard encoding
                "application/json"                      // Tell server it's JSON
            );

            // STEP 3: Add authorization header (your API key - like a password)
            _httpClient.DefaultRequestHeaders.Clear();  // Remove old headers
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

            // STEP 4: Send request to OpenAI's servers
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",  // OpenAI's API endpoint
                content
            );

            // STEP 5: Check if successful (throws error if API key is wrong, quota exceeded, etc.)
            response.EnsureSuccessStatusCode();

            // STEP 6: Read OpenAI's response
            var responseBody = await response.Content.ReadAsStringAsync();

            // STEP 7: Parse JSON response
            var jsonDoc = JsonDocument.Parse(responseBody);

            // STEP 8: Extract AI's suggestion from OpenAI's response format
            // OpenAI nests the answer deep in JSON: choices[0].message.content
            return jsonDoc.RootElement
                .GetProperty("choices")[0]              // Get first choice (OpenAI can return multiple)
                .GetProperty("message")                 // Get the message object
                .GetProperty("content")                 // Get the actual text content
                .GetString() ?? string.Empty;           // Convert to string (or empty if null)
        }
        catch (Exception ex)
        {
            // If ANYTHING goes wrong (wrong API key, no internet, quota exceeded, etc.)
            // Print error and return empty string
            Console.WriteLine($"❌ OpenAI Error: {ex.Message}");
            return string.Empty;
        }
    }
}

