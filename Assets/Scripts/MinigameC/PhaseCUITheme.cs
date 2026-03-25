using UnityEngine;

/// <summary>
/// Shared Phase C UI design tokens: colors, spacing, typography.
/// Use for opening, guide bar, story moments, and any Phase C overlay for consistent UX.
/// </summary>
public static class PhaseCUITheme
{
    // ---- Reference resolution (match CanvasScaler) ----
    public const float RefWidth = 1920f;
    public const float RefHeight = 1080f;

    // ---- Colors: background ----
    public static readonly Color BackgroundTop = new Color(0.02f, 0.04f, 0.12f, 0.98f);
    public static readonly Color BackgroundBottom = new Color(0.06f, 0.02f, 0.08f, 0.98f);
    public static readonly Color OverlayDark = new Color(0.02f, 0.04f, 0.1f, 0.92f);

    // ---- Colors: panels ----
    public static readonly Color PanelBg = new Color(0.07f, 0.09f, 0.16f, 0.94f);
    public static readonly Color PanelBorder = new Color(0.35f, 0.55f, 0.75f, 0.5f);

    // ---- Colors: accents ----
    public static readonly Color AccentGold = new Color(0.89f, 0.75f, 0.35f, 1f);
    public static readonly Color AccentCyan = new Color(0.45f, 0.72f, 0.88f, 1f);
    public static readonly Color AccentCyanMuted = new Color(0.45f, 0.72f, 0.88f, 0.85f);

    // ---- Colors: text ----
    public static readonly Color TextTitle = new Color(0.95f, 0.92f, 0.85f, 1f);
    public static readonly Color TextBody = new Color(0.88f, 0.88f, 0.9f, 1f);
    public static readonly Color TextPrimary = new Color(0.95f, 0.94f, 0.9f, 1f);
    public static readonly Color TextSecondary = new Color(0.75f, 0.78f, 0.82f, 1f);
    public static readonly Color TextError = new Color(1f, 0.35f, 0.35f, 1f);

    // ---- Colors: step states ----
    public static readonly Color StepDone = new Color(0.4f, 0.7f, 0.45f, 1f);
    public static readonly Color StepCurrent = new Color(0.89f, 0.75f, 0.35f, 1f);
    public static readonly Color StepPending = new Color(0.35f, 0.38f, 0.45f, 0.9f);
    public static readonly Color DotInactive = new Color(0.3f, 0.35f, 0.45f, 0.8f);

    // ---- Colors: buttons ----
    public static readonly Color ButtonBg = new Color(0.15f, 0.35f, 0.55f, 1f);
    public static readonly Color ButtonAccent = new Color(0.4f, 0.65f, 0.9f, 1f);
    public static readonly Color ButtonGlow = new Color(0.4f, 0.65f, 0.9f, 0.3f);
    public static readonly Color ButtonHighlight = new Color(0.92f, 0.94f, 1f, 1f);
    public static readonly Color ButtonPressed = new Color(0.85f, 0.88f, 0.95f, 1f);

    // ---- Spacing (UX: touch-friendly min ~44px) ----
    public const float PaddingPanel = 32f;
    public const float PaddingTight = 24f;
    public const float PaddingWide = 48f;
    public const float AccentBarHeight = 5f;
    public const float MinTouchHeight = 48f;
    public const float ButtonHeight = 52f;
    public const float ButtonWidthMin = 280f;
    public const float DotSize = 12f;
    public const float DotSpacing = 28f;
    public const float GuideDotSize = 14f;
    public const float GuideDotSpacing = 36f;

    // ---- Typography ----
    public const float FontSizeBadge = 13f;
    public const float FontSizeTitle = 34f;
    public const float FontSizeBody = 26f;
    public const float FontSizeBodySmall = 22f;
    public const float FontSizeCaption = 18f;
    public const float FontSizeButton = 24f;
    public const float LineSpacingBody = 10f;
    public const float GuideStepTitleSize = 22f;
    public const float GuideObjectiveSize = 20f;
    public const float GuideCaptionSize = 16f;
    public const float StoryMomentTitleSize = 32f;
    public const float StoryMomentBodySize = 24f;

    // ---- Mobile responsive helpers ----

    public static bool IsMobileScreen =>
        Application.isMobilePlatform || Screen.width < 960;

    public static bool IsPortrait => Screen.height > Screen.width;

    // Portrait uses match-height so UI fills narrow screens without clipping.
    // Landscape and desktop use the balanced 0.5f blend.
    public static float CanvasMatchWidthOrHeight =>
        IsPortrait ? 1f : 0.5f;

    // Notification toast: capped at 85% of screen width on small screens.
    public static float GetNotifWidth() =>
        Mathf.Min(310f, Screen.width * 0.85f);

    // Delivery panel: capped at 90% of screen width.
    public static float GetDeliveryWidth() =>
        Mathf.Min(480f, Screen.width * 0.9f);

    // Delivery top offset: 25% down from the top edge regardless of resolution.
    public static float GetDeliveryTopOffset() =>
        -(Screen.height * 0.25f);

    // Required items panel: capped at 45% of screen width on mobile.
    public static float GetRequiredPanelWidth() =>
        Mathf.Min(260f, Screen.width * 0.45f);

    // Inventory slot size: larger touch targets on mobile.
    public static float GetInventorySlotSize() =>
        IsMobileScreen ? 90f : 80f;

    // Hint strip: taller on mobile for easier reading and larger fonts.
    public static float GetHintStripHeight() =>
        IsMobileScreen ? 72f : 56f;

    // Minimized hint strip: just enough for the toggle tab.
    public static float GetHintStripMinimizedHeight() =>
        IsMobileScreen ? 32f : 26f;

    // Font size for the bottom hint strip text.
    public static int GetHintFontSize() =>
        IsMobileScreen ? 22 : 18;

    // Font size for required-items panel rows.
    public static int GetRequiredItemFontSize() =>
        IsMobileScreen ? 17 : 14;

    // Required items panel: wider on mobile so text is readable.
    public static float GetRequiredPanelWidthExpanded() =>
        IsMobileScreen ? Mathf.Min(300f, Screen.width * 0.55f) : Mathf.Min(260f, Screen.width * 0.45f);
}
