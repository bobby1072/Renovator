﻿namespace Npm.Renovator.ConsoleApp.Abstract;

public interface IConsoleApp
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}