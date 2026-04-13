using UnityEngine;

/// <summary>
/// Shared Phase C UI design tokens: colors, spacing, typography.
/// Use for opening, guide bar, story moments, and any Phase C overlay for consistent UX.
/// Mobile values are intentionally large - the primary target is phone screens.
/// </summary>
public static class PhaseCUITheme
{
    // ---- Reference resolution (match CanvasScaler) ----
    public const float RefWidth  = 1920f;
    public const float RefHeight = 1080f;

    // ---- Colors: background ----
    public static readonly Color BackgroundTop    = new Color(0.02f, 0.04f, 0.12f, 0.98f);
    public static readonly Color BackgroundBottom = new Color(0.06f, 0.02f, 0.08f, 0.98f);
    public static readonly Color OverlayDark      = new Color(0.02f, 0.04f, 0.10f, 0.92f);

    // ---- Colors: panels ----
    public static readonly Color PanelBg     = new Color(0.07f, 0.09f, 0.16f, 0.94f);
    public static readonly Color PanelBorder = new Color(0.35f, 0.55f, 0.75f, 0.50f);

    // ---- Colors: accents ----
    public static readonly Color AccentGold     = new Color(0.89f, 0.75f, 0.35f, 1f);
    public static readonly Color AccentCyan     = new Color(0.45f, 0.72f, 0.88f, 1f);
    public static readonly Color AccentCyanMuted = new Color(0.45f, 0.72f, 0.88f, 0.85f);

    // ---- Colors: text ----
    public static readonly Color TextTitle     = new Color(0.95f, 0.92f, 0.85f, 1f);
    public static readonly Color TextBody      = new Color(0.88f, 0.88f, 0.90f, 1f);
    public static readonly Color TextPrimary   = new Color(0.95f, 0.94f, 0.90f, 1f);
    public static readonly Color TextSecondary = new Color(0.75f, 0.78f, 0.82f, 1f);
    public static readonly Color TextError     = new Color(1.00f, 0.35f, 0.35f, 1f);

    // ---- Colors: step states ----
    public static readonly Color StepDone    = new Color(0.40f, 0.70f, 0.45f, 1f);
    public static readonly Color StepCurrent = new Color(0.89f, 0.75f, 0.35f, 1f);
    public static readonly Color StepPending = new Color(0.35f, 0.38f, 0.45f, 0.9f);
    public static readonly Color DotInactive = new Color(0.30f, 0.35f, 0.45f, 0.8f);

    // ---- Colors: buttons ----
    public static readonly Color ButtonBg        = new Color(0.15f, 0.35f, 0.55f, 1f);
    public static readonly Color ButtonAccent    = new Color(0.40f, 0.65f, 0.90f, 1f);
    public static readonly Color ButtonGlow      = new Color(0.40f, 0.65f, 0.90f, 0.3f);
    public static readonly Color ButtonHighlight = new Color(0.92f, 0.94f, 1.00f, 1f);
    public static readonly Color ButtonPressed   = new Color(0.85f, 0.88f, 0.95f, 1f);

    // ---- Spacing (touch-friendly min ~44 px) ----
    public const float PaddingPanel    = 32f;
    public const float PaddingTight    = 24f;
    public const float PaddingWide     = 48f;
    public const float AccentBarHeight = 5f;
    public const float MinTouchHeight  = 56f;
    public const float ButtonHeight    = 60f;
    public const float ButtonWidthMin  = 300f;
    public const float DotSize         = 14f;
    public const float DotSpacing      = 32f;
    public const float GuideDotSize    = 14f;
    public const float GuideDotSpacing = 36f;

    // ---- Typography (static constants - used by opening screen & story moments) ----
    public const float FontSizeBadge        = 13f;
    public const float FontSizeTitle        = 34f;
    public const float FontSizeBody         = 26f;
    public const float FontSizeBodySmall    = 22f;
    public const float FontSizeCaption      = 18f;
    public const float FontSizeButton       = 24f;
    public const float LineSpacingBody      = 10f;
    public const float GuideStepTitleSize   = 22f;
    public const float GuideObjectiveSize   = 20f;
    public const float GuideCaptionSize     = 16f;
    public const float StoryMomentTitleSize = 32f;
    public const float StoryMomentBodySize  = 24f;

    // ---- Mobile responsive helpers ----

    public static bool IsMobileScreen =>
        Application.isMobilePlatform || Screen.width < 960;

    public static bool IsPortrait => Screen.height > Screen.width;

    // Portrait → match-height so UI fills narrow screens without clipping.
    // Landscape / desktop → balanced 0.5 blend.
    public static float CanvasMatchWidthOrHeight =>
        IsPortrait ? 1f : 0.5f;

    // Notification toast: capped at 85 % of screen width on small screens.
    public static float GetNotifWidth() =>
        Mathf.Min(340f, Screen.width * 0.85f);

    // Delivery panel: capped at 90 % of screen width.
    public static float GetDeliveryWidth() =>
        Mathf.Min(520f, Screen.width * 0.9f);

    // Delivery top offset: 25 % down from the top edge regardless of resolution.
    public static float GetDeliveryTopOffset() =>
        -(Screen.height * 0.25f);

    // Required items panel (collapsed): capped at 50 % of screen width.
    public static float GetRequiredPanelWidth() =>
        Mathf.Min(300f, Screen.width * 0.50f);

    // Inventory slot size: large touch targets on mobile.
    public static float GetInventorySlotSize() =>
        IsMobileScreen ? 110f : 80f;

