using System.ComponentModel.DataAnnotations;

namespace Npm.Renovator.ConsoleApp.Models;

internal enum YNEnum
{
    [Display(Name = "n")]
    N,
    [Display(Name = "y")]
    Y
}