#if UNITY_EDITOR
#endif
using System.Collections;
using UnityEngine;

namespace GrabCoin.AsyncProcesses
{
    public class SetTokenBalanceData : DataClass
    {
        public static string secretKey = "";

        public SetTokenBalanceData(string playerId, string tokenId, decimal delta)
        {
            url = GetURL() + $"update-balance";
            requestType = RequestType.POST;
            form = new WWWForm();
            form.AddField("tokenName", tokenId);
            form.AddField("userId", playerId);
            form.AddField("deltaBalance", delta.ToString().Replace(",", "."));
            StringData hash = Translator.SendOneAnswer<GeneralProtocol, ISendData, StringData>(GeneralProtocol.GetHash, new StringData { value = $"{tokenId}&{playerId}&{delta.ToString().Replace(",", ".")}&{secretKey}" });
            form.AddField("hash", hash.value);
        }

        internal override IEnumerator CheckFreshToken()
        {
            yield break;
        }
    }
}