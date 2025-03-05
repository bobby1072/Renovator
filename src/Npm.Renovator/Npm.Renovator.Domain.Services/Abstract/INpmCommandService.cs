using Npm.Renovator.Domain.Models.Views;

namespace Npm.Renovator.Domain.Services.Abstract;

public interface INpmCommandService
{
    Task<NpmCommandResultsView> RunNpmInstall(string filePath);
}