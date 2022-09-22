using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class AsyncFuncInvoker<TResult>
    {
        public AsyncFuncInvoker(Func<Task<TResult?>> invoker)
        {
            _invoker = invoker;
        }
        private readonly Func<Task<TResult?>> _invoker;
        private bool _isInvoke;
        private TResult? _result;
        public async Task<TResult?> GetResultAsync()
        {
            if (_isInvoke)
            {
                return _result;
            }
            _result = await _invoker().ConfigureAwait(false);
            _isInvoke = true;
            return _result;
        }

        public static implicit operator AsyncFuncInvoker<TResult>(Func<Task<TResult?>> func) => new AsyncFuncInvoker<TResult>(func);

    }
}
