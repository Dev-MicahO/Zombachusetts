using UnityEngine;
// Enum for the different states of the battle, such as player's turn, enemy's turn, and the end states (won, lost, fled).
public enum BattleState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    BUSY,
    WON,
    LOST,
    FLED
}