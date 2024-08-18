using TMPro; // TODO: Work with HUD not directly changing the text field
using Mirror;
using UnityEngine;
using System.Linq;
using GrabCoin.GameWorld.Player;
using PlayFab;
#if ENABLE_PLAYFABSERVER_API
using PlayFab.ServerModels;
#endif
using GrabCoin.Model;
using Nethereum.Signer;
using Nethereum.Util;
using System.Text;
using System.Collections.Generic;
using Code.Services.AuthService;
using Zenject;

namespace GrabCoin.UI.HUD
{
    public class ProjectNetworkContext : NetworkBehaviour
    {
        private class DoubleString
        {
            public GameObject conn;
            public string message;
            public string signature;
            public string id;
            public string selfId;
        }

        public enum AuthState
        {
            None,
            SuccessLink,
            Fail,
            StartLink,
            ServerGetSelfAccount,
            GetedReadonlyData,
            ValidateCreateAccount,
            Banned
        }

        public static ProjectNetworkContext Instance { get; private set; }

        [SerializeField] private string _playfabId;
        [SerializeField] private AuthState _authtorizeState;

        private EmailAuthService _emailAuthService;

        public string PlayFabId => _playfabId;
        public AuthState AuthtorizeState => _authtorizeState;

        [Inject]
        private void Construct(EmailAuthService emailAuthService)
        {
            _emailAuthService = emailAuthService;
        }

        private void Awake()
        {
            Debug.Log("<<<<<<<<<<<<<Create ProjectNetworkContext>>>>>>>>>>>>>>");
#if !UNITY_SERVER
            //DontDestroyOnLoad(this);
#endif
            Instance = this;
        }

        public void LinkMetamask(string id, string message, string signature)
        {
            CmdLinkMetamask(FindObjectOfType<Player>(true).gameObject, id, message, signature);
        }

        [Command(requiresAuthority = false)]
        private void CmdLinkMetamask(GameObject conn, string id, string message, string signature)
        {
            Debug.Log("Get link Metamask to account");
            LinkMetamaskResult(conn, AuthState.StartLink);
            GetAccountInfo(conn, id, message, signature);
        }

        private async void GetAccountInfo(GameObject conn, string id, string message, string signature)
        {
            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                string email = "info@grabcoinclub.com";
                int hash = email.GetHashCode();
                if (hash < 0)
                    hash *= -1;
                _emailAuthService.FillingData(hash.ToString(), email, "cfgkhjesru645vcbnhbjtr");
                await _emailAuthService.SignIn();
            }

#if ENABLE_PLAYFABSERVER_API
            var data = new DoubleString { conn = conn, message = message, signature = signature, id = id };
            PlayFabClientAPI.GetAccountInfo(new PlayFab.ClientModels.GetAccountInfoRequest(), GetAccountInfoSuccess,
                error =>
                {
                    Debug.LogError(error);
                    LinkMetamaskResult(conn, AuthState.Fail);
                }, data);
#endif
        }

#if ENABLE_PLAYFABSERVER_API
        private void GetAccountInfoSuccess(PlayFab.ClientModels.GetAccountInfoResult result)
        {
            LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.ServerGetSelfAccount);
            PlayFabServerAPI.GetUserReadOnlyData(new GetUserDataRequest
            { PlayFabId = result.AccountInfo.PlayFabId },
                GetUserReadOnlyDataSuccess,
                error =>
                {
                    Debug.LogError(error);
                    LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.Fail);
                },
                result.CustomData);
        }

        private void GetUserReadOnlyDataSuccess(GetUserDataResult result)
        {
            try
            {
                Debug.Log("Get data list");
                DoubleString data = (DoubleString)result.CustomData;
                AuthInfo info = new AuthInfo(data.message, data.signature);
                var key = GetAddress(info);
                LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.GetedReadonlyData);

                if (result.Data.ContainsKey(key.GetPublicAddress()))
                {
                    if (string.IsNullOrEmpty(data.id))
                    {
                        PlayFabServerAPI.GetUserAccountInfo(new GetUserAccountInfoRequest
                        {
                            PlayFabId = result.Data[key.GetPublicAddress()].Value
                        }, result =>
                        {
                            string customId = result.UserInfo.CustomIdInfo.CustomId;
                            if (string.IsNullOrEmpty(customId))
                            {
                                string newId = System.Guid.NewGuid().ToString();
                                PlayFabServerAPI.LinkServerCustomId(new LinkServerCustomIdRequest
                                {
                                    PlayFabId = result.UserInfo.PlayFabId,
                                    ServerCustomId = newId
                                }, result =>
                                {
                                    Debug.Log("Link new custom id and return");
                                    LinkMetamaskResult(data.conn, AuthState.ValidateCreateAccount, newId);
                                }, error =>
                                {
                                    Debug.LogError(error);
                                    LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.Fail);
                                });
                            }
                            else
                            {
                                Debug.Log("Return saved custom id account with Metamask");
                                LinkMetamaskResult(data.conn, AuthState.ValidateCreateAccount, customId);
                            }
                        }, error =>
                        {
                            Debug.LogError(error);
                            LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.Fail);
                        });

                        //LinkMetamaskResult(data.conn, AuthState.ValidateCreateAccount, result.Data[key.GetPublicAddress()].Value);
                        //Debug.Log($"Validate create account");
                        return;
                    }
                    
                    var isAuthtorized = result.Data[key.GetPublicAddress()].Value == data.id;
                    LinkMetamaskResult(data.conn, isAuthtorized ? AuthState.SuccessLink : AuthState.Banned);
                    Debug.Log($"Valide account: {isAuthtorized}");
                    return;
                }

                if (!string.IsNullOrEmpty(data.id))
                    PlayFabServerAPI.UpdateUserReadOnlyData(new UpdateUserDataRequest
                    {
                        PlayFabId = result.PlayFabId,
                        Data = new Dictionary<string, string>
                            {
                                {key.GetPublicAddress(), data.id}
                            }
                    }, UpdateUserReadOnlyDataSuccess,
                    error =>
                    {
                        Debug.LogError(error);
                        LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.Fail);
                    }, result.CustomData);
                else
                {
                    Debug.Log("Can create new account with Metamask");
                    LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.ValidateCreateAccount);
                }
            }
            catch
            {
                LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.Fail);
            }
        }

        private void UpdateUserReadOnlyDataSuccess(UpdateUserDataResult result)
        {
            Debug.Log("Success link Metamask to account");
            LinkMetamaskResult(((DoubleString)result.CustomData).conn, AuthState.SuccessLink);
        }
#endif

        private void LinkMetamaskResult(GameObject target, AuthState authState, string id = "")
        {
            NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
            TargetLinkMetamaskResult(opponentIdentity.connectionToClient, authState, id);
        }

        [TargetRpc]
        private void TargetLinkMetamaskResult(NetworkConnectionToClient target, AuthState authState, string id)
        {
            _authtorizeState = authState;
            _playfabId = id;
        }

        private EthECKey GetAddress(AuthInfo authInfo)
        {
            var msg = "\x19" + "Ethereum Signed Message:\n" + authInfo.SignMessage.Length + authInfo.SignMessage;
            var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            var signature = MessageSigner.ExtractEcdsaSignature(authInfo.SignSignature);
            return EthECKey.RecoverFromSignature(signature, msgHash);
        }
    }
}
