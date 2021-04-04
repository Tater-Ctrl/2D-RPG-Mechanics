using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private Text tooltip;
    void Start() {
        tooltip = GetComponentInChildren<Text>();
        
    }
    /// <summary>
    /// Opens up a tooltip in 1 second if not cancelled before
    /// </summary>
    public bool GenerateTooltip(Item item) {
        Invoke("DisplayTooltip", 1f);
        string tooltipText = string.Format("{0}\n\n<b>DESCRIPTION:\n{1}\n\nVALUE: {2}\n\nWEIGHT: {3}",
                                                item.itemName.ToUpper(), item.itemDescription.ToUpper(), item.value, item.weight);
        tooltip.text = tooltipText;
        
        return true;
    }

    void DisplayTooltip() {
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Cancels the tooltip timer if not open, and hides itself
    /// </summary>
    public bool HideTooltip() {
        this.gameObject.SetActive(false);
        CancelInvoke();
        return false;
    }
}
