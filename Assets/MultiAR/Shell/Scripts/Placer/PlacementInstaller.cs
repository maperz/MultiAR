namespace MultiAR.Shell.Scripts.Placer
{
    using Zenject;

    public class PlacementInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPlacementService>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
    }
}
