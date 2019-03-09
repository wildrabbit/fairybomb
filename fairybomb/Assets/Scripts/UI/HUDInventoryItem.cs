using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDInventoryItem : MonoBehaviour
{
    [SerializeField] RectTransform _selection;
    [SerializeField] RectTransform _itemXfm;

    [SerializeField] Image _icon;
    [SerializeField] TextMeshProUGUI _amount;
    [SerializeField] TextMeshProUGUI _hotkey;
    [SerializeField] TextMeshProUGUI _name;

    const string kInfinityString = "<sprite name=infinity>";
    const string kAmountString = "x{0}";

    public void Init(int idx)
    {
        _hotkey.text = (idx + 1).ToString();
    }

    internal void SetEntry(BombInventoryEntry entry)
    {
        bool active = entry != null;
        _itemXfm.gameObject.SetActive(active);
        if(!active)
        {
            return;
        }

        _icon.sprite = entry.Bomb.UIIcon;
        _name.text = entry.Bomb.DisplayName;
        SetAmount(entry.Unlimited, entry.Amount);
    }

    public void SetAmount(bool unlimited, int amount)
    {
        _amount.text = unlimited ? kInfinityString : string.Format(kAmountString, amount);
    }

    internal void Deselect()
    {
        _selection.gameObject.SetActive(false);
    }

    internal void Select()
    {
        _selection.gameObject.SetActive(true);
    }

    internal void UpdateItem(BombInventoryEntry entry)
    {
        SetAmount(entry.Unlimited, entry.Amount);
    }
}
