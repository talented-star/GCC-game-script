//using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using GrabCoin.GameWorld.Player;
using System.IO;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace GrabCoin.Services.Chat.VoiceChat
{
    [RequireComponent(typeof(VoiceSpeakerView))]
    [RequireComponent(typeof(VoiceListener))]
    public class VoicePresenter : MonoBehaviour
    {
        [SerializeField] private ThirdPersonPlayerController thirdPersonControl;
        [SerializeField] private VoiceNetwork voiceNetwork;
        [SerializeField] private int _lengthSec;
        [SerializeField] private int _micSamplePacketSize = 2250;

        private VoiceSpeakerView _voiceView;
        private VoiceListener _listener;
        public VoiceRecorder _recorder;
        [Inject] private VoiceHudView _hudView;
        private void Awake()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return;
#endif
            _voiceView = GetComponent<VoiceSpeakerView>();
            _listener = GetComponent<VoiceListener>();
            _recorder = new VoiceRecorder(Microphone.devices);
            voiceNetwork.Constructor(new AudioEncoding());
        }

        private void Start()
        { 
            if (voiceNetwork.isLocalPlayer)
            {
                _recorder.OnRecorded += OnRecorded;
            }
            voiceNetwork.ClipRecived += OnRecived;
            _recorder.CreateLocalVoiceAudioAndSource(_lengthSec, _micSamplePacketSize);
        }

        public void SetCharacter(ThirdPersonPlayerController thirdPersonControl)
        {
            this.thirdPersonControl = thirdPersonControl;
            thirdPersonControl.Talked += Talk;
            thirdPersonControl.StartTalk += StartTalk;
        }

        private void OnRecorded(float[] audioData)
        {
            voiceNetwork.Record(audioData);
        }
 
        private void StartTalk()
        {
            //_recorder.StartRecord();
            _recorder.RestartRecording(_lengthSec, _micSamplePacketSize);
            _hudView?.OnPlaying();
        }

        private void Talk()
        {
            _recorder.Record();
            _hudView?.OnPlaying();
        }

        private void OnDisable()
        {
            if (voiceNetwork.isLocalPlayer)
            {
                if (thirdPersonControl)
                {
                    thirdPersonControl.Talked -= Talk;
                    thirdPersonControl.StartTalk -= StartTalk;
                }
                _recorder.OnRecorded -= OnRecorded;
            }
            else
            {
                voiceNetwork.ClipRecived -= OnRecived;
            }
        }

        private void OnRecived(float[] audioData)
        {
            _listener.OnRecived(audioData);
            _voiceView.OnPlaying();
        }
    }
}