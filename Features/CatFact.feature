Feature: Public Cat Fact API Testing
  As an API tester
  I want to fetch data from a public API
  So that I can verify external read operations

  Scenario: Get a random cat fact
    When I send a GET request to "https://catfact.ninja/fact"
    Then the response status code should be 200
    And the response body should contain a cat fact