using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace GrabCoin.Services.Chat.VoiceChat
{
    [Serializable]
    public class VoiceRecorder
    {
        private MicWrapper _inputSource;
        public const SamplingRate _samplingRate = SamplingRate.Sampling20000;

        private static VoiceRecorder _instance;
        public static List<MicRef> micOptions = new();
        public static List<string> micOptionsStrings = new();
        private static DeviceInfo microphoneDevice = DeviceInfo.Default;

        public static DeviceInfo MicrophoneDevice
        {
            get
            {
                return microphoneDevice;
            }
            set
            {
                if (microphoneDevice != value)
                {
                    microphoneDevice = value;
                    Debug.Log("Recorder.MicrophoneDevice changed");
                    _instance.RestartRecording(_instance._lengthSec, _instance._micSamplePacketSize);
                }
            }
        }

        public event Action<float[]> OnRecorded;
        private AudioClip microphoneClip;
        private int lastSample;
        //public const int SampleRate = 20000;
        private int _micSamplePacketSize = 2250;
        private int _lengthSec;

        public VoiceRecorder(string[] micDev)
        {
            UnityMicrophone.Initialize(micDev);
            micOptions.Clear();
            micOptionsStrings.Clear();
            DeviceEnumeratorBase unityMicEnum = new AudioInEnumerator();
            foreach (var d in unityMicEnum)
            {
                micOptions.Add(new MicRef(d));
                micOptionsStrings.Add(string.Format("[Unity]\u00A0{0}", d));
            }
            _instance = this;
        }

        public void CreateLocalVoiceAudioAndSource(int lengthSec, int micSamplePacketSize)
        {
            _micSamplePacketSize = micSamplePacketSize;
            _lengthSec = lengthSec;
            buffer = new float[micSamplePacketSize];
            _inputSource = new MicWrapper(MicrophoneDevice.IDString, (int)_samplingRate, lengthSec);
        }

        public void RestartRecording(int lengthSec, int micSamplePacketSize)
        {
            _inputSource.Dispose();
            CreateLocalVoiceAudioAndSource(lengthSec, micSamplePacketSize);
        }

        private float[] buffer;
        public void Record()
        {
            if (_inputSource.Read(buffer))
                OnRecorded?.Invoke(buffer);

            //int microphonePosition = GetRecordPosition();
            //if (microphonePosition < 1)
            //    return;
            //int difference = microphonePosition - lastSample;

            //if (difference >= micSamplePacketSize)
            //{
            //    float[] floatData = ReadSample(microphoneClip, difference);
            //    OnRecorded?.Invoke(floatData);
            //    lastSample = microphonePosition;
            //}
        }
    }

    public enum SamplingRate : int
    {
        Sampling08000 = 8000,
        Sampling12000 = 12000,
        Sampling16000 = 16000,
        Sampling20000 = 20000,
        Sampling24000 = 24000,
        Sampling48000 = 48000
    }

    public static class UnityMicrophone
    {
#if NO_MICROPHONE_API
        private const string webglIsnotSupported = "Unity Microphone not supported on WebGL";
        private static readonly string[] _devices = new string[0];
#else
        private static string[] _microphoneDevices;
#endif

        public static void Initialize (string[] micDev)
        {
            _microphoneDevices = micDev;
        }

        public static string[] devices
        {
            get
            {
#if NO_MICROPHONE_API
                return _devices;
#else
                return _microphoneDevices; // Microphone.devices;
#endif
            }
        }

        public static void End(string deviceName)
        {
#if NO_MICROPHONE_API
            throw new NotImplementedException(webglIsnotSupported);
#else
            Microphone.End(deviceName);
#endif
        }

        public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq)
        {
#if NO_MICROPHONE_API
            throw new NotImplementedException(webglIsnotSupported);
#else
            Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
#endif
        }

        public static int GetPosition(string deviceName)
        {
#if NO_MICROPHONE_API
            throw new NotImplementedException(webglIsnotSupported);
#else
            return Microphone.GetPosition(deviceName);
#endif
        }

        public static bool IsRecording(string deviceName)
        {
#if NO_MICROPHONE_API
            return false;
#else
            return Microphone.IsRecording(deviceName);
#endif
        }

        public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
        {
#if NO_MICROPHONE_API
            throw new NotImplementedException(webglIsnotSupported);
#else
            return Microphone.Start(deviceName, loop, lengthSec, frequency);
#endif
        }

        public static string CheckDevice(/*ILogger logger, */string logPref, string device, int suggestedFrequency, out int frequency)
        {
#if NO_MICROPHONE_API
            logger.LogError(logPref + webglIsnotSupported);
            frequency = 0;
            return webglIsnotSupported;
#else
            if (Microphone.devices.Length < 1)
            {
                var err = "No microphones found (Microphone.devices is empty)";
                Debug.LogError(logPref + err);
                frequency = 0;
                return err;
            }
            //if (!string.IsNullOrEmpty(device) && !Microphone.devices.Contains(device))
            //{
            //    var err = string.Format("[PV] MicWrapper: \"{0}\" is not a valid Unity microphone device, falling back to default one", device);
            //    Debug.LogError(logPref + err);
            //    frequency = 0;
            //    return err;
            //}
            int minFreq;
            int maxFreq;
            Debug.Log(string.Format("[PV] MicWrapper: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency));
            Microphone.GetDeviceCaps(Microphone.devices[PlayerPrefs.GetInt("microphoneDevices", 0)], out minFreq, out maxFreq);
            frequency = suggestedFrequency;

            //        minFreq = maxFreq = 44100; // test like android client
            if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5)
            {
                if (suggestedFrequency != minFreq && suggestedFrequency != maxFreq)
                {
                    int setFrequency = suggestedFrequency <= minFreq ? minFreq : maxFreq;
                    Debug.LogWarning(logPref + string.Format("microphone does not support suggested frequency {0} (supported frequencies are: {1} and {2}). Setting to {3}",
                        suggestedFrequency, minFreq, maxFreq, setFrequency));
                    frequency = setFrequency;
                }
            }
            else
            {
                if (suggestedFrequency < minFreq || maxFreq != 0 && suggestedFrequency > maxFreq)
                {
                    Debug.LogWarning(logPref + string.Format("microphone does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}",
                        suggestedFrequency, minFreq, maxFreq));
                    frequency = maxFreq;
                }
            }
            frequency = 20000;
            return null;
#endif
        }
    }

    public class MicWrapper
    {
        private AudioClip mic;
        private string device;

        public MicWrapper(string device, int suggestedFrequency, int lengthSec)
        {
            try
            {
                this.device = device;

                int frequency;
                this.Error = UnityMicrophone.CheckDevice("[PV] MicWrapper: ", device, suggestedFrequency, out frequency);
                if (this.Error != null)
                {
                    return;
                }

                this.mic = UnityMicrophone.Start(device, true, lengthSec, frequency);
                Debug.Log(string.Format("[PV] MicWrapper: microphone '{0}' initialized, frequency = {1}, channels = {2}.", device, this.mic.frequency, this.mic.channels));
            }
            catch (Exception e)
            {
                Error = e.ToString();
                if (Error == null) // should never happen but since Error used as validity flag, make sure that it's not null
                {
                    Error = "Exception in MicWrapper constructor";
                }
                Debug.LogError("[PV] MicWrapper: " + Error);
            }
        }

        public int SamplingRate { get { return Error == null ? this.mic.frequency : 0; } }
        public int Channels { get { return Error == null ? this.mic.channels : 0; } }
        public string Error { get; private set; }

        public void Dispose()
        {
            UnityMicrophone.End(this.device);
        }

        private int micPrevPos;
        private int micLoopCnt;
        private int readAbsPos;

        public bool Read(float[] buffer)
        {
            if (Error != null)
            {
                return false;
            }
            int micPos = UnityMicrophone.GetPosition(device);
            // loop detection
            if (micPos < micPrevPos)
            {
                micLoopCnt++;
            }
            micPrevPos = micPos;

            var micAbsPos = micLoopCnt * this.mic.samples + micPos;

            if (mic.channels == 0)
            {
                Error = "Number of channels is 0 in Read()";
                Debug.LogError("[PV] MicWrapper: " + Error);
                return false;
            }
            var bufferSamplesCount = buffer.Length / mic.channels;

            var nextReadPos = readAbsPos + bufferSamplesCount;
            if (nextReadPos < micAbsPos)
            {
                mic.GetData(buffer, readAbsPos % mic.samples);
                readAbsPos = nextReadPos;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public struct DeviceInfo
    {
        // used internally for Default property creation
        private DeviceInfo(bool isDefault, int idInt, string idString, string name, DeviceFeatures features = null)
        {
            IsDefault = isDefault;
            IDInt = idInt;
            IDString = idString;
            Name = name;
            useStringID = false;
            this.features = features;
        }

        // numeric id
        public DeviceInfo(int id, string name, DeviceFeatures features = null)
        {
            IsDefault = false;
            IDInt = id;
            IDString = "";
            Name = name;
            useStringID = false;
            this.features = features;
        }

        // string id
        public DeviceInfo(string id, string name, DeviceFeatures features = null)
        {
            IsDefault = false;
            IDInt = 0;
            IDString = id;
            Name = name;
            useStringID = true;
            this.features = features;
        }

        // name is id (Unity Microphone and WebCamTexture APIs)
        public DeviceInfo(string name, DeviceFeatures features = null)
        {
            IsDefault = false;
            IDInt = 0;
            IDString = name;
            Name = name;
            useStringID = true;
            this.features = features;
        }

        public bool IsDefault { get; private set; }
        public int IDInt { get; private set; }
        public string IDString { get; private set; }
        public string Name { get; private set; }
        public DeviceFeatures Features => features == null ? DeviceFeatures.Default : features;
        DeviceFeatures features;

        private bool useStringID;

        public static bool operator ==(DeviceInfo d1, DeviceInfo d2)
        {
            return d1.Equals(d2);
        }

        public static bool operator !=(DeviceInfo d1, DeviceInfo d2)
        {
            return !d1.Equals(d2);
        }

        // trivial implementation to avoid warnings CS0660 and CS0661 about missing overrides when == and != defined
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (useStringID)
            {
                return (Name == null ? "" : Name) + (IDString == null || IDString == Name ? "" : " (" + IDString.Substring(0, Math.Min(10, IDString.Length)) + ")");
            }
            else
            {
                return string.Format("{0} ({1})", Name, IDInt);
            }
        }

        // default device id may differ on different platform, use this platform value instead of Default.Int
        public static readonly DeviceInfo Default = new DeviceInfo(true, -128, "", "[Default]");
    }

    public class DeviceFeatures
    {
        public DeviceFeatures()
        {
        }
        public DeviceFeatures(CameraFacing facing)
        {
            CameraFacing = facing;
        }
        public CameraFacing CameraFacing { get; private set; }

        static internal DeviceFeatures Default = new DeviceFeatures();
    }

    public abstract class DeviceEnumeratorBase : IDisposable, IEnumerable<DeviceInfo>
    {
        protected List<DeviceInfo> devices = new List<DeviceInfo>();

        public virtual bool IsSupported => true;

        public virtual string Error { get; protected set; }

        public IEnumerator<DeviceInfo> GetEnumerator()
        {
            return (devices == null ? System.Linq.Enumerable.Empty<DeviceInfo>() : devices).GetEnumerator();
        }

        public abstract void Refresh();

        // if the enum has already the list, return it as soon as the callback ist set
        public Action OnReady
        {
            protected get
            {
                return onReady;
            }
            set
            {
                onReady = value;

                if (devices != null && onReady != null)
                {
                    onReady();
                }
            }
        }

        private Action onReady;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public abstract void Dispose();
    }

    public class AudioInEnumerator : DeviceEnumeratorBase
    {
        public AudioInEnumerator() : base()
        {
            Refresh();
        }

        public override void Refresh()
        {
            var unityDevs = UnityMicrophone.devices;
            devices = new List<DeviceInfo>();
            for (int i = 0; i < unityDevs.Length; i++)
            {
                var d = unityDevs[i];
                devices.Add(new DeviceInfo(d));
            }

            if (OnReady != null)
            {
                OnReady();
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public override bool IsSupported => false;

        public override string Error { get { return "Current platform " + Application.platform + " is not supported by AudioInEnumerator."; } }
#else
        public override string Error { get { return null; } }
#endif

        public override void Dispose()
        {
        }
    }

    public enum CameraFacing
    {
        Undef,
        Front,
        Back,
    }

    public struct MicRef
    {
        public readonly DeviceInfo Device;

        public MicRef(DeviceInfo device)
        {
            this.Device = device;
        }

        public override string ToString()
        {
            return string.Format("Mic reference: {0}", this.Device.Name);
        }
    }

    public interface ObjectFactory<TType, TInfo> : IDisposable
    {
        TInfo Info { get; }
        TType New();
        TType New(TInfo info);
        void Free(TType obj);
        void Free(TType obj, TInfo info);
    }

    public abstract class ObjectPool<TType, TInfo> : IDisposable
    {
        protected int capacity;
        protected TInfo info;
        private TType[] freeObj = new TType[0];
        protected int pos;
        protected string name;
        private bool inited;
        abstract protected TType createObject(TInfo info);
        abstract protected void destroyObject(TType obj);
        abstract protected bool infosMatch(TInfo i0, TInfo i1);
        internal string LogPrefix { get { return "[ObjectPool] [" + name + "]"; } }

        /// <summary>Create a new ObjectPool instance. Does not call Init().</summary>
        /// <param name="capacity">Capacity (size) of the object pool.</param>
        /// <param name="name">Name of the object pool.</param>
        public ObjectPool(int capacity, string name)
        {
            this.capacity = capacity;
            this.name = name;
        }

        /// <summary>Create a new ObjectPool instance with the given info structure. Calls Init().</summary>
        /// <param name="capacity">Capacity (size) of the object pool.</param>
        /// <param name="name">Name of the object pool.</param>
        /// <param name="info">Info about this Pool's objects.</param>
        public ObjectPool(int capacity, string name, TInfo info)
        {
            this.capacity = capacity;
            this.name = name;
            Init(info);
        }

        /// <summary>(Re-)Initializes this ObjectPool.</summary>
        /// If there are objects available in this Pool, they will be destroyed.
        /// Allocates (Capacity) new Objects.
        /// <param name="info">Info about this Pool's objects.</param>
        public void Init(TInfo info)
        {
            lock (this)
            {
                while (pos > 0)
                {
                    destroyObject(freeObj[--pos]);
                }
                this.info = info;
                this.freeObj = new TType[capacity];
                inited = true;
            }
        }

        /// <summary>The property (info) that objects in this Pool must match.</summary>
        public TInfo Info
        {
            get { return info; }
        }

        /// <summary>Acquire an existing object, or create a new one if none are available.</summary>
        /// <remarks>If it fails to get one from the pool, this will create from the info given in this pool's constructor.</remarks>
        public TType AcquireOrCreate()
        {
            lock (this)
            {
                if (pos > 0)
                {
                    return freeObj[--pos];
                }
                if (!inited)
                {
                    throw new Exception(LogPrefix + " not initialized");
                }
            }
            return createObject(this.info);
        }

        /// <summary>Acquire an existing object (if info matches), or create a new one from the passed info.</summary>
        /// <param name="info">Info structure to match, or create a new object with.</param>
        public TType AcquireOrCreate(TInfo info)
        {
            // TODO: this.info thread safety
            if (!infosMatch(this.info, info))
            {
                Init(info);
            }
            return AcquireOrCreate();
        }

        /// <summary>Returns object to pool.</summary>
        /// <param name="obj">The object to return to the pool.</param>
        /// <param name="objInfo">The info structure about obj.</param>
        /// <remarks>obj is returned to the pool only if objInfo matches this pool's info. Else, it is destroyed.</remarks>
        virtual public bool Release(TType obj, TInfo objInfo)
        {
            // TODO: this.info thread safety
            if (infosMatch(this.info, objInfo))
            {
                lock (this)
                {
                    if (pos < freeObj.Length)
                    {
                        freeObj[pos++] = obj;
                        return true;
                    }
                }
            }

            // destroy if can't reuse
            //UnityEngine.Debug.Log(LogPrefix + " Release(Info) destroy");
            destroyObject(obj);
            // TODO: log warning
            return false;
        }

        /// <summary>Returns object to pool, or destroys it if the pool is full.</summary>
        /// <param name="obj">The object to return to the pool.</param>
        virtual public bool Release(TType obj)
        {
            lock (this)
            {
                if (pos < freeObj.Length)
                {
                    freeObj[pos++] = obj;
                    return true;
                }
            }

            // destroy if can't reuse
            //UnityEngine.Debug.Log(LogPrefix + " Release destroy " + pos);
            destroyObject(obj);
            // TODO: log warning
            return false;
        }

        /// <summary>Free resources assoicated with this ObjectPool</summary>
        public void Dispose()
        {
            lock (this)
            {
                while (pos > 0)
                {
                    destroyObject(freeObj[--pos]);
                }
                freeObj = new TType[0];
            }
        }
    }

    public class PrimitiveArrayPool<T> : ObjectPool<T[], int>
    {
        public PrimitiveArrayPool(int capacity, string name) : base(capacity, name) { }
        public PrimitiveArrayPool(int capacity, string name, int info) : base(capacity, name, info) { }
        protected override T[] createObject(int info)
        {
            //UnityEngine.Debug.Log(LogPrefix + " Create " + pos);
            return new T[info];
        }

        protected override void destroyObject(T[] obj)
        {
            //UnityEngine.Debug.Log(LogPrefix + " Dispose " + pos + " " + obj.GetHashCode());
        }

        protected override bool infosMatch(int i0, int i1)
        {
            return i0 == i1;
        }
    }

    public class FactoryPrimitiveArrayPool<T> : ObjectFactory<T[], int>
    {
        PrimitiveArrayPool<T> pool;
        public FactoryPrimitiveArrayPool(int capacity, string name)
        {
            pool = new PrimitiveArrayPool<T>(capacity, name);
        }

        public FactoryPrimitiveArrayPool(int capacity, string name, int info)
        {
            pool = new PrimitiveArrayPool<T>(capacity, name, info);
        }

        public int Info { get { return pool.Info; } }

        public T[] New()
        {
            return pool.AcquireOrCreate();
        }

        public T[] New(int size)
        {
            return pool.AcquireOrCreate(size);
        }

        public void Free(T[] obj)
        {
            pool.Release(obj);
        }

        public void Free(T[] obj, int info)
        {
            pool.Release(obj, info);
        }

        public void Dispose()
        {
            pool.Dispose();
        }
    }
}