    // ---- Bottom hint strip ----

    public static float GetHintStripHeight()          => IsMobileScreen ? 100f : 60f;
    public static float GetHintStripMinimizedHeight() => IsMobileScreen ?  38f : 28f;
    public static int   GetHintFontSize()             => IsMobileScreen ?   30 : 18;

    // Split point between action-hint section (left) and task section (right).
    public static float GetHintMiddleSectionEnd() => IsMobileScreen ? 0.52f : 0.55f;

    // ---- Required items panel ----

    // Panel width: wider on mobile so text is readable.
    public static float GetRequiredPanelWidthExpanded() =>
        IsMobileScreen ? Mathf.Min(340f, Screen.width * 0.60f)
                       : Mathf.Min(270f, Screen.width * 0.45f);

    public static float GetRequiredTitleBarHeight() => IsMobileScreen ? 60f : 44f;
    public static float GetRequiredItemRowHeight()  => IsMobileScreen ? 56f : 44f;
    public static float GetRequiredIconSize()       => IsMobileScreen ? 48f : 36f;
    public static float GetRequiredNpcTextHeight()  => IsMobileScreen ? 36f : 24f;

    // Three-level font hierarchy: title (bold+gold), item rows, subtitle.
    public static int GetRequiredTitleFontSize()    => IsMobileScreen ? 24 : 15;
    public static int GetRequiredItemFontSize()     => IsMobileScreen ? 22 : 14;
    public static int GetRequiredSubtitleFontSize() => IsMobileScreen ? 20 : 13;

    // ---- Shared side-panel height (Assembly + Required Items) ----

    public static float GetSidePanelHeight() => IsMobileScreen ? 320f : 220f;

    // ---- Assembly visualizer (bottom-left spacecraft panel) ----

    public static float GetAssemblyPanelWidth()  => IsMobileScreen ? 220f : 170f;
    public static float GetAssemblyPanelHeight() => GetSidePanelHeight();
    public static int   GetAssemblyTitleFont()   => IsMobileScreen ?  20  :  14;
    public static int   GetAssemblyStepFont()    => IsMobileScreen ?  16  :  12;

    // ---- Return-to-hub button ----

    public static float GetHubButtonWidth()  => IsMobileScreen ? 260f : 210f;
    public static int   GetHubButtonFont()   => IsMobileScreen ?  20  :  15;
    public static float GetHubBottomOffset() => GetHintStripHeight() + 8f;

    // ---- Guide panel (top strip - no step-dot row) ----

    public static float GetGuidePanelHeight()       => IsMobileScreen ? 220f : 150f;
    public static float GetGuideStepTitleFontSize() => IsMobileScreen ?  32f :  22f;
    public static float GetGuideObjectiveFontSize() => IsMobileScreen ?  28f :  20f;
    public static float GetGuideCaptionFontSize()   => IsMobileScreen ?  22f :  16f;

    // Layout Y offsets (top-anchored, negative = downward, no dots row).
    // Mobile  220 px panel:  title 40 px zone, objective 84 px, talkTo 44 px → 186 px used.
    // Desktop 150 px panel:  title 26 px zone, objective 66 px, talkTo 36 px → 146 px used.
    public static float GetGuideStepTitleY()      => IsMobileScreen ?  -6f :  -6f;
    public static float GetGuideStepTitleBottom() => IsMobileScreen ? -46f : -32f;
    public static float GetGuideObjectiveY()      => IsMobileScreen ? -52f : -38f;
    public static float GetGuideObjectiveBottom() => IsMobileScreen ? -136f : -104f;
    public static float GetGuideTalkToY()         => IsMobileScreen ? -142f : -110f;
    public static float GetGuideTalkToBottom()    => IsMobileScreen ? -186f : -146f;

    // Dot helpers kept so other code that references them does not break.
    public static float GetGuideDotsContainerY()   => IsMobileScreen ? -40f : -32f;
    public static float GetGuideDotsContainerH()   => IsMobileScreen ?  54f :  44f;
    public static float GetGuideDotSize()          => IsMobileScreen ?  20f :  14f;
    public static float GetGuideDotSpacing()       => IsMobileScreen ?  46f :  36f;
    public static float GetGuideDotLabelFontSize() => IsMobileScreen ?  13f :   9f;

    // ---- Opening / welcome screen ----

    public static float GetOpeningTitleFontSize()  => IsMobileScreen ? 40f : 34f;
    public static float GetOpeningBodyFontSize()   => IsMobileScreen ? 28f : 26f;
    public static float GetOpeningBadgeFontSize()  => IsMobileScreen ? 18f : 13f;
    public static float GetOpeningButtonFontSize() => IsMobileScreen ? 30f : 24f;

    // ---- Inventory popup ----

    public static int GetInventoryTitleFontSize()  => IsMobileScreen ? 28 : 22;
    public static int GetInventoryStatusFontSize() => IsMobileScreen ? 22 : 16;
    public static int GetInventoryHintFontSize()   => IsMobileScreen ? 18 : 13;
    public static float GetInventoryTitleHeight()  => IsMobileScreen ? 52f : 40f;
    public static float GetInventoryStatusHeight() => IsMobileScreen ? 36f : 28f;
    public static float GetInventoryHintHeight()   => IsMobileScreen ? 36f : 28f;
    public static float GetInventoryPadding()      => IsMobileScreen ? 32f : 24f;
}
