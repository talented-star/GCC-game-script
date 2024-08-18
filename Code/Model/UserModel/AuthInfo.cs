using System;

namespace GrabCoin.Model
{
    [Serializable]
    public struct AuthInfo
    {
        private string _signMessage;
        private string _signSignature;
        private string _signAccount;

        public string SignMessage => _signMessage;
        public string SignSignature => _signSignature;
        public string SignAccount => _signAccount;

        public AuthInfo(string signMessage, string signSignature)
        {
            _signMessage = signMessage;
            _signSignature = signSignature;
            _signAccount = "";
        }

        public AuthInfo(string signMessage, string signSignature, string signAccount)
        {
            _signMessage = signMessage;
            _signSignature = signSignature;
            _signAccount = signAccount;
        }

        public AuthInfo(AuthInfo authInfo, string signAccount)
        {
            _signMessage = authInfo.SignMessage;
            _signSignature = authInfo.SignSignature;
            _signAccount = signAccount;
        }
    }
}