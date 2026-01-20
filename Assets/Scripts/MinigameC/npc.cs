using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class npc : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping;
    private bool playerInRange = false;
    private float dialogueCooldown = 0.5f; // Cooldown period after dialogue ends
    private float lastDialogueEndTime = -1f;

    // Static reference to track which NPC currently has active dialogue
    // This prevents state conflicts when multiple NPCs share the same dialogue panel
    private static npc currentActiveNPC = null;

    public bool CanInteract()
    {
        return currentActiveNPC == null;
    }

    void Update()
    {
        // Check for interaction input when player is in range
        if (playerInRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            // If this NPC has active dialogue, advance it
            if (currentActiveNPC == this)
            {
                NextLine();
            }
            // If no dialogue is active, try to start one
            else if (currentActiveNPC == null)
            {
                TryStartDialogue();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Automatically start dialogue when player collides with NPC
            TryStartDialogue();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if player is still in range
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Allow retrigger if no dialogue is active and cooldown passed
            if (currentActiveNPC == null && dialogueData != null)
            {
                float timeSinceEnd = Time.time - lastDialogueEndTime;
                if (timeSinceEnd > dialogueCooldown || lastDialogueEndTime < 0)
                {
                    TryStartDialogue();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Auto-close dialogue when player moves away from this NPC
            if (currentActiveNPC == this)
            {
                EndDialogueInternal();
            }
        }
    }

    void TryStartDialogue()
    {
        // Don't start if any NPC already has active dialogue
        if (currentActiveNPC != null)
        {
            return;
        }

        if (dialogueData != null)
        {
            // Check if enough time has passed since last dialogue ended, or if this is first trigger
            bool canTrigger = (Time.time - lastDialogueEndTime > dialogueCooldown) || (lastDialogueEndTime < 0);
            if (canTrigger)
            {
                StartDialogue();
            }
        }
    }

    public void Interact()
    {
        // If no dialogue data
        if (dialogueData == null)
        {
            return;
        }

        // If this NPC has active dialogue, advance it
        if (currentActiveNPC == this)
        {
            NextLine();
        }
        // If no dialogue is active, start one
        else if (currentActiveNPC == null)
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        currentActiveNPC = this;
        dialogueIndex = 0;
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;
        dialoguePanel.SetActive(true);
        // Don't pause the game so player can move and dialogue auto-closes when they walk away
        // PauseController.SetPause(true);
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            // Skip typing animation and show the full line
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            // If another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            // End dialogue
            EndDialogueInternal();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // This is the method called by the close button
    // It will end dialogue for whichever NPC is currently active
    public void EndDialogue()
    {
        // If there's an active NPC dialogue, end it
        if (currentActiveNPC != null)
        {
            currentActiveNPC.EndDialogueInternal();
        }
    }

    // Internal method that actually ends this NPC's dialogue
    private void EndDialogueInternal()
    {
        StopAllCoroutines();
        isTyping = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        lastDialogueEndTime = Time.time;

        // Clear the static reference
        if (currentActiveNPC == this)
        {
            currentActiveNPC = null;
        }
    }
}

