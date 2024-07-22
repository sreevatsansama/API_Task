Feature: Copy Cards

  Scenario: Copy a card from one board to another
    Given I have a source board with id "sourceBoardId"
    And I have a destination board with id "destinationBoardId"
    And I have a card with id "cardId" on the source board
    When I copy the card to the destination board
    Then the card should exist on the destination board
