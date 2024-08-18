#if UNITY_EDITOR
#endif

namespace GrabCoin.AsyncProcesses
{
    public class SetTokenBalanceResult : Result
    {
        public class Data
        {
            public long size;
        }

        public Data sizeData;

        public SetTokenBalanceResult()
        {
        }

        internal override void Init(string answer)
        {
            base.Init(answer);
            //sizeData = JsonConvert.DeserializeObject<Data>(answer);
        }
    }
}