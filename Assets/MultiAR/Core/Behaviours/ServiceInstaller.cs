using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using Zenject;

namespace MultiAR.Core.Behaviours
{
    using Shell.Scripts.Pointer;

    public class ServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IMultiUserService>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<IRoomDescriptionService>().FromComponentInHierarchy().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<RoomService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PointerService>().AsSingle().NonLazy();
        }
    }
}
