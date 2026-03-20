using TMPro;
using UnityEngine;

public class FontSetter : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_FontAsset defaultFont;
    [SerializeField] private TMPro.TMP_FontAsset accessibleFont;

    [SerializeField] private TextMeshProUGUI[] textElements;
    [SerializeField] private int fontSizeDifference = 30;

    private const string PREF_FONT_KEY = "AccessibleFont";

    private bool useAccessibleFont = false;
    private bool fontChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        useAccessibleFont = PlayerPrefs.GetInt(PREF_FONT_KEY, 0) == 1;
        if (useAccessibleFont) { 
            foreach (TextMeshProUGUI text in textElements)
            {
                text.font = accessibleFont;
                text.fontSize -= fontSizeDifference;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt(PREF_FONT_KEY, 0) == 1 && !useAccessibleFont)
        {
            useAccessibleFont = true;
            fontChanged = true;
        }
        else if (PlayerPrefs.GetInt(PREF_FONT_KEY, 0) == 0 && useAccessibleFont)
        {
            useAccessibleFont = false;
            fontChanged = true;
        }
        if (fontChanged)
        {
            foreach (TextMeshProUGUI text in textElements)
            {
                text.font = useAccessibleFont ? accessibleFont : defaultFont;
                text.fontSize = useAccessibleFont ? text.fontSize - fontSizeDifference : text.fontSize + fontSizeDifference;
            }
            fontChanged = false;
        }
    }
}
