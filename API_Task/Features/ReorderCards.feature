Feature: Reorder Cards

  Scenario: Reorder cards on a list
    Given I have a board with id "boardId" and a list with id "listId"
    And I have cards with ids "cardId1" and "cardId2" on the list
    When I move "cardId1" before "cardId2"
    Then "cardId1" should be positioned before "cardId2" on the list
