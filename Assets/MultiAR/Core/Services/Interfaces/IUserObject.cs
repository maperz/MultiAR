namespace MultiAR.Core.Services.Interfaces
{
    using JetBrains.Annotations;
    using Models;

    public interface IUserObject
    {
        [CanBeNull] public User User { get; }
    }
}
