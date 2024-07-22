Feature: Reorder Cards on a List
  As a Trello user
  I want to reorder cards on a list
  So that they can be prioritised

  Scenario: Move a card to a different position in the list
    Given I have a Trello account
    And I have a board named "Board A" with a list named "To Do"
    And "To Do" contains cards "Card 1", "Card 2", and "Card 3" in that order
    When I move "Card 3" to the position after "Card 1" in the "To Do" list
    Then the "To Do" list should contain "Card 1", "Card 3", "Card 2" in that order
