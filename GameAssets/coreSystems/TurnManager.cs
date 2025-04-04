using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<Character> characters = new List<Character>();
    private int currentCharacterIndex = 0;
    private bool isPlayerTurn = true;
    public DeathController deathController;
    public BracerManager bracerManager;
    public CombatManager combatManager;
    public AIManager aiManager;

    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }

    void Start()
    {
        GameObject playerObject = GameObject.Find("PlayerCharacter");

        Character playerCharacter = playerObject.GetComponent<Character>(); // Saves the playercharacter roundstart to be referenced elsewhere

        if (playerObject != null)
        {
            if (playerCharacter != null)
            {
                AddCharacter(playerCharacter);
            }
            else
            {
                Debug.LogError("PlayerCharacter does not have a Character component.");
            }
        }
        else
        {
            Debug.LogError("PlayerCharacter not found in the scene.");
        }

        if (deathController != null)
        {
            deathController.characters = characters;
        }

        // Check if BracerManager and CombatManager are assigned
        if (bracerManager == null)
        {
            Debug.LogError("BracerManager is not assigned in TurnManager.");
        }

        if (combatManager == null)
        {
            Debug.LogError("CombatManager is not assigned in TurnManager.");
        }
    }

    void Update()
    {
        if (isPlayerTurn)
        {
            HandlePlayerInput();
        }
        else
        {
            HandleAI();
        }
    }

    void HandlePlayerInput()
    {
        if (characters.Count == 0)
        {
            Debug.LogError("No characters available to control.");
            return;
        }

        // Ensure the current index is valid
        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Count)
        {
            Debug.LogError("Invalid current character index.");
            return;
        }

        Character currentCharacter = characters[currentCharacterIndex];
        
        if (!currentCharacter.aiming)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) // Change bracer segment 1
            {
                if (bracerManager != null)
                {
                    bracerManager.BraceChange1();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) // Change bracer segment 2
            {
                if (bracerManager != null)
                {
                    bracerManager.BraceChange2();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) // Change bracer segment 3
            {
                if (bracerManager != null)
                {
                    bracerManager.BraceChange3();
                }
            }
        }
        
        if (currentCharacter.aiming)
        {
            combatManager.CombatPrompt();
            return;
        }
        
        if (!currentCharacter.aiming)
        {
            if (Input.GetKeyDown(KeyCode.W)) // Move Up (North)
            {
                Vector2Int targetPosition = currentCharacter.currentTilePosition + Vector2Int.up;
                if (currentCharacter.CanMoveTo(targetPosition))
                {
                    currentCharacter.Move(targetPosition);
                }
            }

            if (Input.GetKeyDown(KeyCode.S)) // Move Down (South)
            {
                Vector2Int targetPosition = currentCharacter.currentTilePosition + Vector2Int.down;
                if (currentCharacter.CanMoveTo(targetPosition))
                {
                    currentCharacter.Move(targetPosition);
                }
            }

            if (Input.GetKeyDown(KeyCode.A)) // Move Left (West)
            {
                Vector2Int targetPosition = currentCharacter.currentTilePosition + Vector2Int.left;
                if (currentCharacter.CanMoveTo(targetPosition))
                {
                    currentCharacter.Move(targetPosition);
                }
            }

            if (Input.GetKeyDown(KeyCode.D)) // Move Right (East)
            {
                Vector2Int targetPosition = currentCharacter.currentTilePosition + Vector2Int.right;
                if (currentCharacter.CanMoveTo(targetPosition))
                {
                    currentCharacter.Move(targetPosition);
                }
            }

            if (Input.GetKeyDown(KeyCode.Return)) // Skip your turn
            {
                EndTurn();
            }

            if (Input.GetKeyDown(KeyCode.Space)) // Use your Bracer in its current mode
            {
                if (bracerManager != null)
                {
                    bracerManager.UpdateBracer(); // Updates the current bracer mode
                    bracerManager.UseBracer(); // Activates the current bracer mode
                    bracerManager.BracerEnergy -= bracerManager.drainAmount; // Drains the bracer by the amount specified by the bracer mode
                    if (bracerManager.weaponMode && combatManager != null) // If a weapon is equipped, move to the combat prompt
                    {
                        combatManager.StartCombat();  // Start combat phase
                        currentCharacter.aiming = true; // Stop the player from moving while aiming weapons
                        Debug.Log("Entering combat prompt.");
                    }
                    else
                    {
                        EndTurn();
                    }
                }
            }
        }
        
        if (currentCharacter.movementRange <= 0) // If all moves have been spent, end the turn.
        {
            EndTurn();
        }
    }

    void HandleAI()
    {
        aiManager.AITurn(currentCharacter);
    }

    public void EndTurn()
    {
        //Checks if current health is above max health and sets health to max health if so
        if (currentCharacter.currentHealth > currentCharacter.baseHealth)
        {
            currentCharacter.currentHealth = currentCharacter.baseHealth;
        }
        //Checks if health is equal to or below zero and destroys the current character if so.
        if (currentCharacter.currentHealth <= 0)
        {
            destroy(currentCharacter.gameObject);
        }
        
        Character currentCharacter = characters[currentCharacterIndex];
        currentCharacter.ResetMovementRange();

        Debug.Log("Next turn!");
        
        currentCharacter.aiming = false;

        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        
        isPlayerTurn = !isPlayerTurn; // Switch turns
    }
}
