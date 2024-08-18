#if UNITY_EDITOR
#endif
using Debug = UnityEngine.Debug;

namespace GrabCoin.AsyncProcesses
{
    public class Result
    {
        public Result()
        { }

        internal virtual void Init(string answer)
        {
#if UNITY_EDITOR
            Debug.Log(answer);

#endif
        }
    }
}