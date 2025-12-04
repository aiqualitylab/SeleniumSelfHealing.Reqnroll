using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using SeleniumSelfHealing.Utilities;

namespace SeleniumSelfHealing.Support;

[Binding]
public sealed class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private IWebDriver? _driver;

    public Hooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        Console.WriteLine("========================================");
        Console.WriteLine($"Starting Scenario: {_scenarioContext.ScenarioInfo.Title}");
        Console.WriteLine("========================================");

        // Setup Chrome driver
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        
        // Uncomment for headless mode
        // options.AddArgument("--headless");
        // options.AddArgument("--disable-gpu");
        
        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        
        // Initialize self-healing
        _driver.InitializeSelfHealing();
        
        // Store driver in ScenarioContext for step definitions
        _scenarioContext["WebDriver"] = _driver;
        
        Console.WriteLine("WebDriver initialized with self-healing");
    }

    [AfterScenario]
    public void AfterScenario()
    {
        Console.WriteLine("========================================");
        
        if (_scenarioContext.TestError != null)
        {
            Console.WriteLine($"Scenario Failed: {_scenarioContext.ScenarioInfo.Title}");
            Console.WriteLine($"Error: {_scenarioContext.TestError.Message}");
            
            // Take screenshot on failure
            if (_driver != null)
            {
                try
                {
                    var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                    var filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    screenshot.SaveAsFile(filename);
                    Console.WriteLine($"Screenshot saved: {filename}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not take screenshot: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine($"Scenario Passed: {_scenarioContext.ScenarioInfo.Title}");
        }

        // Cleanup
        _driver?.Quit();
        _driver?.Dispose();
        
        Console.WriteLine("WebDriver closed");
        Console.WriteLine("========================================");
    }

    [BeforeStep]
    public void BeforeStep()
    {
        Console.WriteLine($"Step: {_scenarioContext.StepContext.StepInfo.Text}");
    }

    [AfterStep]
    public void AfterStep()
    {
        if (_scenarioContext.TestError != null)
        {
            Console.WriteLine($"❌ Step Failed: {_scenarioContext.StepContext.StepInfo.Text}");
        }
        else
        {
            Console.WriteLine($"✅ Step Completed");
        }
    }
}
