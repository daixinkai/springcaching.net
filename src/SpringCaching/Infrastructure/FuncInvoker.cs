using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class FuncInvoker<TResult>
    {
        public FuncInvoker(Func<TResult?> invoker)
        {
            _invoker = invoker;
        }
        private readonly Func<TResult?> _invoker;
        private bool _isInvoke;
        private TResult? _result;
        public TResult? GetResult()
        {
            if (_isInvoke)
            {
                return _result;
            }
            _result = _invoker();
            _isInvoke = true;
            return _result;
        }

        public static implicit operator FuncInvoker<TResult>(Func<TResult?> func) => new FuncInvoker<TResult>(func);

    }
}
