Feature: RestfulBooker API CRUD Operations
  As an API tester
  I want to perform CRUD operations on the booking service
  So that I can verify booking functionality works as expected

  Scenario: Create a new booking (Create)
    Given I have a new booking request payload with the following details:
      | firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
      | Olena     | Uska     | 150        | true        | 2026-11-01 | 2026-11-10 | Breakfast       |
    When I send a POST request to "/booking"
    Then the response status code should be 200
    And the response should contain a valid booking ID
    And the created booking details should match the request

  Scenario: Get booking by ID (Read)
    Given an existing booking exists with ID 1
    When I send a GET request to "/booking/1"
    Then the response status code should be 200
    And the response should contain booking details for "Sally" or valid firstname

  Scenario: Generate Auth Token and Update booking details (Update)
    Given I generate an authentication token with username "admin" and password "password123"
    And I have created a booking to update with firstname "Olena" and lastname "Uska"
    When I send a PUT request to update the booking with new firstname "Olena_Updated" and totalprice 200
    Then the response status code should be 200
    And the updated booking firstname should be "Olena_Updated"

  Scenario: Delete a booking (Delete)
    Given I generate an authentication token with username "admin" and password "password123"
    And I have created a booking to delete
    When I send a DELETE request for the created booking
    Then the response status code should be 201