using UnityEngine;

namespace GrabCoin.Model
{
    public class UserModelLocal : UserModel
    {
        private const string AGREEMENT_READED_KEY = "agreement_readed_key";


        public override bool AgreementAccepted => PlayerPrefs.GetInt(AGREEMENT_READED_KEY, 0) > 0;

        public override void AcceptAgreement() => PlayerPrefs.SetInt(AGREEMENT_READED_KEY, 1);

    }
}