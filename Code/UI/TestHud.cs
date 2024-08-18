using UnityEngine;
using TMPro; // TODO: Work with HUD not directly changing the text field
using UnityEngine.UI;

namespace GrabCoin.UI.HUD
{

    public class TestHud : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private MiniMapController _miniMapController;
        [SerializeField] private Texture _mapTexture;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _testObject;
        [SerializeField] private Rect _worldRect;
        [SerializeField] private TextMeshProUGUI _txtPlayersNum;
        [SerializeField] private TextMeshProUGUI _txtQuality;
        [SerializeField] private TMP_Text _helperInfoText;
        [SerializeField] private TMP_Text _ammoInfoText;
        [SerializeField] private Image _aimCross;
        [SerializeField] private GameObject _shield;
        [SerializeField] private Image _shieldBar;
        [SerializeField] private Image _healthBar;
        [SerializeField] private Image _staminaBar;

        private CustomEvent _customEvent;


        // TODO: implement this workaround via HUDProtocol events
        // WORKAROUND START
        private int _quality = -1;
        void Update ()
        {
            int newQuality = QualitySettings.GetQualityLevel();
            if (newQuality != _quality)
            {
                _quality = newQuality;
                OnQualityChanged();
            }
        }
        // WORKAROUND FINISH

        private void OnEnable()
        {
            _customEvent = OnPlayersNumberChanged;
            Translator.Add<HUDProtocol>(_customEvent);
        }

        private void OnDisable()
        {
            Translator.Remove<HUDProtocol>(_customEvent);
        }

        public void SetupPlayer(Transform player)
        {
            _playerTransform = player;
        }

        public void OnPlayersNumberChanged (System.Enum code, ISendData data)
        {
            switch (code)
            {
                case HUDProtocol.ChangePlayerCount:
                    var playersNum = (IntData)data;
                    _txtPlayersNum.text = $"Players online: {playersNum.value}";
                    break;
                case HUDProtocol.HelperInfo:
                    var info = (StringData)data;
                    _helperInfoText.text = info.value;
                    break;
                case HUDProtocol.CountBullet:
                    var count = (StringData)data;
                    _ammoInfoText.text = count.value;
                    break;
                case HUDProtocol.AimCross:
                    var isActive = (BoolData)data;
                    _aimCross.gameObject.SetActive(isActive.value);
                    break;
                case HUDProtocol.ChangedHealth:
                    _healthBar.fillAmount = ((FloatData)data).value;
                    break;
                case HUDProtocol.ChangedStamina:
                    _staminaBar.fillAmount = ((FloatData)data).value;
                    break;
                case HUDProtocol.ChangedShield:
                    _shieldBar.fillAmount = ((FloatData)data).value;
                    break;
                case HUDProtocol.EnableShield:
                    _shield.SetActive(((BoolData)data).value);
                    break;
            }
        }

        public void OnQualityChanged ()
        {
            _txtQuality.text = $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}";
        }
    }
}
