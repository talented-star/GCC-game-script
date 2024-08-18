using Cysharp.Threading.Tasks;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    public interface ITransition
    {
        UniTask Show(ITransition prevOrNull = null);
        UniTask Hide();
    }
}