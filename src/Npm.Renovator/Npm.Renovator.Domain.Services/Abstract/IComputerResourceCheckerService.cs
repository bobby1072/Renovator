namespace Npm.Renovator.Domain.Services.Abstract
{
    public interface IComputerResourceCheckerService
    {
        Task ExecuteAsync(CancellationToken token = default);
    }
}
