using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GrabCoin.Services.Backend
{
    public class PostmanRequestTermsConfirm : PostmanRequestBase<PostmanRequestTermsConfirm.Response, PostmanRequestTermsConfirm.Parameters>
    {
        protected override string Method => UnityWebRequest.kHttpVerbPOST;
        protected override string Address => "/TermsOfUse/Confirm";

        public class Parameters
        {
        }

        public class Response
        {
        }

        public PostmanRequestTermsConfirm(BackendServicePostman service) : base(service)
        {
        }
    }
}