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

    [Tooltip("Optional: assign to override. If unset, uses Image on dialoguePanel.")]
    public Image dialogueBackground;

    [Tooltip("Optional: show when this NPC is the current step objective (e.g. icon or '!').")]
    public GameObject objectiveIndicator;

    private static readonly Vector2 DialoguePanelSize = new Vector2(720f, 240f);

    private int dialogueIndex;
    private bool isTyping;
    private string[] activeDialogueLines;
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
        if (dialogueData == null)
        {
            return;
        }

        currentActiveNPC = this;
        dialogueIndex = 0;
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;
        ApplyDialoguePanelStyle();
        dialoguePanel.SetActive(true);
        PhaseCAnimationManager.TriggerDialogueStart();
        // Don't pause the game so player can move and dialogue auto-closes when they walk away
        // PauseController.SetPause(true);
        activeDialogueLines = ResolveDialogueLines();
        if (activeDialogueLines == null || activeDialogueLines.Length == 0)
        {
            EndDialogueInternal();
            return;
        }
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            // Skip typing animation and show the full line
            StopAllCoroutines();
            dialogueText.SetText(activeDialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < activeDialogueLines.Length)
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
        foreach (char letter in activeDialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (ShouldAutoProgressLine(dialogueIndex))
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
        PhaseCAnimationManager.TriggerDialogueEnd();
        dialoguePanel.SetActive(false);
        lastDialogueEndTime = Time.time;

        // Clear the static reference
        if (currentActiveNPC == this)
        {
            currentActiveNPC = null;
        }

        if (dialogueData != null)
        {
            PhaseCAssemblyController controller = PhaseCAssemblyController.Instance;
            if (controller != null)
            {
                controller.NotifyDialogueClosed(dialogueData.npcName);
            }
        }
    }

    public void RefreshDialogueLines()
    {
        if (dialogueData == null)
        {
            return;
        }

        activeDialogueLines = ResolveDialogueLines();
    }

    public void SetIsCurrentObjective(bool isCurrent)
    {
        if (objectiveIndicator != null)
            objectiveIndicator.SetActive(isCurrent);
    }

    private string[] ResolveDialogueLines()
    {
        if (dialogueData == null)
        {
            return null;
        }

        PhaseCAssemblyController controller = PhaseCAssemblyController.Instance;
        if (controller != null)
        {
            string[] phaseCLines = controller.GetDialogueLinesForNpc(dialogueData.npcName);
            if (phaseCLines != null && phaseCLines.Length > 0)
            {
                return phaseCLines;
            }
        }

        return dialogueData.dialogueLines;
    }

    void ApplyDialoguePanelStyle()
    {
        if (dialoguePanel == null) return;

        // Panel root: ensure it has a visible size (scene may have 0.8x0.8 = invisible)
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            Vector2 size = panelRect.sizeDelta;
            if (size.x < 100f || size.y < 100f)
                panelRect.sizeDelta = DialoguePanelSize;
        }

        // Ensure there is an Image on the panel for the background (create if missing)
        Image panelImage = dialoguePanel.GetComponent<Image>();
        if (panelImage == null)
            panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.color = PhaseCUITheme.PanelBg;
        panelImage.enabled = true;
        panelImage.raycastTarget = true;
        // Unity UI Image with no sprite still draws solid color; use a white sprite for reliability
        if (panelImage.sprite == null)
            panelImage.sprite = CreateSolidSprite();

        // Optional override: use assigned background instead
        if (dialogueBackground != null)
        {
            dialogueBackground.color = PhaseCUITheme.PanelBg;
            dialogueBackground.enabled = true;
            dialogueBackground.raycastTarget = true;
            if (dialogueBackground.sprite == null)
                dialogueBackground.sprite = CreateSolidSprite();
        }

        if (dialogueText != null)
            dialogueText.color = PhaseCUITheme.TextBody;
        if (nameText != null)
            nameText.color = PhaseCUITheme.AccentCyan;
    }

    private static Texture2D _solidWhiteTexture;
    private static Sprite _solidWhiteSprite;

    private static Sprite CreateSolidSprite()
    {
        if (_solidWhiteSprite != null) return _solidWhiteSprite;
        _solidWhiteTexture = new Texture2D(1, 1);
        _solidWhiteTexture.SetPixel(0, 0, Color.white);
        _solidWhiteTexture.Apply();
        _solidWhiteSprite = Sprite.Create(_solidWhiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        return _solidWhiteSprite;
    }

    private bool ShouldAutoProgressLine(int index)
    {
        if (dialogueData == null || dialogueData.autoProgressLines == null)
        {
            return false;
        }

        return dialogueData.autoProgressLines.Length > index && dialogueData.autoProgressLines[index];
    }
}

