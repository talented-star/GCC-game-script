#if UNITY_EDITOR
#endif
using Newtonsoft.Json;

namespace GrabCoin.AsyncProcesses
{
    public class GetUserRequestsResult : Result
    {
        public class Data
        {
            public long size;
        }

        public Data sizeData;

        public GetUserRequestsResult()
        {
        }

        internal override void Init(string answer)
        {
            base.Init(answer);
            sizeData = JsonConvert.DeserializeObject<Data>(answer);
        }
    }
}