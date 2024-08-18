using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Model;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Zenject;

namespace Code.Services.AuthService
{
    public class EmailAuthService
    {
        public string message;

        private UniTaskCompletionSource<bool> _completion;
        private PlayerState _playerState;
        private UserModel _userModel;
        private CustomIdAuthService _customIdAuthService;
        private InventoryDataManager _inventoryData;
        private string _userName;
        private string _userEmail;
        private string _userPassword;
        private bool _authorisationComplete;
        private bool _authorisationProcess;

        [Inject]
        public void Construct(PlayerState playerState, UserModel userModel, CustomIdAuthService customIdAuthService, InventoryDataManager inventoryData)
        {
            _playerState = playerState;
            _userModel = userModel;
            _customIdAuthService = customIdAuthService;
            _inventoryData = inventoryData;
        }
        
        public Task SignOut()
        {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> SignIn()
        {
            //Debug.Log("Start request sign in");
            _completion = new UniTaskCompletionSource<bool>();
            SignInPlayFab();
            return _completion.Task;
        }

        private void SignInPlayFab()
        {
            if (_authorisationProcess) return;
            _authorisationProcess = true;
            PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest
            {
                Username = _userName,
                Password = _userPassword
            }, SuccessSignIn, Fail);
            CheckAuth();
        }

        private async void CheckAuth()
        {
            await UniTask.Delay(60000);
            if (PlayFabSettings.staticPlayer == null || !PlayFabSettings.staticPlayer.IsClientLoggedIn())
            {
                var result = await SignIn();
                if (!result)
                    Debug.Log("Fail auth");
            }
            CheckAuth();
        }

        public async UniTask<bool> SignUp()
        {
            _completion = new UniTaskCompletionSource<bool>();

            //if (_customIdAuthService.ValidateAuth().Result)
            //{
            //    if (!PlayFabClientAPI.IsClientLoggedIn())
            //    {
            //        await _customIdAuthService.SignIn();
            //    }

            //    PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest
            //    {
            //        Email = _userEmail,
            //        Username = _userName,
            //        Password = _userPassword
            //    }, SuccessSignUp, Fail);
            //}
            //else
            {
                PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
                {
                    Username = _userName,
                    Email = _userEmail,
                    Password = _userPassword,
                    RequireBothUsernameAndEmail = true
                }, SuccessSignUp, Fail);
            }
            return await _completion.Task;
        }

        public async UniTask<bool> LinkEmail()
        {
            _completion = new UniTaskCompletionSource<bool>();

            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                await _customIdAuthService.SignIn();
            }

            PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest
            {
                Email = _userEmail,
                Username = _userName,
                Password = _userPassword
            }, SuccessSignUp, Fail);

            return await _completion.Task;
        }

        public void FillingData(string name, string email, string password)
        {
            _userName = name;
            _userEmail = email;
            _userPassword = password;
        }

        private void SuccessSignUp(RegisterPlayFabUserResult result)
        {
            Debug.Log($"User registrated: {result.Username}");
            _playerState.PlayFabID = result.PlayFabId;
            SignInPlayFab();
        }

        private void SuccessSignUp(AddUsernamePasswordResult result)
        {
            Debug.Log($"Added login/password: {result.Username}");
            _completion.TrySetResult(true);
            _inventoryData.Initialize();
        }

        private void SuccessSignIn(LoginResult result)
        {
            Debug.Log($"User {result.PlayFabId} enter: {result.LastLoginTime}");
            _authorisationComplete = true;
            _authorisationProcess = false;
            _playerState.PlayFabID = result.PlayFabId;
            _userModel.AuthorizePlayFab(result.PlayFabId);
#if !UNITY_SERVER
            _inventoryData.Initialize();
#endif
            _completion.TrySetResult(true);
        }

        private void Fail(PlayFabError error)
        {
            Debug.LogError(error);
            message = error.ErrorMessage;
            _authorisationProcess = false;
            _completion.TrySetResult(false);
        }

        public Task<bool> ValidateAuth()
        {
            return Task.FromResult(PlayFabClientAPI.IsClientLoggedIn());
        }

        public Task<AuthInfo> GetAccount()
        {
            return Task.FromResult(_userModel.AuthInfo);
        }
    }
}