using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public class PostmanRequestRegister : PostmanRequestBase<PostmanRequestRegister.Response, PostmanRequestRegister.Parameters>
    {
        public override bool NeedAuthorization => false;
        protected override string Method => UnityWebRequest.kHttpVerbPOST;
        protected override string Address => "/Account/Register";

        public PostmanRequestRegister(BackendServicePostman service) : base(service)
        {
        }

        public class Parameters
        {
            public string email;
            public string password;
        }

        public class Response
        {
        }
    }
}