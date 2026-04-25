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

    private int dialogueIndex;
    private bool isTyping;
    private string[] activeDialogueLines;
    private bool playerInRange = false;
    private float dialogueCooldown = 10f; // After dialogue closes, require 10 s before it can auto-start again
    private float lastDialogueEndTime = -1f;
    // True when the player closed dialogue via the button while still inside the trigger zone.
    // Prevents auto-restart until the player exits and re-enters the collider.
    private bool _dismissedWhileInRange = false;

    // Track the screen size that was used to build the current dialogue layout.
    // When the window is resized (editor) or the device is rotated the panel is
    // re-flowed so every piece of content stays readable and inside the screen.
    private int lastScreenWidth;
    private int lastScreenHeight;
    private GameObject dialogueFocusOverlay;
    private GameObject dialogueHintPanel;
    private TMP_Text dialogueHintText;

    private const string DialogueHintMessage = "Move away from npc to close the dialogue or press Space to speed up.";

    // Static reference to track which NPC currently has active dialogue
    // This prevents state conflicts when multiple NPCs share the same dialogue panel
    private static npc currentActiveNPC = null;

    private GameObject _nameLabel;
    private const float NameLabelYOffset = -0.8f;

    void Start()
    {
        BuildNameLabel();
    }

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

        // Keep world-space name label upright regardless of NPC transform rotation.
        if (_nameLabel != null)
            _nameLabel.transform.rotation = Quaternion.identity;

        // Re-flow the dialogue layout if the screen size changed while this
        // NPC's dialogue is on-screen (e.g. window resize, device rotation).
        if (currentActiveNPC == this && dialoguePanel != null && dialoguePanel.activeInHierarchy)
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                ApplyDialoguePanelStyle();
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
            // If the player dismissed dialogue while in range, don't auto-restart.
            // They must exit and re-enter the trigger to trigger dialogue again.
            if (_dismissedWhileInRange) return;
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
            _dismissedWhileInRange = false; // Reset so next entry triggers normally

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
        ShowDialogueFocusOverlay();
        ShowDialogueHintPanel();
        PhaseCAnimationManager.TriggerDialogueStart();
        MinigameCAudioManager.PlayDialogueOpen();
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
            // Mark as dismissed so the trigger stay doesn't immediately restart dialogue
            if (currentActiveNPC.playerInRange)
                currentActiveNPC._dismissedWhileInRange = true;
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
        MinigameCAudioManager.PlayDialogueClose();
        dialoguePanel.SetActive(false);
        HideDialogueFocusOverlay();
        HideDialogueHintPanel();
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

        lastScreenWidth  = Screen.width;
        lastScreenHeight = Screen.height;

        // Make sure the parent Canvas scales height-aware on portrait phones so
        // the dialogue doesn't collapse to a sliver on narrow screens.
        EnsureParentCanvasResponsive();

        Vector2 panelSize    = PhaseCUITheme.GetDialoguePanelSize();
        float   padding      = PhaseCUITheme.GetDialoguePadding();
        float   portraitSize = PhaseCUITheme.GetDialoguePortraitSize();
        float   nameRowH     = PhaseCUITheme.GetDialogueNameRowHeight();
        int     nameFont     = PhaseCUITheme.GetDialogueNameFontSize();
        int     bodyFont     = PhaseCUITheme.GetDialogueBodyFontSize();

        // ── Panel root: centred horizontally, above vertical centre ───────
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
            panelRect.pivot            = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta        = panelSize;
            panelRect.anchoredPosition = new Vector2(0f, PhaseCUITheme.GetDialogueYOffsetAboveCenter());
        }

        // ── Background image ──────────────────────────────────────────────
        Image panelImage = dialoguePanel.GetComponent<Image>();
        if (panelImage == null)
            panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.color        = PhaseCUITheme.PanelBg;
        panelImage.enabled      = true;
        panelImage.raycastTarget = true;
        if (panelImage.sprite == null)
            panelImage.sprite = CreateSolidSprite();

        if (dialogueBackground != null)
        {
            dialogueBackground.color         = PhaseCUITheme.PanelBg;
            dialogueBackground.enabled       = true;
            dialogueBackground.raycastTarget = true;
            if (dialogueBackground.sprite == null)
                dialogueBackground.sprite = CreateSolidSprite();
        }

        // ── Portrait (left side, vertically centred) ──────────────────────
        if (portraitImage != null)
        {
            RectTransform portraitRect = portraitImage.GetComponent<RectTransform>();
            if (portraitRect != null)
            {
                portraitRect.anchorMin        = new Vector2(0f, 0.5f);
                portraitRect.anchorMax        = new Vector2(0f, 0.5f);
                portraitRect.pivot            = new Vector2(0f, 0.5f);
                portraitRect.sizeDelta        = new Vector2(portraitSize, portraitSize);
                portraitRect.anchoredPosition = new Vector2(padding, 0f);
            }

            // Keep the full portrait visible inside its square slot.
            portraitImage.preserveAspect = true;
            portraitImage.type = Image.Type.Simple;
        }

        // ── Name text (top of body area) ──────────────────────────────────
        if (nameText != null)
        {
            RectTransform nameRect = nameText.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchorMin = new Vector2(0f, 1f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.pivot     = new Vector2(0.5f, 1f);
                nameRect.offsetMin = new Vector2(portraitSize + padding * 2f, -(padding * 0.5f + nameRowH));
                nameRect.offsetMax = new Vector2(-padding, -padding * 0.5f);
            }

            nameText.fontSize  = nameFont;
            nameText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color     = PhaseCUITheme.AccentCyan;
            nameText.enableWordWrapping = false;
        }

        // ── Dialogue body (fills remaining space) ─────────────────────────
        if (dialogueText != null)
        {
            RectTransform bodyRect = dialogueText.GetComponent<RectTransform>();
            if (bodyRect != null)
            {
                bodyRect.anchorMin = new Vector2(0f, 0f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.offsetMin = new Vector2(portraitSize + padding * 2f, padding);
                bodyRect.offsetMax = new Vector2(-padding, -(padding * 0.5f + nameRowH + 6f));
            }

            dialogueText.fontSize  = bodyFont;
            dialogueText.alignment = TextAlignmentOptions.Center;
            dialogueText.color     = PhaseCUITheme.TextBody;
            dialogueText.enableWordWrapping = true;
            dialogueText.overflowMode = TextOverflowModes.Truncate;
        }

        // ── Instruction panel shown below the dialogue popup ───────────────
        LayoutDialogueHintPanel(panelRect, bodyFont);

        // ── Remove close button (X) from dialogue panel ───────────────────
        HideCloseButton();

        // Keep the backdrop covering the whole screen after resize/orientation changes.
        LayoutDialogueFocusOverlay();
    }

    /// <summary>
    /// Patch the parent Canvas's CanvasScaler so it scales the same way as
    /// all the other Phase C HUD canvases. The scene ships with match=0 (width
    /// only), which makes the dialogue panel shrink to near-invisible on
    /// narrow portrait phones; matching height-aware fixes that.
    /// </summary>
    private void EnsureParentCanvasResponsive()
    {
        if (dialoguePanel == null) return;

        Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>(true);
        if (parentCanvas == null) return;

        CanvasScaler scaler = parentCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(PhaseCUITheme.RefWidth, PhaseCUITheme.RefHeight);
        scaler.matchWidthOrHeight  = PhaseCUITheme.CanvasMatchWidthOrHeight;

        // Dialogue is modal: keep this canvas above the generated HUD canvases
        // so the backdrop hides the other panels while an NPC is speaking.
        parentCanvas.overrideSorting = true;
        parentCanvas.sortingOrder    = 300;
    }

    private void ShowDialogueFocusOverlay()
    {
        if (dialoguePanel == null) return;

        EnsureDialogueFocusOverlay();
        LayoutDialogueFocusOverlay();

        if (dialogueFocusOverlay != null)
        {
            dialogueFocusOverlay.SetActive(true);
            dialogueFocusOverlay.transform.SetAsLastSibling();
        }

        // Dialogue must render above the dark backdrop.
        dialoguePanel.transform.SetAsLastSibling();
        if (dialogueHintPanel != null)
            dialogueHintPanel.transform.SetAsLastSibling();
    }

    private void HideDialogueFocusOverlay()
    {
        if (dialogueFocusOverlay == null)
            EnsureDialogueFocusOverlay();

        if (dialogueFocusOverlay != null)
            dialogueFocusOverlay.SetActive(false);
    }

    private void EnsureDialogueFocusOverlay()
    {
        if (dialoguePanel == null || dialogueFocusOverlay != null) return;

        Transform parent = dialoguePanel.transform.parent;
        if (parent == null) return;

        Transform existing = parent.Find("DialogueFocusOverlay");
        if (existing != null)
        {
            dialogueFocusOverlay = existing.gameObject;
            return;
        }

        dialogueFocusOverlay = new GameObject("DialogueFocusOverlay");
        dialogueFocusOverlay.transform.SetParent(parent, false);

        Image overlayImage = dialogueFocusOverlay.AddComponent<Image>();
        overlayImage.sprite        = CreateSolidSprite();
        overlayImage.color         = PhaseCUITheme.GetDialogueBackdropColor();
        overlayImage.raycastTarget = true;

        dialogueFocusOverlay.SetActive(false);
    }

    private void LayoutDialogueFocusOverlay()
    {
        if (dialogueFocusOverlay == null)
            EnsureDialogueFocusOverlay();
        if (dialogueFocusOverlay == null) return;

        Image overlayImage = dialogueFocusOverlay.GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.color         = PhaseCUITheme.GetDialogueBackdropColor();
            overlayImage.raycastTarget = true;
            if (overlayImage.sprite == null)
                overlayImage.sprite = CreateSolidSprite();
        }

        RectTransform overlayRect = dialogueFocusOverlay.GetComponent<RectTransform>();
        if (overlayRect == null)
            overlayRect = dialogueFocusOverlay.AddComponent<RectTransform>();

        overlayRect.anchorMin        = Vector2.zero;
        overlayRect.anchorMax        = Vector2.one;
        overlayRect.pivot            = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.offsetMin        = Vector2.zero;
        overlayRect.offsetMax        = Vector2.zero;
    }

    private void HideCloseButton()
    {
        if (dialoguePanel == null) return;

        Transform closeTransform = dialoguePanel.transform.Find("CloseButton");
        if (closeTransform == null) return;
        closeTransform.gameObject.SetActive(false);
    }

    private void EnsureDialogueHintPanel()
    {
        if (dialoguePanel == null) return;
        if (dialogueHintPanel != null && dialogueHintText != null) return;

        Transform parent = dialoguePanel.transform.parent;
        if (parent == null) return;

        Transform existing = parent.Find("DialogueHintPanel");
        if (existing != null)
        {
            dialogueHintPanel = existing.gameObject;
            dialogueHintText = dialogueHintPanel.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        dialogueHintPanel = new GameObject("DialogueHintPanel");
        dialogueHintPanel.transform.SetParent(parent, false);

        Image panelBg = dialogueHintPanel.AddComponent<Image>();
        panelBg.sprite = CreateSolidSprite();
        panelBg.color = new Color(0.09f, 0.14f, 0.24f, 0.95f);
        panelBg.raycastTarget = false;

        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(dialogueHintPanel.transform, false);
        Image borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(0.42f, 0.62f, 0.82f, 0.55f);
        borderImg.raycastTarget = false;
        RectTransform borderRect = borderGo.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-1f, -1f);
        borderRect.offsetMax = new Vector2(1f, 1f);
        borderGo.transform.SetAsFirstSibling();

        GameObject hintGo = new GameObject("DialogueHintText");
        hintGo.transform.SetParent(dialogueHintPanel.transform, false);
        dialogueHintText = hintGo.AddComponent<TextMeshProUGUI>();
    }

    private void LayoutDialogueHintPanel(RectTransform panelRect, int bodyFont)
    {
        EnsureDialogueHintPanel();
        if (dialogueHintPanel == null || dialogueHintText == null || panelRect == null) return;

        RectTransform hintPanelRect = dialogueHintPanel.GetComponent<RectTransform>();
        if (hintPanelRect == null)
            hintPanelRect = dialogueHintPanel.AddComponent<RectTransform>();

        float panelWidth = panelRect.sizeDelta.x;
        float panelHeight = Mathf.Clamp(bodyFont * 1.8f + 16f, 42f, 70f);
        float y = panelRect.anchoredPosition.y - (panelRect.sizeDelta.y * 0.5f) - 10f;

        hintPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        hintPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        hintPanelRect.pivot = new Vector2(0.5f, 1f);
        hintPanelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        hintPanelRect.anchoredPosition = new Vector2(0f, y);

        RectTransform hintRect = dialogueHintText.GetComponent<RectTransform>();
        hintRect.anchorMin = Vector2.zero;
        hintRect.anchorMax = Vector2.one;
        hintRect.offsetMin = new Vector2(16f, 6f);
        hintRect.offsetMax = new Vector2(-16f, -6f);

        dialogueHintText.text = DialogueHintMessage;
        dialogueHintText.fontSize = Mathf.Max(14, bodyFont - 4);
        dialogueHintText.color = PhaseCUITheme.TextSecondary;
        dialogueHintText.alignment = TextAlignmentOptions.Center;
        dialogueHintText.enableWordWrapping = true;
        dialogueHintText.raycastTarget = false;
    }

    private void ShowDialogueHintPanel()
    {
        if (dialoguePanel == null) return;
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        LayoutDialogueHintPanel(panelRect, PhaseCUITheme.GetDialogueBodyFontSize());
        if (dialogueHintPanel != null)
        {
            dialogueHintPanel.SetActive(true);
            dialogueHintPanel.transform.SetAsLastSibling();
        }
    }

    private void HideDialogueHintPanel()
    {
        if (dialogueHintPanel != null)
            dialogueHintPanel.SetActive(false);
    }

    private void BuildNameLabel()
    {
        if (dialogueData == null || string.IsNullOrEmpty(dialogueData.npcName)) return;

        // Match the NPC sprite's sorting layer so the label isn't hidden by background tiles
        SpriteRenderer npcSr = GetComponentInChildren<SpriteRenderer>();
        int layerID    = npcSr != null ? npcSr.sortingLayerID : 0;
        int layerOrder = npcSr != null ? npcSr.sortingOrder    : 0;

        _nameLabel = new GameObject("NPCNameLabel");
        _nameLabel.transform.SetParent(transform, false);
        _nameLabel.transform.localPosition = new Vector3(0f, NameLabelYOffset, 0f);

        // Split "Dr. Sarah Chen" → "Dr. Sarah" on line 1, "Chen" on line 2
        string[] words = dialogueData.npcName.Split(' ');
        string firstLine  = words.Length > 1 ? string.Join(" ", words, 0, words.Length - 1) : dialogueData.npcName;
        string secondLine = words.Length > 1 ? words[words.Length - 1] : "";
        string displayText = words.Length > 1 ? firstLine + "\n" + secondLine : firstLine;

        TextMesh tm = _nameLabel.AddComponent<TextMesh>();
        tm.text          = displayText;
        tm.fontSize      = 32;
        tm.characterSize = 0.06f;
        tm.lineSpacing   = 1f;
        tm.anchor        = TextAnchor.MiddleCenter;
        tm.alignment     = TextAlignment.Center;
        tm.color         = new Color(PhaseCUITheme.AccentCyan.r, PhaseCUITheme.AccentCyan.g, PhaseCUITheme.AccentCyan.b, 1f);
        tm.font          = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        MeshRenderer mr = _nameLabel.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerID = layerID;
            mr.sortingOrder   = layerOrder + 2;
        }

        // Dark background pill sized for two lines at half font
        GameObject bg = new GameObject("NPCNameLabelBg");
        bg.transform.SetParent(_nameLabel.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.01f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite           = CreateSolidSprite();
        bgSr.color            = new Color(0.05f, 0.08f, 0.18f, 0.80f);
        bgSr.sortingLayerID   = layerID;
        bgSr.sortingOrder     = layerOrder + 1;

        int longestLen = Mathf.Max(firstLine.Length, secondLine.Length);
        float bgWidth  = longestLen * 0.048f + 0.3f;
        float bgHeight = words.Length > 1 ? 0.46f : 0.26f;
        bg.transform.localScale = new Vector3(bgWidth, bgHeight, 1f);
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
