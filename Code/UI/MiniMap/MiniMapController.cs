using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrabCoin.UI.HUD
{
    public class MiniMapController : MonoBehaviour
    {
        [SerializeField] private Image map;
        [SerializeField] private Image player;
        [SerializeField] private RectTransform mapMask;

        [SerializeField] private CardinalPoint[] cardinalPoints;

        private MinimapPoser poser;
        private Player _playerHandler;
        private Transform _target;
        private Camera targetCamera;

        private bool waitingPlayerSpawn;

        private bool _isVR = false;

        private async UniTaskVoid PlayerSpawned()
        {
            waitingPlayerSpawn = true;
            await UniTask.WaitUntil(() => NetworkClient.localPlayer?.gameObject != null);
            _playerHandler = NetworkClient.localPlayer.gameObject.GetComponent<Player>();
            poser = MinimapPoser.Instance;
            await UniTask.WaitUntil(() => _playerHandler.GetMode() != Enum.PlayerMode.None);
            targetCamera = _playerHandler.GetViewCamera();
            
            _isVR = _playerHandler.GetMode() == Enum.PlayerMode.VR;
            if (_isVR)
            { 
                _target = targetCamera.transform;
            }
            else
            {
                _target = _playerHandler.GetPlayer();
            }
            map.sprite = poser.GetMinimapData().map;
            waitingPlayerSpawn = false;
        }

        private void Start()
        {
            PlayerSpawned().Forget();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (waitingPlayerSpawn)
                return;
            PlayerSpawned().Forget();
        }

        private void LateUpdate()
        {
            if(_target == null)
            {
                return;
            }

            map.rectTransform.pivot = poser.GetPivot(_target.transform.position);

            float cameraRotY = targetCamera.transform.eulerAngles.y;

            // map.rectTransform.rotation = Quaternion.Euler(0, 0, cameraRotY);
            // player.rectTransform.rotation = Quaternion.Euler(0, 0, cameraRotY - _target.transform.eulerAngles.y);

            map.rectTransform.localRotation = Quaternion.Euler(0, 0, cameraRotY);
            
            if (!_isVR) 
            {
                player.rectTransform.localRotation = Quaternion.Euler(0, 0, cameraRotY - _target.transform.eulerAngles.y);
            }
            /* Not required for VR
            else
            {
                player.rectTransform.localRotation = Quaternion.Euler(0, 0, -cameraRotY);
            }
            */

            for (int i = 0; i < cardinalPoints.Length; i++)
            {
                cardinalPoints[i].point.localPosition = Quaternion.Euler(0, 0, cameraRotY + cardinalPoints[i].offset) *
                    Vector3.up * mapMask.rect.height / Mathf.Cos(Mathf.PingPong(cameraRotY + cardinalPoints[i].offset, 45f) * Mathf.Deg2Rad) / 2f;
            }
        }

    }
    [System.Serializable]
    public class CardinalPoint
    {
        public float offset = 0;
        public RectTransform point;
    }
}