# Selenium Self-Healing Tests with AI

A simple BDD test framework that uses **AI to automatically fix broken element locators**.

![selenium-self-healing](https://github.com/user-attachments/assets/f4fe21a2-31db-4a0b-b4aa-961953a0c9ae)

## What Is This?

One working test that demonstrates how AI can fix broken Selenium tests automatically. When an element locator fails, AI analyzes the page and suggests a new one!

## ✨ Features

- ✅ **One Working Test** - Wikipedia search demo
- ✅ **AI Self-Healing** - Automatically fixes broken locators
- ✅ **BDD with Reqnroll** - Tests written in plain English
- ✅ **Free AI (Ollama)** - Runs on your computer
- ✅ **Fully Commented Code** - Easy to understand

## 🚀 Quick Start

### What You Need

1. **.NET 9 SDK** - https://dotnet.microsoft.com/download/dotnet/9.0
2. **Google Chrome** - https://www.google.com/chrome/
3. **Ollama** - https://ollama.ai

### Installation (5 Minutes)

```bash
# 1. Install Ollama AI model
ollama pull qwen3-coder:480b-cloud

# 2. Navigate to project
cd SeleniumSelfHealing.Reqnroll.Net9

# 3. Build
dotnet restore
dotnet build

# 4. Run the test
dotnet test --logger "console;verbosity=detailed"
```

**That's it!** Watch AI fix the broken locator automatically! ✨

## 📁 What's Inside

```
SeleniumSelfHealing.Reqnroll.Net9/
│
├── Features/
│   └── WikipediaDemo.feature        ← Test in plain English
│
├── StepDefinitions/
│   └── WikipediaDemoSteps.cs        ← Step implementations
│
├── Pages/
│   └── WikipediaPage.cs             ← Page with WRONG locator (on purpose!)
│
├── Utilities/
│   ├── LlmClient.cs                 ← AI communication (fully commented)
│   ├── LlmConfig.cs                 ← AI settings (fully commented)
│   └── WebDriverExtensions.cs      ← Self-healing magic (fully commented)
│
└── Support/
    └── Hooks.cs                     ← Test setup/teardown
```

## 🎯 The Test

**Features/WikipediaDemo.feature:**

```gherkin
Feature: Wikipedia Search Demo

@demo @working
Scenario: Search for Selenium on Wikipedia
    Given I navigate to "https://www.wikipedia.org"
    When I search for "Selenium"
    Then the page should contain "Selenium"
```

Simple! Written in plain English!

## 🤖 How Self-Healing Works

### The Intentional Bug

The test uses a **WRONG locator on purpose**:

```csharp
// Pages/WikipediaPage.cs
private readonly By _searchBox = By.Id("searchBox");  // WRONG!
// Real ID is: "searchInput"
```

### What Happens

```
1. Test tries By.Id("searchBox")
   ❌ Element not found!

2. AI analyzes the page HTML
   🤖 "I see an input with id='searchInput'"

3. AI suggests new locator
   💡 "Try: //input[@id='searchInput']"

4. Test tries AI's suggestion
   ✅ Found it! Test continues!
```

### Test Output

```
▶️ Step: When I search for "Selenium"
Element not found with By.Id: searchBox
Attempting self-healing for: Wikipedia search box
Asking AI for help (attempt 1/3)...
AI suggested: //input[@id='searchInput']
Self-healing successful! New locator works!
✅ Entered text in: Wikipedia search box
✅ Test Passed!
```

**This proves AI self-healing works!** 🎉

## 🎓 Understanding the Code

All AI code is **fully commented** for beginners:

```csharp
// STEP 1: Try finding element the normal way first
try
{
    return driver.FindElement(locator);  // Try original locator
}
catch (NoSuchElementException)
{
    // Element not found - time for self-healing!
}

// STEP 2: Ask AI for help
var suggestedLocator = await aiClient.GetSuggestedLocator(
    pageHTML,           // The webpage code
    failedLocator,      // What didn't work
    "Search box"        // What we're looking for
);

// STEP 3: Try AI's suggestion
var element = driver.FindElement(By.XPath(suggestedLocator));
// ✅ It works!
```

## ⚙️ Configuration

**appsettings.json** - AI settings:

```json
{
  "Provider": "Local",
  "BaseUrl": "http://localhost:11434",
  "Model": "qwen3-coder:480b-cloud",
  "Temperature": 0.1,
  "MaxTokens": 1000
}
```

**Use OpenAI instead?** Change to:

```json
{
  "Provider": "OpenAI",
  "ApiKey": "sk-your-api-key-here",
  "BaseUrl": "https://api.openai.com/v1",
  "Model": "gpt-4o"
}
```

## 📊 Expected Results

```
Starting test execution...

Passed!  - Failed: 0, Passed: 1, Total: 1

✅ 1 test runs
✅ AI self-healing activates
✅ Test passes
```

## 🔧 Building Your Own Tests

### Step 1: Create Feature File

```gherkin
Feature: My Test
    
Scenario: Test my website
    Given I navigate to "https://mysite.com"
    When I click the login button
    Then I should see the login form
```

### Step 2: Implement Steps

```csharp
[Given(@"I navigate to ""(.*)""")]
public async Task GivenINavigateTo(string url)
{
    _driver.Navigate().GoToUrl(url);
}

[When(@"I click the login button")]
public async Task WhenIClickLogin()
{
    // Uses self-healing automatically!
    await _driver.Click(By.Id("login"), "Login button");
}
```

### Step 3: Run It!

```bash
dotnet test
```

**AI will fix broken locators automatically!** ✨

## 🐛 Troubleshooting

### "Connection refused to localhost:11434"

Ollama isn't running. Start it:

```bash
# Mac/Linux
ollama serve

# Windows - Click Ollama icon in system tray
```

### "dotnet: command not found"

Install .NET 9 SDK from https://dotnet.microsoft.com/download/dotnet/9.0

### Build errors

```bash
dotnet clean
dotnet restore
dotnet build
```

## 📚 Commands

```bash
# Run test
dotnet test

# See detailed output (watch AI heal!)
dotnet test --logger "console;verbosity=detailed"

# Clean and rebuild
dotnet clean && dotnet restore && dotnet build
```

## 💡 Key Points

1. **One Working Test** - Wikipedia search demo
2. **Intentional Wrong Locator** - Proves self-healing works
3. **Free AI** - Uses Ollama (or paid OpenAI)
4. **Fully Commented** - 430+ lines explaining everything
5. **Ready to Customize** - Build your own tests easily

## 🎯 What This Demonstrates
 
✅ AI automatically fixing broken locators  
✅ No manual intervention needed  
✅ Tests that heal themselves  

## 🚀 Next Steps

1. **Run the demo** - See AI self-healing work
2. **Read the comments** - Understand how it works
3. **Create your own test** - Use as template
4. **Customize** - Add your own pages and scenarios

## 📖 Documentation

- **This file** - Simple overview
- **Code comments** - Every line explained

## ⚡ Quick Reference

```bash
# Everything in 3 commands:
ollama pull qwen3-coder:480b-cloud
dotnet restore && dotnet build
dotnet test --logger "console;verbosity=detailed"
```

---

**Ready to see AI fix your tests automatically? Run it now! 🚀**

For detailed explanations, check the commented code in `Utilities/` folder!

## Advanced Self-Healing Concepts

The current implementation uses the first locator the AI suggests. That works for one test, but in bigger suites you need to be more careful — a locator that works isn't always the right one. Some ideas to take it further:

### Don't Trust the First Match
- Don't just accept the first element that makes the test pass — it might be the wrong one
- Ask the AI for a few ranked options instead of one
- Pick the best one by checking:
  - Does it match only one element?
  - Does it use something stable like an `id` instead of a shaky XPath?
  - Does it actually fit what we're looking for?

### Use a Confidence Score
- High confidence: fix it automatically
- Medium confidence: fix it but flag it for a human to check
- Low confidence: don't fix it, just show the options and let a human decide

### Watch It at Scale
- In big suites you can't check every fix by hand
- Keep an eye on how many fixes get flagged
- Watch for the same step getting fixed over and over — that usually means a weak locator

### Make It a Feedback Loop
- If a step keeps getting healed, fix the locator for real instead of healing it every run
- Use that history to find the weak spots in your tests
- Self-healing should help you find problems, not become a crutch
