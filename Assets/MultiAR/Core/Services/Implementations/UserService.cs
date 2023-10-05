using System.Threading.Tasks;
using UnityEngine;

namespace MultiAR.Core.Services.Implementations
{
    using Interfaces;

    public class UserService : IUserService
    {
        public Task<string> GetUserName()
        {
            return Task.FromResult(GenerateRandomUserName());
        }

        private string GenerateRandomUserName()
        {
            return $"{SystemInfo.deviceName}#{Random.Range(0, 100)}";
        }
    }
}
