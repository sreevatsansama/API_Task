Feature: Delete Label

  Scenario: Delete a label on a card
    Given I have a board with id "boardId" and a card with id "cardId"
    And the card has a label with id "labelId"
    When I delete the label from the card
    Then the label should no longer exist on the card
