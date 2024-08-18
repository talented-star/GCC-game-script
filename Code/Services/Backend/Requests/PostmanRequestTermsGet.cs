namespace GrabCoin.Services.Backend
{
    public class PostmanRequestTermsGet : PostmanRequestBase<PostmanRequestTermsGet.Response, PostmanRequestTermsGet.Parameters>
    {
        protected override string Address => "/TermsOfUse/Get";

        public class Parameters
        {
        }

        public class Response
        {
            public string text;
        }

        public PostmanRequestTermsGet(BackendServicePostman service) : base(service)
        {
        }
    }
}