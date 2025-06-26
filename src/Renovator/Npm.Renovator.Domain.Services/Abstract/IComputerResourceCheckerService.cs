namespace Npm.Renovator.Domain.Services.Abstract
{
    public interface IComputerResourceCheckerService
    {
        Task CheckResourcesAsync(CancellationToken token = default);
    }
}
