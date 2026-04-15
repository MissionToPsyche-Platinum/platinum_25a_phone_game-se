using UnityEngine;

/// <summary>
/// Shared Phase C UI design tokens: colors, spacing, typography.
///
/// Responsive tiers (evaluated live each call - no caching):
///
///   Phone          Portrait phone / narrow editor window.
///                  Canvas scale ≈ 2.35 on iPhone 13 portrait (height-matched).
///                  Effective canvas width ≈ 499 units - keep panels narrow.
///
///   PhoneLandscape Landscape phone. Wide canvas (≈ 2035 units), short height (≈ 941 units).
///                  Use generous panel widths and short panel heights.
///
///   Tablet         iPad / mid-size device (portrait or landscape).
///
///   Desktop        PC / Mac. Canvas ≈ 1920×1080.
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
    public static readonly Color AccentGold      = new Color(0.89f, 0.75f, 0.35f, 1f);
    public static readonly Color AccentCyan      = new Color(0.45f, 0.72f, 0.88f, 1f);
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

    // ---- Spacing ----
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

    // ---- Typography constants (opening screen & story moments) ----
    public const float FontSizeBadge        = 15f;
    public const float FontSizeTitle        = 36f;
    public const float FontSizeBody         = 26f;
    public const float FontSizeBodySmall    = 22f;
    public const float FontSizeCaption      = 18f;
    public const float FontSizeButton       = 26f;
    public const float LineSpacingBody      = 10f;
    public const float GuideStepTitleSize   = 26f;
    public const float GuideObjectiveSize   = 22f;
    public const float GuideCaptionSize     = 18f;
    public const float StoryMomentTitleSize = 32f;
    public const float StoryMomentBodySize  = 24f;

    /// <summary>Space reserved at top of story popup (accent + padding + wrapped title).</summary>
    public static float GetStoryMomentTitleBlockHeight() =>
        Tier(124f, 104f, 132f, 132f);

    /// <summary>Space reserved at bottom for Continue + margins.</summary>
    public static float GetStoryMomentFooterHeight() =>
        Tier(PaddingWide + ButtonHeight + 28f, PaddingWide + ButtonHeight + 22f, PaddingWide + ButtonHeight + 28f, PaddingWide + ButtonHeight + 28f);

    public static float GetStoryMomentBodyFontSize() =>
        Tier(20f, 18f, 22f, StoryMomentBodySize);

    // =========================================================================
    // ---- 4-Tier screen category ----
    // =========================================================================

    public enum ScreenCategory
    {
        /// <summary>Portrait phone or narrow editor window.</summary>
        Phone,
        /// <summary>Landscape phone (wide, height-constrained).</summary>
        PhoneLandscape,
        /// <summary>iPad / mid-size tablet.</summary>
        Tablet,
        /// <summary>Desktop / standalone.</summary>
        Desktop
    }

    /// <summary>
    /// Classify the current screen into one of four tiers.
    ///
    /// On real iOS/Android builds, Application.isMobilePlatform is authoritative.
    ///   short-edge &lt; 1400 px → phone (portrait or landscape based on orientation).
    ///   short-edge ≥ 1400 px → tablet.
    ///
    /// In the Unity editor / desktop builds:
    ///   Landscape with aspect &gt; 1.85 and min-edge &lt; 1500 → PhoneLandscape
    ///     (covers iPhone 13/14 landscape at 2532×1170).
    ///   Portrait / narrow with width &lt; 1350 → Phone
    ///     (covers iPhone 13 portrait at 1170×2532 and small game-view windows).
    ///   Standard desktop min-edge ≥ 1000 → Desktop.
    ///   Everything else → Tablet.
    /// </summary>
    public static ScreenCategory GetScreenCategory()
    {
        if (Application.isMobilePlatform)
        {
            float shortEdge = Mathf.Min(Screen.width, Screen.height);
            if (shortEdge < 1400f)
                return IsPortrait ? ScreenCategory.Phone : ScreenCategory.PhoneLandscape;
            return ScreenCategory.Tablet;
        }

        // Editor / desktop / WebGL
        float sw      = Screen.width;
        float sh      = Screen.height;
        float minEdge = Mathf.Min(sw, sh);
        float maxEdge = Mathf.Max(sw, sh);
        float aspect  = maxEdge / Mathf.Max(minEdge, 1f);

        // Modern phones in landscape have 19:9+ aspect (> 1.85).
        // Desktop 16:9 is 1.78, so this threshold cleanly separates them.
        // minEdge < 1500 excludes ultrawide desktop monitors.
        if (!IsPortrait && aspect > 1.85f && minEdge < 1500f)
            return ScreenCategory.PhoneLandscape;

        // Portrait phone widths: up to iPhone 13 Pro Max = 1284 px.
        // Also covers narrow game-view windows used for phone testing.
        if (sw < 1350f)
            return ScreenCategory.Phone;

        // Standard desktop resolutions: 1080 p and above.
        if (minEdge >= 1000f)
            return ScreenCategory.Desktop;

        return ScreenCategory.Tablet;
    }

    // ---- Convenience shortcuts ----
    public static bool IsPhone          => GetScreenCategory() == ScreenCategory.Phone;
    public static bool IsPhoneLandscape => GetScreenCategory() == ScreenCategory.PhoneLandscape;
    public static bool IsTablet         => GetScreenCategory() == ScreenCategory.Tablet;
    public static bool IsDesktop        => GetScreenCategory() == ScreenCategory.Desktop;

    /// <summary>Backward-compat shim: true for Phone or PhoneLandscape.</summary>
    public static bool IsMobileScreen => IsPhone || IsPhoneLandscape;

    public static bool IsPortrait => Screen.height > Screen.width;

    // ---- Canvas scaler matchWidthOrHeight per tier ----
    public static float CanvasMatchWidthOrHeight =>
        GetScreenCategory() switch
        {
            ScreenCategory.Phone          => IsPortrait ? 1.0f : 0.7f,
            ScreenCategory.PhoneLandscape => 0.3f,
            ScreenCategory.Tablet         => IsPortrait ? 0.8f : 0.5f,
            _                             => 0.5f
        };

    // ---- Per-tier helper ----
    private static T Tier<T>(T phone, T phoneLandscape, T tablet, T desktop) =>
        GetScreenCategory() switch
        {
            ScreenCategory.Phone          => phone,
            ScreenCategory.PhoneLandscape => phoneLandscape,
            ScreenCategory.Tablet         => tablet,
            _                             => desktop
        };

    // =========================================================================
    // ---- Safe-area helpers (punch-hole camera / notch) ----
    // =========================================================================

    /// <summary>
    /// Top inset in canvas units caused by a punch-hole camera or notch.
    /// Apply as a negative Y offset on the guide panel so text clears the cutout.
    /// Falls back to a fixed offset when Screen.safeArea reports no inset
    /// (Unity editor without Device Simulator).
    /// </summary>
    public static float GetSafeAreaTopOffset()
    {
        Rect safe = Screen.safeArea;
        float topInsetPx = Screen.height - (safe.y + safe.height);
        if (topInsetPx >= 2f)
            return topInsetPx / GetCanvasScaleFactor();

        // Fallback fixed offsets in canvas units for common phone form-factors.
        if (IsPhone)          return 44f;   // portrait: clears most notches/Dynamic Islands
        if (IsPhoneLandscape) return 18f;   // landscape: notch is at side, small top margin
        return 0f;
    }

    /// <summary>
    /// Bottom inset in canvas units (home-bar area on iPhones without a physical button).
    /// Apply as a positive Y offset on the hint strip.
    /// </summary>
    public static float GetSafeAreaBottomOffset()
    {
        if (!Application.isMobilePlatform) return 0f;
        float bottomInsetPx = Screen.safeArea.y;
        if (bottomInsetPx < 2f) return 0f;
        return bottomInsetPx / GetCanvasScaleFactor();
    }

    private static float GetCanvasScaleFactor()
    {
        float m  = CanvasMatchWidthOrHeight;
        float rw = Mathf.Max(Screen.width,  1) / RefWidth;
        float rh = Mathf.Max(Screen.height, 1) / RefHeight;
        return Mathf.Exp(Mathf.Log(rw) * (1f - m) + Mathf.Log(rh) * m);
    }

    // =========================================================================
    // ---- Notification / delivery widths ----
    // =========================================================================

    public static float GetNotifWidth() =>
        Mathf.Min(Tier(340f, 440f, 360f, 380f), Screen.width * 0.85f);

    public static float GetDeliveryWidth() =>
        Mathf.Min(Tier(520f, 620f, 540f, 560f), Screen.width * 0.9f);

    public static float GetDeliveryTopOffset() => -(Screen.height * 0.25f);

    public static float GetRequiredPanelWidth() =>
        Mathf.Min(Tier(190f, 440f, 310f, 290f), Screen.width * 0.50f);

    // =========================================================================
    // ---- Inventory ----
    // =========================================================================

    public static float GetInventorySlotSize()       => Tier(110f, 120f,  95f,  90f);
    public static int   GetInventoryTitleFontSize()  => Tier(  28,   30,   24,   24);
    public static int   GetInventoryStatusFontSize() => Tier(  22,   24,   18,   18);
    public static int   GetInventoryHintFontSize()   => Tier(  18,   20,   15,   15);
    public static float GetInventoryTitleHeight()    => Tier( 52f,  56f,  46f,  46f);
    public static float GetInventoryStatusHeight()   => Tier( 36f,  38f,  30f,  30f);
    public static float GetInventoryHintHeight()     => Tier( 36f,  38f,  30f,  30f);
    public static float GetInventoryPadding()        => Tier( 32f,  36f,  28f,  28f);

    // =========================================================================
    // ---- Bottom hint strip ----
    // =========================================================================

    public static float GetHintStripHeight()          => Tier( 88f,  68f, 80f, 72f);
    public static float GetHintStripMinimizedHeight() => Tier( 36f,  28f, 30f, 30f);
    public static int   GetHintFontSize()             => Tier(  28,   24,  22,  22);
    public static float GetHintMiddleSectionEnd()     => Tier(0.52f, 0.54f, 0.54f, 0.55f);

    // =========================================================================
    // ---- Shared side-panel height (Assembly + Required Items) ----
    // =========================================================================

    public static float GetSidePanelHeight() => Tier(260f, 195f, 275f, 250f);

    // =========================================================================
    // ---- Required items panel ----
    //
    //  Portrait phone effective canvas width ≈ 499 units (iPhone 13).
    //  Assembly right-edge: 14 + 120 = 134 units.
    //  Required Items left-edge must be > 134 to avoid overlap.
    //  With width 185: left = 499 - 16 - 185 = 298 → gap = 164 units. ✓
    // =========================================================================

    public static float GetRequiredPanelWidthExpanded()
    {
        switch (GetScreenCategory())
        {
            case ScreenCategory.Phone:
                float canvasW = Screen.width * RefHeight / Screen.height;
                return Mathf.Min(185f, canvasW * 0.37f);
            case ScreenCategory.PhoneLandscape:
                return 420f;
            case ScreenCategory.Tablet:
                return 310f;
            default:
                return 290f;
        }
    }

    public static float GetRequiredTitleBarHeight() => Tier( 52f,  56f,  52f,  50f);
    public static float GetRequiredItemRowHeight()  => Tier( 50f,  54f,  48f,  48f);
    public static float GetRequiredIconSize()       => Tier( 42f,  46f,  40f,  40f);
    public static float GetRequiredNpcTextHeight()  => Tier( 32f,  34f,  28f,  28f);

    public static int GetRequiredTitleFontSize()    => Tier(20, 28, 20, 20);
    public static int GetRequiredItemFontSize()     => Tier(18, 24, 18, 18);
    public static int GetRequiredSubtitleFontSize() => Tier(16, 22, 16, 16);

    // =========================================================================
    // ---- Assembly visualizer (bottom-left spacecraft panel) ----
    // =========================================================================

    public static float GetAssemblyPanelWidth()      => Tier(132f, 352f, 206f, 200f);
    public static float GetAssemblyPanelHeight()     => GetSidePanelHeight();
    public static float GetAssemblyTitleBarHeight()  => Tier( 44f,  50f,  44f,  42f);
    public static int   GetAssemblyTitleFont()       => Tier(  16,   22,   17,   18);

    // Step font: visible and large enough to read at all scales.
    // Phone portrait scale ≈ 2.35: font 18 → 42 px on screen.
    // Phone landscape scale ≈ 1.27: font 22 → 28 px on screen.
    public static int   GetAssemblyStepFont()        => Tier(  18,   22,   16,   16);

    // =========================================================================
    // ---- Return-to-hub button ----
    // =========================================================================

    public static float GetHubButtonWidth()  => Tier(220f, 320f, 235f, 230f);
    public static int   GetHubButtonFont()   => Tier(  18,   22,   17,   18);
    public static float GetHubBottomOffset() => GetHintStripHeight() + 8f;

    // =========================================================================
    // ---- Guide panel (top strip) ----
    //
    //  "Talk to: [NPC]" and "E/Space: talk | ..." rows have been removed from
    //  the guide panel. Heights are sized for title + objective only.
    //
    //  Phone portrait  (158 px): smaller fonts + centered objective; room for wrapped lines.
    //  Phone landscape (100 px): same intent in short vertical space.
    // =========================================================================

    public static float GetGuidePanelHeight()       => Tier(158f, 100f, 140f, 125f);
    public static float GetGuideTitleBarHeight()    => Tier( 48f,  38f,  44f,  42f);
    public static float GetGuideStepTitleFontSize() => Tier( 26f,  24f,  26f,  26f);
    public static float GetGuideObjectiveFontSize() => Tier( 20f,  18f,  22f,  22f);
    public static float GetGuideCaptionFontSize()   => Tier( 20f,  18f,  18f,  18f);

    // ---- Guide Y offsets (top-anchored, negative = downward) ----
    // Step title zone + objective zone only (talkTo / controls rows removed).

    public static float GetGuideStepTitleY()      => -6f;
    public static float GetGuideStepTitleBottom() => Tier(-46f, -34f, -42f, -38f);
    public static float GetGuideObjectiveY()      => Tier(-50f, -38f, -48f, -44f);
    public static float GetGuideObjectiveBottom() => Tier(-150f, -96f, -130f, -116f);

    // Kept for backward compatibility (no longer rendered in guide panel).
    public static float GetGuideTalkToY()         => Tier(-148f,  -98f, -136f, -122f);
    public static float GetGuideTalkToBottom()    => Tier(-186f, -124f, -174f, -156f);

    // Dot helpers retained so other code referencing them does not break.
    public static float GetGuideDotsContainerY()   => Tier(-40f, -34f, -36f, -34f);
    public static float GetGuideDotsContainerH()   => Tier( 54f,  40f,  48f,  44f);
    public static float GetGuideDotSize()          => Tier( 20f,  16f,  16f,  16f);
    public static float GetGuideDotSpacing()       => Tier( 46f,  38f,  40f,  38f);
    public static float GetGuideDotLabelFontSize() => Tier( 13f,  11f,  11f,  11f);

    // =========================================================================
    // ---- Opening / welcome screen ----
    // =========================================================================

    public static float GetOpeningTitleFontSize()  => Tier(32f, 30f, 36f, 36f);
    public static float GetOpeningBodyFontSize()   => Tier(21f, 19f, 26f, 26f);
    public static float GetOpeningBadgeFontSize()  => Tier(14f, 13f, 15f, 15f);
    public static float GetOpeningButtonFontSize() => Tier(24f, 22f, 26f, 26f);

    /// <summary>Opening panel anchors (fractions of parent). Wider insets on phone so content stays inside the window.</summary>
    public static Vector2 GetOpeningPanelAnchorMin() =>
        Tier(new Vector2(0.05f, 0.12f), new Vector2(0.06f, 0.14f), new Vector2(0.12f, 0.18f), new Vector2(0.12f, 0.18f));

    public static Vector2 GetOpeningPanelAnchorMax() =>
        Tier(new Vector2(0.95f, 0.82f), new Vector2(0.94f, 0.76f), new Vector2(0.88f, 0.78f), new Vector2(0.88f, 0.78f));

    public static Vector2 GetOpeningBorderAnchorMin() =>
        Tier(new Vector2(0.03f, 0.10f), new Vector2(0.04f, 0.12f), new Vector2(0.10f, 0.16f), new Vector2(0.10f, 0.16f));

    public static Vector2 GetOpeningBorderAnchorMax() =>
        Tier(new Vector2(0.97f, 0.84f), new Vector2(0.96f, 0.78f), new Vector2(0.90f, 0.80f), new Vector2(0.90f, 0.80f));

    public static float GetOpeningButtonWidth() => Tier(240f, 260f, 300f, 300f);

    public static float GetOpeningTitleStripHeight() => Tier(56f, 48f, 56f, 56f);

    public static float GetOpeningBadgeStripHeight() => Tier(28f, 26f, 34f, 34f);
}
