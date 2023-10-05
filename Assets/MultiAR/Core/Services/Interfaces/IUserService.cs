namespace MultiAR.Core.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IUserService
    {
        public Task<string> GetUserName();
    }
}
