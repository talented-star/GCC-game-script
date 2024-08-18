using System;

namespace GrabCoin.Model
{
    public abstract class UserModel
    {
        public event Action OnAuthorized;

        public abstract bool AgreementAccepted { get; }
        public abstract void AcceptAgreement();

        public AuthInfo AuthInfo { get; private set; }
        public void Authorize(string signMessage, string signSignature)
        {
            AuthInfo = new AuthInfo(signMessage, signSignature);
            OnAuthorized?.Invoke();
        }
        public void AuthorizePlayFab(string playFabId)
        {
            AuthInfo = new AuthInfo(AuthInfo, playFabId);
        }
        public void UnAuthorize()
        {
            AuthInfo = new AuthInfo("", "");
        }
    }
}