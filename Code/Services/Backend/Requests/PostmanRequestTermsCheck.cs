namespace GrabCoin.Services.Backend
{
    public class PostmanRequestTermsCheck : PostmanRequestBase<PostmanRequestTermsCheck.Response, PostmanRequestTermsCheck.Parameters>
    {
        protected override string Address => "/TermsOfUse/CheckConfirmation";

        public class Parameters
        {
        }

        public class Response
        {
            public bool confirmed;
        }

        public PostmanRequestTermsCheck(BackendServicePostman service) : base(service)
        {
        }
    }
}