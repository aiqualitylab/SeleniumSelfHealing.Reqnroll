using OpenQA.Selenium;
using SeleniumSelfHealing.Utilities;

namespace SeleniumSelfHealing.Pages;

public class WikipediaPage
{
    private readonly IWebDriver _driver;

    public WikipediaPage(IWebDriver driver)
    {
        _driver = driver;
    }

    // INTENTIONALLY WRONG LOCATOR - to demonstrate AI self-healing!
    // The real ID is "searchInput", but we use "searchBox" to trigger self-healing
    private readonly By _searchBox = By.Id("searchBox");  // WRONG---- Will trigger AI healing
    
    // Alternative locators for different Wikipedia pages
    private readonly By _searchButton = By.CssSelector("button[type='submit']");

    public async Task NavigateTo(string url)
    {
        _driver.Navigate().GoToUrl(url);
        await Task.Delay(2000); // Wait for page load
        Console.WriteLine($"Navigated to: {url}");
    }

    public async Task SearchFor(string searchTerm)
    {
        Console.WriteLine($"Attempting to search for: {searchTerm}");
        
        // This will FAIL with the wrong locator, triggering AI self-healing
        await _driver.SendKeys(_searchBox, "Wikipedia search box", searchTerm);
        
        await Task.Delay(500);
        
        // Submit the search
        await _driver.Click(_searchButton, "Search button");
        
        await Task.Delay(3000); // Wait for results
        Console.WriteLine($"Search completed for: {searchTerm}");
    }
}
