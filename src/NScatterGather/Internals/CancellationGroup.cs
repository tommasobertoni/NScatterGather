using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NScatterGather
{
    internal class CancellationGroup : IDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly CancellationTokenTaskSource<bool> _cancellationTaskSource;

        public CancellationGroup(CancellationToken cancellationToken)
            : this(new[] { cancellationToken })
        {
        }

        public CancellationGroup(IReadOnlyList<CancellationToken> cancellationTokens)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens.ToArray());
            _cancellationTaskSource = new CancellationTokenTaskSource<bool>(_cts.Token);
            _cancellationTaskSource.Task.ContinueWith(_ => OnCanceled?.Invoke());
        }

        public CancellationToken CancellationToken => _cts.Token;

        public event Action? OnCanceled;

        public void Cancel() =>
            _cts.Cancel();

        public void Dispose()
        {
            _cts.Dispose();
            _cancellationTaskSource.Dispose();
        }
    }
}
