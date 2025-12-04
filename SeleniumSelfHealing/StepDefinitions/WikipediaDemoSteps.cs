using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using SeleniumSelfHealing.Pages;

namespace SeleniumSelfHealing.StepDefinitions;

[Binding]
public class WikipediaDemoSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IWebDriver _driver;
    private readonly WikipediaPage _wikipediaPage;

    public WikipediaDemoSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = (IWebDriver)scenarioContext["WebDriver"];
        _wikipediaPage = new WikipediaPage(_driver);
    }

    [Given(@"I navigate to ""(.*)""")]
    public async Task GivenINavigateTo(string url)
    {
        await _wikipediaPage.NavigateTo(url);
    }

    [When(@"I search for ""(.*)""")]
    public async Task WhenISearchFor(string searchTerm)
    {
        await _wikipediaPage.SearchFor(searchTerm);
    }

    [Then(@"the page should contain ""(.*)""")]
    public void ThenThePageShouldContain(string expectedText)
    {
        var pageSource = _driver.PageSource;
        Assert.That(pageSource, Does.Contain(expectedText), 
            $"Page should contain '{expectedText}'");
        Console.WriteLine($"Page contains: {expectedText}");
    }
}
