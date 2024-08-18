using Cysharp.Threading.Tasks;

namespace GrabCoin.AsyncProcesses
{
    public interface IAsyncProcess<T>
    {
        public UniTask<T> Run();
    }
}