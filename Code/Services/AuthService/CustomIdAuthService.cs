using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GrabCoin.Model;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Zenject;

namespace Code.Services.AuthService
{
    public class CustomIdAuthService
    {
        private const string AUTHENTIFICATION_KEY = "AUTHENTIFICATION_KEY";

        private struct Data
        {
            public bool needCreation;
            public string id;
        }

        private UniTaskCompletionSource<bool> _completion;
        private UserModel _userModel;
        private InventoryDataManager _inventoryData;

        [Inject]
        public void Construct(UserModel userModel, InventoryDataManager inventoryData)
        {
            _userModel = userModel;
            _inventoryData = inventoryData;
        }
        
        public Task SignOut()
        {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> SignIn()
        {
            _completion = new UniTaskCompletionSource<bool>();
            WithoutAuth();
            return _completion.Task;
        }
        private void WithoutAuth()
        {
            var needCreation = !PlayerPrefs.HasKey(AUTHENTIFICATION_KEY);
            var id = PlayerPrefs.GetString(AUTHENTIFICATION_KEY, Guid.NewGuid().ToString());
            var data = new Data { needCreation = needCreation, id = id };
            var request = new LoginWithCustomIDRequest
            {
                CustomId = id,
                CreateAccount = needCreation
            };
            PlayFabClientAPI.LoginWithCustomID(request, Success, Fail, data);
            CheckAuth();
        }

        private async void CheckAuth()
        {
            await UniTask.Delay(60000);
            if (PlayFabSettings.staticPlayer == null || !PlayFabSettings.staticPlayer.IsClientLoggedIn())
            {
                WithoutAuth();
            }
        }

        private void Success(LoginResult result)
        {
            PlayerPrefs.SetString(AUTHENTIFICATION_KEY, ((Data)result.CustomData).id);
            Debug.Log(result.PlayFabId);
            Debug.Log(((Data)result.CustomData).needCreation);
            Debug.Log(((Data)result.CustomData).id);
            _userModel.AuthorizePlayFab(result.PlayFabId);
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), SuccessInfo, Error);
            _inventoryData.Initialize();
        }

        private void Fail(PlayFabError error)
        {
            var errorMessage = error.GenerateErrorReport();
            Debug.LogError(errorMessage);
            _completion.TrySetResult(false);
        }

        private void Error(PlayFabError error)
        {
            Debug.LogError(error);
        }

        private void SuccessInfo(GetAccountInfoResult result)
        {
            Debug.Log(result.AccountInfo.PlayFabId);
            _completion.TrySetResult(true);
        }

        public Task<bool> ValidateAuth()
        {
            return Task.FromResult(PlayerPrefs.HasKey(AUTHENTIFICATION_KEY));
        }

        public Task<AuthInfo> GetAccount()
        {
            return Task.FromResult(_userModel.AuthInfo);
        }
    }
}