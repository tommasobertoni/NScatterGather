using System;

namespace NScatterGather
{
    public class ScatterGatherOptions
    {
        private TimeSpan _cancellationWindow;

        public ScatterGatherOptions()
        {
            CancellationWindow = TimeSpan.FromMilliseconds(100);
            AllowCancellationWindowOnAllRecipients = false;
        }

        public TimeSpan CancellationWindow
        {
            get { return _cancellationWindow; }
            set
            {
                if (value.IsNegative())
                    throw new ArgumentException($"{nameof(CancellationWindow)} can't be negative.");

                _cancellationWindow = value;
            }
        }

        public bool AllowCancellationWindowOnAllRecipients { get; set; }

        public int? Limit { get; set; }

        internal ScatterGatherOptions Clone() =>
            (ScatterGatherOptions)MemberwiseClone();
    }
}
