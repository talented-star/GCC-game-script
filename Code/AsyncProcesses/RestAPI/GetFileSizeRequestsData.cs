#if UNITY_EDITOR
#endif
using System.Collections;

namespace GrabCoin.AsyncProcesses
{
    public class GetFileSizeRequestsData : DataClass
    {
        public GetFileSizeRequestsData(string fileName)
        {
            url = GetURL() + $"file?file={fileName}";
            requestType = RequestType.GET;
        }

        internal override IEnumerator CheckFreshToken()
        {
            yield break;
        }
    }
}