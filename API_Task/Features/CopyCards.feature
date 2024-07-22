Feature: Copy Cards Between Boards
  As a Trello user
  I want to copy cards from one board to another
  So that I don’t have to duplicate effort

  Scenario: Copy a card from Board A to Board B
    Given I have a Trello account 1
    And I have a board named "Board A" with a card named "Card 1" 1
    And I have a board named "Board B"
    When I copy the card "Card 1" from "Board A" to "Board B"
    Then "Card 1" should exist on "Board B"
    And "Card 1" should still exist on "Board A"
