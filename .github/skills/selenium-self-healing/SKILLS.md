<!-- .github/skills/selenium-self-healing/SKILLS.md -->
---
name: selenium-self-healing
description: Copilot skill for generating, maintaining, and repairing self-healing Selenium tests with Reqnroll BDD features, step definitions, page objects, and AI-powered locator recovery.
---

# Selenium Self-Healing Automation Skills

This document defines **all skills, scripts, templates, golden examples, and behavioral rules** for Copilot when working in the **SeleniumSelfHealing.Reqnroll** repository.

---

## Purpose

Enable Copilot to:
- Generate robust Selenium UI tests
- Use AI-powered self-healing locator strategies
- Extend and maintain Reqnroll (SpecFlow-style) BDD tests
- Follow repository-specific architecture and conventions
- Prefer recovery and diagnostics over brittle failures
- Follow patterns shown in **golden examples**

---

## Repository Context

This repository demonstrates **self-healing Selenium tests** with AI via:
- .NET / C#
- Selenium WebDriver
- Reqnroll (BDD / Gherkin)
- AI-based locator recovery (Ollama / OpenAI via `LlmClient`)

### Key Areas
- `Features/` → Gherkin `.feature` files  
- `StepDefinitions/` → C# step implementations  
- `Pages/` → Page Objects (descriptive, AI-friendly)  
- `Utilities/` → `WebDriverExtensions.cs`, `LlmClient.cs`, `LlmConfig.cs`  
- `Support/` → Setup / teardown hooks  

### Golden Examples
- `Features/Examples/WikipediaSearch.feature`  
- `StepDefinitions/Examples/WikipediaSearchSteps.cs`  
- `Pages/Examples/WikipediaPage.cs`  

---

## Hard Rules for Copilot

### Must
- Use **self-healing WebDriver extensions**
- Prefer **element descriptions** over raw locators
- Generate step definitions matching Gherkin steps
- Log all healing attempts
- Align generated code with **golden examples**

### Must Not
- Bypass self-healing logic
- Hardcode brittle XPath or CSS unless last resort
- Use `Thread.Sleep`
- Silence locator failures

---

## Canonical Step Definition Pattern

For feature steps like:

```gherkin
Given I navigate to "https://www.wikipedia.org"
When I search for "Selenium"
Then the page should contain "Selenium"
```

Copilot should generate:

```csharp
[Given(@"I navigate to ""(.*)""")]
public async Task GivenINavigateTo(string url)
{
    _driver.Navigate().GoToUrl(url);
}

[When(@"I enter ""(.*)"" into the ""(.*)""")]
public async Task WhenIEnterTextIntoElement(string text, string elementDescription)
{
    await _driver.SendKeys(
        By.CssSelector(""),
        elementDescription,
        text
    );
}

[When(@"I click the ""(.*)""")]
public async Task WhenIClickElement(string elementDescription)
{
    await _driver.Click(
        By.CssSelector(""),
        elementDescription
    );
}

[Then(@"the page should contain ""(.*)""")]
public void ThenThePageShouldContain(string expectedText)
{
    Assert.That(_driver.PageSource.Contains(expectedText));
}
```

---

## Self-Healing Locator Strategy (Priority)

1. **Primary**: AI-interpreted element description
2. **Fallback 1**: Semantic locators (aria-label, role, visible text)
3. **Fallback 2**: DOM structure similarity analysis
4. **Fallback 3**: Ranked XPath strategies (last resort)
5. **Logging**: Capture all attempts and HTML snapshot on failure

Always use `WebDriverExtensions` for retries and logging.

---

## Feature File Template

```gherkin
Feature: <Feature Name>

  Scenario: <Scenario Description>
    Given I navigate to "<URL>"
    When I enter "<text>" into the "<element description>"
    And I click the "<element description>"
    Then I should see "<expected text>"
```

---

## Page Object Template

```csharp
public class WikipediaPage
{
    public string SearchBox => "Wikipedia search box";
    public string SearchButton => "search button";
    public string ResultsArea => "search results area";
}
```

---

## Golden Example

### Feature file:
```gherkin
Feature: Wikipedia Search

Scenario: Search for Selenium
  Given I navigate to "https://www.wikipedia.org"
  When I enter "Selenium" into the "Wikipedia search box"
  And I click the "search button"
  Then I should see "Selenium"
```

### Step Definition:
```csharp
[When(@"I enter ""(.*)"" into the ""(.*)""")]
public async Task WhenIEnterTextIntoElement(string text, string elementDescription)
{
    await _driver.SendKeys(
        By.CssSelector(""),
        elementDescription,
        text
    );
}
```

### Page Object:
```csharp
public string SearchBox => "Wikipedia search box";
public string SearchButton => "search button";
```
