using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public class PostmanRequestLogin : PostmanRequestBase<PostmanRequestLogin.Response, PostmanRequestLogin.Parameters>
    {
        public override bool NeedAuthorization => false;
        protected override string Method => UnityWebRequest.kHttpVerbPOST;
        protected override string Address => "/Account/Login";

        public override async UniTask<IBackendRequest<Response, Parameters>.Response> ProcessRequest()
        {
            var response = await base.ProcessRequest();
            if (response != null && response.data != null)
            {
                _service.SetAuthToken(response.data.token);
            }

            return response;
        }

        public class Parameters
        {
            public string email;
            public string password;
        }

        public class Response
        {
            public string token;
        }

        public PostmanRequestLogin(BackendServicePostman service) : base(service)
        {
        }
    }
}