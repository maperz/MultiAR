using Microsoft.MixedReality.Toolkit;
using MultiAR.Core.Models;

namespace MultiAR.Core.Services.Interfaces
{
    public interface IDeviceTypeService : IMixedRealityExtensionService
    {
        Device GetDeviceType();

        bool IsDeviceScreenBased();
    }
}
