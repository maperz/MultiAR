using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiAR.Core.Services.Interfaces
{
    public class LocationEvent<TLocationIdentifier>
    {
        public TLocationIdentifier Identifier { get; set; }
        public Pose Pose { get; set; }
    }

    public interface IColocateService<TLocationIdentifier>
    {
        public bool IsSupported();
        public void AddInitializedListener(Action callback);
        public void RemoveInitializedListener(Action callback);

        public IObservable<float> CanCreateProgress();
        public IObservable<LocationEvent<TLocationIdentifier>> OnLocated();

        public void Init();

        public void StartLocating(TLocationIdentifier identifier);
        public void StopLocating(TLocationIdentifier identifier);

        public Pose? GetCachedLocation(TLocationIdentifier identifier);

        public Task<TLocationIdentifier> CreateAnchor(Pose location, TimeSpan duration);
    }
}
