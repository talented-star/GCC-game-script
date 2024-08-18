using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.UI.Screens
{
    public class RaidItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _count;

        public void Populate(Sprite icon, string name, int count)
        {
            _image.sprite = icon;
            _name.text = name;
            _count.text = count.ToString();
        }
    }
}
