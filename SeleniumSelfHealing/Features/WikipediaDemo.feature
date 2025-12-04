Feature: Wikipedia Search Demo
    As a user
    I want to search on Wikipedia
    So that I can demonstrate AI self-healing

@demo @working
Scenario: Search for Selenium on Wikipedia
    Given I navigate to "https://www.wikipedia.org"
    When I search for "Selenium"
    Then the page should contain "Selenium"
