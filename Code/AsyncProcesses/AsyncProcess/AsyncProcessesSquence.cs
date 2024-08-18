using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace GrabCoin.AsyncProcesses
{
    public class AsyncProcessesSquence
    {
        private IAsyncProcess<bool>[] _processes;
        public AsyncProcessesSquence(params IAsyncProcess<bool>[] processes)
        {
            _processes = processes;
        }


        public async UniTask<bool> Run()
        {
            foreach (var process in _processes)
            {
                var result = process.Run();
                if (!(await process.Run()))
                    return false;
            }
            return true;
        }

        public async UniTask<bool> RunInParallel()
        {
            var tasks = new List<UniTask<bool>>();
            foreach (var process in _processes)
            {
                tasks.Add(process.Run());
            }
            var results = await UniTask.WhenAll(tasks);
            return Array.IndexOf(results, false) < 0;
        }
    }
}