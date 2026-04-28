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

    // ---- Canvas sorting orders ----
    // Base HUD layers
    public const int SortOrderHintStrip = 6;
    public const int SortOrderRequiredItems = 8;
    public const int SortOrderGuide = 10;
    public const int SortOrderTimer = 12;
    public const int SortOrderInventory = 15;

    // Popup / modal layers (always above base HUD)
    public const int SortOrderFeedbackPopup = 205;
    public const int SortOrderMissionAlertPopup = 206;
    public const int SortOrderStoryMomentPopup = 210;
    public const int SortOrderHubConfirmPopup = 212;
    public const int SortOrderTimeUpPopup = 214;
    public const int SortOrderSavePopup = 215;
    // Tips panel must be above all HUD elements including timer (12) and hub button
    public const int SortOrderTipsBackdrop = 300;
    public const int SortOrderTipsPopup = 301;

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

    // Tips panel typography
    public const float TipsTitleSize   = 32f;
    public const float TipsContentSize = 22f;

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
    // ---- Bottom panel anchors (left assembly + right required items) ----
    // =========================================================================

    public static float GetLeftBottomPanelAnchorMaxX()  => Tier(0.48f, 0.50f, 0.46f, 0.45f);
    public static float GetRightBottomPanelAnchorMinX() => Tier(0.52f, 0.50f, 0.54f, 0.55f);

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

    public static float GetHintStripHeight()          => Tier( 44f,  36f, 42f, 40f);
    public static float GetHintStripMinimizedHeight() => Tier( 16f,  12f, 14f, 14f);
    public static int   GetHintFontSize()             => Tier(  24,   22,  22,  22);
    public static float GetHintMiddleSectionEnd()     => Tier(0.60f, 0.60f, 0.60f, 0.60f);

    /// <summary>Height of the contextual-hints strip that sits between the guide panel and the timer widgets.</summary>
    public static float GetInfoStripHeight() => GetHintStripHeight();

    /// <summary>Bottom edge Y (canvas units from screen top) of the info/hints strip: use as the Y anchor for the timer and hub button.</summary>
    public static float GetInfoStripBottomY() => GetGuideBarBottomY() + GetInfoStripHeight();

    // =========================================================================
    // ---- Shared side-panel height (Assembly + Required Items) ----
    // =========================================================================
    //
    // Enlarged so both side-panels reveal more of their contents by default
    // and remain readable across all screen categories.
    public static float GetSidePanelHeight() => Tier(330f, 240f, 360f, 340f);

    // =========================================================================
    // ---- Required items panel ----
    //
    //  Panel is enlarged so required-item rows stay readable at every size.
    //  Portrait phone effective canvas width ≈ 499 units (iPhone 13).
    //  Assembly right-edge (width 160): 14 + 160 = 174 units.
    //  Required Items left-edge must be > 174 to avoid overlap.
    //  With width 205: left = 499 - 16 - 205 = 278 → gap ≈ 104 units. ✓
    // =========================================================================

    public static float GetRequiredPanelWidthExpanded()
    {
        switch (GetScreenCategory())
        {
            case ScreenCategory.Phone:
                float canvasW = Screen.width * RefHeight / Screen.height;
                return Mathf.Min(205f, canvasW * 0.40f);
            case ScreenCategory.PhoneLandscape:
                return 500f;
            case ScreenCategory.Tablet:
                return 380f;
            default:
                return 360f;
        }
    }

    public static float GetRequiredTitleBarHeight() => Tier( 58f,  58f,  60f,  56f);
    public static float GetRequiredItemRowHeight()  => Tier( 58f,  56f,  60f,  56f);
    public static float GetRequiredIconSize()       => Tier( 48f,  48f,  52f,  48f);
    public static float GetRequiredNpcTextHeight()  => Tier( 36f,  36f,  32f,  32f);

    public static int GetRequiredTitleFontSize()    => Tier(22, 22, 22, 22);
    public static int GetRequiredItemFontSize()     => Tier(20, 20, 20, 20);
    public static int GetRequiredSubtitleFontSize() => Tier(20, 20, 20, 20);

    // =========================================================================
    // ---- Assembly visualizer (bottom-left spacecraft panel) ----
    //
    // Wider and taller than before so the spacecraft sprite and step label
    // stay visible and legible on every screen category.
    // =========================================================================

    public static float GetAssemblyPanelWidth()      => Tier(170f, 420f, 280f, 260f);
    public static float GetAssemblyPanelHeight()     => GetSidePanelHeight();
    public static float GetAssemblyTitleBarHeight()  => Tier( 52f,  56f,  52f,  50f);
    public static int   GetAssemblyTitleFont()       => Tier(  22,   22,   22,   22);

    // Step font: matches body text size for consistency across all panels.
    public static int   GetAssemblyStepFont()        => Tier(  20,   20,   20,   20);

    // =========================================================================
    // ---- Timer / hub widgets (top-right) ----
    // =========================================================================

    public static float GetTimerWidgetWidth()   => Tier(176f, 186f, 188f, 192f);
    // Match timer widget width so "Central Hub" and "MISSION TIME" panels are visually uniform.
    public static float GetHubWidgetWidth()     => GetTimerWidgetWidth();
    public static float GetTimerWidgetHeight()  => Tier( 52f,  56f,  58f,  60f);
    public static float GetTimerDigitsFontSize()=> Tier( 24f,  26f,  28f,  30f);
    public static float GetTimerCaptionFontSize()=> Tier(13f, 13f, 14f, 14f);
    public static float GetHubIconFontSize()    => Tier( 22f,  24f,  26f,  28f);
    public static float GetHubLabelFontSize()   => GetTimerCaptionFontSize();

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
    //  Phone portrait  (180 px): larger fonts + centered objective; room for wrapped lines.
    //  Phone landscape (108 px): same intent in short vertical space.
    // =========================================================================

    public static float GetGuidePanelHeight()       => Tier(125f,  78f, 105f, 100f);
    public static float GetGuideTitleBarHeight()    => Tier( 36f,  30f,  34f,  34f);
    public static float GetGuideStepTitleFontSize() => Tier( 22f,  20f,  22f,  22f);
    public static float GetGuideObjectiveFontSize() => Tier( 24f,  22f,  22f,  22f);
    public static float GetGuideCaptionFontSize()   => Tier( 16f,  14f,  16f,  16f);

    /// <summary>Extra buffer below the safe-area top inset so the guide panel always clears Dynamic Island / notch in the simulator.</summary>
    public static float GetGuideExtraTopPadding() => Tier(8f, 4f, 0f, 0f);

    /// <summary>Bottom edge of the guide panel in canvas units from the screen top: use as the Y anchor for widgets that sit just below the guide strip.</summary>
    public static float GetGuideBarBottomY() => GetSafeAreaTopOffset() + GetGuideExtraTopPadding() + GetGuidePanelHeight();

    /// <summary>Height for the full-width bottom HUD panels (Assembly + Required Items) that span 0–45 % and 55–100 % of the screen width.</summary>
    public static float GetBottomPanelHeight() => Tier(260f, 200f, 300f, 280f);

    /// <summary>Bottom Y (canvas units from screen bottom) for feedback popups: sits above the bottom HUD panels.</summary>
    public static float GetPopupBottomOffset() => GetBottomPanelHeight() + 30f;

    // ---- Guide Y offsets (top-anchored, negative = downward) ----
    // Step title zone + objective zone only (talkTo / controls rows removed).

    public static float GetGuideStepTitleY()      => -6f;
    public static float GetGuideStepTitleBottom() => Tier(-32f, -24f, -30f, -28f);
    public static float GetGuideObjectiveY()      => Tier(-36f, -28f, -34f, -32f);
    public static float GetGuideObjectiveBottom() => Tier(-102f, -58f, -88f, -82f);

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
    // ---- NPC dialogue panel (conversation overlay) ----
    //
    //  Sits centred horizontally and slightly above the vertical centre of
    //  the screen. Sized so it covers the name, portrait and wrapped body
    //  text on every tier. Widths are clamped against the effective canvas
    //  width on narrow screens so the panel never overflows.
    // =========================================================================

    public static Vector2 GetDialoguePanelSize()
    {
        switch (GetScreenCategory())
        {
            case ScreenCategory.Phone:
            {
                // Portrait phones: compute effective canvas width using height-match
                // scaling and let the panel take up ~94 % of that width.
                float canvasW = Screen.width * RefHeight / Mathf.Max(Screen.height, 1);
                float w = Mathf.Min(820f, Mathf.Max(500f, canvasW * 0.94f));
                return new Vector2(w, 340f);
            }
            case ScreenCategory.PhoneLandscape:
                return new Vector2(1300f, 320f);
            case ScreenCategory.Tablet:
                return new Vector2(1140f, 400f);
            default: // Desktop
                return new Vector2(1200f, 420f);
        }
    }

    public static float GetDialoguePortraitSize() => Tier(120f, 150f, 170f, 190f);
    public static int   GetDialogueNameFontSize() => Tier(   22,   22,   22,   22);
    public static int   GetDialogueBodyFontSize() => Tier(   22,   22,   22,   22);
    public static float GetDialoguePadding()      => Tier( 26f,  24f,  30f,  32f);

    /// <summary>Name-row reserved height above the wrapped body text.</summary>
    public static float GetDialogueNameRowHeight() => GetDialogueNameFontSize() * 1.5f;

    /// <summary>Close-button square side and its label font size.</summary>
    public static float GetDialogueCloseSize()     => Tier( 52f,  48f,  56f,  58f);
    public static int   GetDialogueCloseFontSize() => Tier(   26,   24,   28,   28);

    /// <summary>
    /// Positive Y offset (canvas units) so the panel centre lands above the
    /// vertical centre of the screen. Kept modest so the dialogue feels more
    /// centered while still clearing the player/HUD area.
    /// </summary>
    public static float GetDialogueYOffsetAboveCenter() => 0f;

    public static Color GetDialogueBackdropColor() => new Color(0f, 0f, 0f, 0.86f);

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

    // =========================================================================
    // ---- Tips overlay panel ----
    // =========================================================================

    public static float GetTipsTitleFontSize()   => Tier(26f, 24f, 30f, 32f);
    public static float GetTipsContentFontSize() => Tier(15f, 14f, 19f, 20f);
    public static float GetTipsCloseButtonSize() => Tier(36f, 34f, 40f, 44f);
    public static float GetTipsLineSpacing()      => Tier(1.28f, 1.22f, 1.30f, 1.32f);

    public static float GetTipsHorizontalInset()  => Tier(0.02f, 0.03f, 0.08f, 0.12f);
    public static float GetTipsBottomPadding()    => Tier(12f, 10f, 16f, 20f);
    public static float GetTipsTopGap()           => Tier(4f, 4f, 6f, 8f);
}
