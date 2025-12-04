// ============================================================================
// SELF-HEALING EXTENSIONS - MAKES SELENIUM TESTS FIX THEMSELVES
// ============================================================================
// This file adds "magical" self-healing powers to Selenium WebDriver
// When an element is not found, it asks AI for help instead of failing
// ============================================================================

using OpenQA.Selenium;
using System.Text.Json;

namespace SeleniumSelfHealing.Utilities;

/// <summary>
/// Self-Healing Extensions: Adds automatic fixing capabilities to Selenium
/// These methods let Selenium ask AI for help when locators break
/// </summary>
public static class WebDriverExtensions
{
    // SHARED AI CLIENT: One AI client for all tests (saves memory and connections)
    private static LlmClient? _llmClient;

    // LOCK OBJECT: Prevents multiple tests from creating AI client at the same time
    private static readonly object _lock = new();

    /// <summary>
    /// INITIALIZE SELF-HEALING: Turn on the AI-powered self-healing system
    /// Call this once at the start of each test
    /// </summary>
    /// <param name="driver">The Selenium WebDriver (browser controller)</param>
    /// <param name="configPath">Path to settings file (default: appsettings.json)</param>
    public static void InitializeSelfHealing(this IWebDriver driver, string configPath = "appsettings.json")
    {
        // Check if AI client already exists (only create it once)
        if (_llmClient == null)
        {
            // Use lock to prevent multiple tests creating clients at same time
            lock (_lock)
            {
                // Double-check (another thread might have created it while we waited)
                if (_llmClient == null)
                {
                    // STEP 1: Read the settings file (appsettings.json)
                    var configJson = File.ReadAllText(configPath);

                    // STEP 2: Convert JSON text into a config object
                    var config = JsonSerializer.Deserialize<LlmConfig>(configJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // STEP 3: Create the AI client with these settings
                    _llmClient = new LlmClient(config ?? new LlmConfig());
                }
            }
        }
    }

    /// <summary>
    /// AI FIND ELEMENT: Smart element finder that uses AI if normal finding fails
    /// This is the CORE of self-healing - it tries normal way first, then asks AI
    /// </summary>
    /// <param name="driver">Browser controller</param>
    /// <param name="locator">How to find element (e.g., By.Id("username"))</param>
    /// <param name="elementDescription">Human description (e.g., "Username input box")</param>
    /// <param name="maxRetries">How many times to ask AI (default: 3)</param>
    /// <returns>The found element, or null if not found even with AI help</returns>
    public static async Task<IWebElement?> AiFindElement(
        this IWebDriver driver,
        By locator,
        string elementDescription,
        int maxRetries = 3)
    {
        // ========================================================================
        // STEP 1: Try finding element the normal way first
        // ========================================================================
        try
        {
            // Try to find element using the provided locator
            return driver.FindElement(locator);
            // If found, return it immediately - no AI needed! ✅
        }
        catch (NoSuchElementException)
        {
            // Element not found - time for self-healing! 🔧
            Console.WriteLine($"Element not found with {locator}");
            Console.WriteLine($"Attempting self-healing for: {elementDescription}");
        }

        // ========================================================================
        // STEP 2: Make sure AI client is ready
        // ========================================================================
        if (_llmClient == null)
        {
            // Initialize AI if not done yet
            driver.InitializeSelfHealing();
        }

        // ========================================================================
        // STEP 3: Get current webpage HTML (AI needs to see the page)
        // ========================================================================
        var pageSource = driver.PageSource;

        // ========================================================================
        // STEP 4: Try asking AI for help (up to maxRetries times)
        // ========================================================================
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Show what we're doing
                Console.WriteLine($"Asking AI for help (attempt {attempt + 1}/{maxRetries})...");

                // Ask AI: "Here's the HTML, the failed locator, and what I'm looking for - help!"
                var suggestedLocator = await _llmClient!.GetSuggestedLocator(
                    pageSource,           // The full webpage HTML
                    locator.ToString(),   // The locator that failed
                    elementDescription    // Human description of what we want
                );

                // Show what AI suggested
                Console.WriteLine($"AI suggested: {suggestedLocator}");

                // ====================================================================
                // STEP 5: Try using AI's suggested locator
                // ====================================================================

                // AI might suggest XPath (starts with //) or CSS selector
                IWebElement? element = null;

                // Try to interpret AI's suggestion
                if (suggestedLocator.StartsWith("//") || suggestedLocator.StartsWith("(//"))
                {
                    // This looks like XPath - use XPath locator
                    element = driver.FindElement(By.XPath(suggestedLocator));
                }
                else if (suggestedLocator.Contains("#") || suggestedLocator.Contains(".") || suggestedLocator.Contains("["))
                {
                    // This looks like CSS selector - use CSS locator
                    element = driver.FindElement(By.CssSelector(suggestedLocator));
                }
                else
                {
                    // Not sure what format - try CSS first, then XPath
                    try
                    {
                        element = driver.FindElement(By.CssSelector(suggestedLocator));
                    }
                    catch
                    {
                        element = driver.FindElement(By.XPath(suggestedLocator));
                    }
                }

                // ====================================================================
                // STEP 6: Success! AI fixed it! 🎉
                // ====================================================================
                if (element != null)
                {
                    Console.WriteLine($"Self-healing successful! New locator works: {suggestedLocator}");
                    return element;
                }
            }
            catch (Exception ex)
            {
                // This attempt failed - show error and try again
                Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
            }
        }

