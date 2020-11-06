using System.Threading.Tasks;

namespace NScatterGather
{
    internal static class TaskBclExtensions
    {
        public static bool IsCompletedSuccessfully(this Task task)
        {
#if NETSTANDARD2_0
            return task.IsCompleted &&
                !task.IsFaulted &&
                !task.IsCanceled;
#else
            return task.IsCompletedSuccessfully;
#endif
        }
    }
}
