// ============================================================================
// AI CONFIGURATION - SETTINGS FOR THE AI SERVICE
// ============================================================================
// This file stores all the settings needed to connect to AI (Ollama or OpenAI)
// Think of this as the "phone book" with AI's contact information
// ============================================================================

namespace SeleniumSelfHealing.Utilities;

/// <summary>
/// AI Configuration: Stores all settings for connecting to AI services
/// These settings come from appsettings.json file
/// </summary>
public class LlmConfig
{
    /// <summary>
    /// PROVIDER: Which AI service to use
    /// Options: "Local" (Ollama on your computer) or "OpenAI" (Cloud AI)
    /// Default: "Local" (free)
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// API KEY: Your password to use OpenAI
    /// Only needed if Provider = "OpenAI"
    /// Get from: https://platform.openai.com/api-keys
    /// Default: Empty (not needed for Ollama)
    /// Example: "sk-proj-abc123xyz..."
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// BASE URL: Where to send AI requests
    /// For Ollama (Local): "http://localhost:11434" (your computer)
    /// For OpenAI: "https://api.openai.com/v1" (OpenAI servers)
    /// Default: "http://localhost:11434" (Ollama)
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// MODEL: Which AI brain to use
    /// For Ollama: "qwen3-coder:480b-cloud" (good for code) or "codellama:7b"
    /// For OpenAI: "gpt-4o" (best), "gpt-4o-mini" (cheaper), "gpt-3.5-turbo" (cheapest)
    /// Default: "qwen3-coder:480b-cloud" (works with Ollama)
    /// </summary>
    public string Model { get; set; } = "qwen3-coder:480b-cloud";

    /// <summary>
    /// TEMPERATURE: How creative should AI be?
    /// 0.0 = Very consistent, always gives same answer (best for tests)
    /// 1.0 = More creative, varies answers
    /// 2.0 = Very creative, unpredictable
    /// Default: 0.1 (very consistent - we want same locators every time)
    /// </summary>
    public double Temperature { get; set; } = 0.1;

    /// <summary>
    /// MAX TOKENS: Maximum length of AI's response
    /// 1 token ≈ 4 characters
    /// 1000 tokens ≈ 750 words (plenty for a locator suggestion)
    /// Default: 1000 (enough for detailed selector + explanation)
    /// </summary>
    public int MaxTokens { get; set; } = 1000;
}

