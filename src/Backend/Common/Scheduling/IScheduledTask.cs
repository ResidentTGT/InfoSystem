using System.Threading;
using System.Threading.Tasks;

namespace Company.Common.Scheduling.Cron
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}