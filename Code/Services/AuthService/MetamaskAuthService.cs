using System;
using System.Text;
using System.Threading.Tasks;
using GrabCoin.Code.Common.Extensions;
using GrabCoin.Model;
using Nethereum.Signer;
using Nethereum.Util;
using PlayFab;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.UI.HUD;
using static GrabCoin.UI.HUD.ProjectNetworkContext;

namespace Code.Services.AuthService
{
    [Serializable]
    public class MetamaskAuthService : IAuthService
    {
        //[SerializeField] private PlayerState _playerState;
        private UserModel _userModel;
        private CustomIdAuthService _customIdAuthService;
        private EmailAuthService _emailAuthService;

        [Inject]
        public void Construct(/*PlayerState playerState, */UserModel userModel, CustomIdAuthService customIdAuthService, EmailAuthService emailAuthService)
        {
            //_playerState = playerState;
            _userModel = userModel;
            _customIdAuthService = customIdAuthService;
            _emailAuthService = emailAuthService;
        }
        
        public Task SignOut()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> SignIn()
        {
            var message = Guid.NewGuid().ToString();
            try
            {
                var signature = await Web3Wallet.Sign(message);
                Debug.Log($"Metamask authorized with msg={message}\n sign={signature}".ApplyColorTag("green"));
                _userModel.Authorize(message, signature);

                GetIdAndLinkMetamask();

                AuthState authState = AuthState.None;
                while (!(authState is
                    AuthState.SuccessLink or
                    AuthState.Fail or
                    AuthState.Banned or
                    AuthState.ValidateCreateAccount))
                {
                    await UniTask.NextFrame();
                    authState = ProjectNetworkContext.Instance.AuthtorizeState;
                }

                Debug.Log($"Answer server after metamask auth: {ProjectNetworkContext.Instance.AuthtorizeState}");
                if (ProjectNetworkContext.Instance.AuthtorizeState is
                    ProjectNetworkContext.AuthState.ValidateCreateAccount)
                {
                    if (!string.IsNullOrEmpty(ProjectNetworkContext.Instance.PlayFabId))
                    {
                        Debug.Log($"Save new id");
                        PlayerPrefs.SetString("AUTHENTIFICATION_KEY", ProjectNetworkContext.Instance.PlayFabId);
                    }

                    Debug.Log($"login with new id");
                    await _customIdAuthService.SignIn();

                    GetIdAndLinkMetamask();
                    authState = AuthState.None;
                    if (string.IsNullOrEmpty(ProjectNetworkContext.Instance.PlayFabId))
                    {
                        while (!(authState is
                            AuthState.SuccessLink or
                            AuthState.Fail or
                            AuthState.Banned))
                        {
                            await UniTask.NextFrame();
                            authState = ProjectNetworkContext.Instance.AuthtorizeState;
                        }
                    }
                }
                bool returnValue = ProjectNetworkContext.Instance.AuthtorizeState is
                ProjectNetworkContext.AuthState.SuccessLink or
                ProjectNetworkContext.AuthState.ValidateCreateAccount;

                if (!returnValue)
                    _userModel.UnAuthorize();

                return returnValue;
            }
            catch (Exception e)
            {
                Debug.LogError("Wallet auth exception");
                _userModel.UnAuthorize();
                return false;
            }
        }

        //private UniTask<bool> AuthToId()
        //{
        //    var completion = new UniTaskCompletionSource<bool>();

            

        //    return completion.Task;
        //}

        private void GetIdAndLinkMetamask()
        {
            var id = PlayFabSettings.staticPlayer.PlayFabId ?? "";
            Debug.Log($"Send linc with id: {id}");
            ProjectNetworkContext.Instance.LinkMetamask(id, _userModel.AuthInfo.SignMessage, _userModel.AuthInfo.SignSignature);
        }

        public void StopWaitAuth() =>
            Web3Wallet.StopWaitBuffer();

        public Task<bool> ValidateAuth()
        {
            var authInfo = _userModel.AuthInfo;
            if (string.IsNullOrEmpty(authInfo.SignMessage)) return Task.FromResult(false);

            if (ProjectNetworkContext.Instance.AuthtorizeState is
                ProjectNetworkContext.AuthState.SuccessLink)
                return Task.FromResult(true);

            if (ProjectNetworkContext.Instance.AuthtorizeState is
                ProjectNetworkContext.AuthState.Fail or
                ProjectNetworkContext.AuthState.Banned)
                return Task.FromResult(false);

            var msg = "\x19" + "Ethereum Signed Message:\n" + authInfo.SignMessage.Length + authInfo.SignMessage;
            var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            var signature = MessageSigner.ExtractEcdsaSignature(authInfo.SignSignature);
            var key = GetAddress(authInfo);
            var isValid = key.Verify(msgHash, signature);
            Debug.Log("Address Returned: " + key.GetPublicAddress());
            Debug.Log("Is Valid: " + isValid);
            return Task.FromResult(true);
        }

        private EthECKey GetAddress(AuthInfo authInfo)
        {
            var msg = "\x19" + "Ethereum Signed Message:\n" + authInfo.SignMessage.Length + authInfo.SignMessage;
            var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            var signature = MessageSigner.ExtractEcdsaSignature(authInfo.SignSignature);
            return EthECKey.RecoverFromSignature(signature, msgHash); 
        }

        public string GetAddress()
        {
            var msg = "\x19" + "Ethereum Signed Message:\n" + _userModel.AuthInfo.SignMessage.Length + _userModel.AuthInfo.SignMessage;
            var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            var signature = MessageSigner.ExtractEcdsaSignature(_userModel.AuthInfo.SignSignature);
            return EthECKey.RecoverFromSignature(signature, msgHash).GetPublicAddress(); 
        }

        public Task<AuthInfo> GetAccount()
        {
            return Task.FromResult(_userModel.AuthInfo);
        }
    }
}