        // ========================================================================
        // STEP 7: All attempts failed - give up 😞
        // ========================================================================
        Console.WriteLine($"Self-healing failed after all attempts");
        throw new NoSuchElementException($"Could not find element: {elementDescription}");
    }

    /// <summary>
    /// CLICK: Smart click that uses self-healing if element not found
    /// Use this instead of driver.FindElement().Click()
    /// </summary>
    public static async Task Click(this IWebDriver driver, By locator, string elementDescription)
    {
        // Find element using AI-powered finder
        var element = await driver.AiFindElement(locator, elementDescription);

        // Click it
        element?.Click();

        // Show success message
        Console.WriteLine($"Clicked: {elementDescription}");
    }

    /// <summary>
    /// SEND KEYS: Smart text entry that uses self-healing
    /// Use this instead of driver.FindElement().SendKeys()
    /// </summary>
    public static async Task SendKeys(this IWebDriver driver, By locator, string elementDescription, string text)
    {
        // Find element using AI-powered finder
        var element = await driver.AiFindElement(locator, elementDescription);

        // Clear any existing text
        element?.Clear();

        // Type the new text
        element?.SendKeys(text);

        // Show success message
        Console.WriteLine($"Entered text in: {elementDescription}");
    }

    /// <summary>
    /// GET TEXT: Smart text reading that uses self-healing
    /// Use this instead of driver.FindElement().Text
    /// </summary>
    public static async Task<string> GetText(this IWebDriver driver, By locator, string elementDescription)
    {
        // Find element using AI-powered finder
        var element = await driver.AiFindElement(locator, elementDescription);

        // Get its text
        var text = element?.Text ?? string.Empty;

        // Show what we found
        Console.WriteLine($"Got text from {elementDescription}: {text}");

        return text;
    }

    /// <summary>
    /// IS ELEMENT VISIBLE: Check if element exists and is visible (with self-healing)
    /// </summary>
    public static async Task<bool> IsElementVisible(this IWebDriver driver, By locator, string elementDescription)
    {
        try
        {
            // Find element using AI-powered finder
            var element = await driver.AiFindElement(locator, elementDescription);

            // Check if it's displayed
            var isVisible = element?.Displayed ?? false;

            // Show result
            Console.WriteLine($"✅ {elementDescription} visible: {isVisible}");

            return isVisible;
        }
        catch
        {
            // Element not found even with AI help
            Console.WriteLine($"❌ {elementDescription} not found");
            return false;
        }
    }
}

