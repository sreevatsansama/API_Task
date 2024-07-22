Feature: Delete a Label on a Card
  As a Trello user
  I want to delete a label on a card
  So that they can be re-categorised

  Scenario: Remove a label from a card
    Given I have a Trello account 2
    And I have a board named "Board A" with a card named "Card 1" 2
    And "Card 1" has a label named "Urgent"
    When I delete the label "Urgent" from "Card 1"
    Then "Card 1" should no longer have the label "Urgent"
