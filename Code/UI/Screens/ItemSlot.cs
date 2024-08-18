using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrabCoin.UI.Screens
{
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countText;

        public void Populate(Sprite icon, string count)
        {
            _icon.sprite = icon;
            _countText.text = count;
        }
    }
}