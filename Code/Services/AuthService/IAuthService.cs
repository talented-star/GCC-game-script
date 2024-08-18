using System.Threading.Tasks;
using GrabCoin.Model;
using Models;

namespace Code.Services.AuthService
{
    public interface IAuthService
    {
        Task SignOut();
        Task<bool> SignIn();
        Task<bool> ValidateAuth();
        Task<AuthInfo> GetAccount();
    }
}