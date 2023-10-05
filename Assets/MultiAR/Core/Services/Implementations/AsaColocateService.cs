#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
#define ASA_LOCATE_ENABLED
# endif

#if UNITY_EDITOR
#undef ASA_LOCATE_ENABLED
#endif

using System;
using System.Threading.Tasks;
using MultiAR.Core.Services.Interfaces;
using UnityEngine;

#if ASA_LOCATE_ENABLED
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
#endif

namespace MultiAR.Core.Services.Implementations
{
    using UniRx;

#if ASA_LOCATE_ENABLED
	public class AsaColocateService : MonoBehaviour, IColocateService<string>
	{
		private SpatialAnchorManager _anchorManager;
		private CloudSpatialAnchorWatcher _anchorWatcher;

		private readonly Dictionary<string, int> _watchedAnchorsRefCount = new Dictionary<string, int>();
		private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

		private readonly Dictionary<string, Pose> _cachedLocations = new Dictionary<string, Pose>();

		private bool _isInitialized;
		private event Action OnInitialized;

		private readonly BehaviorSubject<float> _canCreateProgress = new BehaviorSubject<float>(0);
        private readonly Subject<LocationEvent<string>> _anchorLocated = new Subject<LocationEvent<string>>();

		private void QueueOnUpdate(Action updateAction)
		{
			lock (_dispatchQueue)
			{
				_dispatchQueue.Enqueue(updateAction);
			}
		}

		private void Start()
		{
            _anchorManager = GetComponent<SpatialAnchorManager>();
            if (_anchorManager == null)
            {
                _anchorManager = gameObject.AddComponent<SpatialAnchorManager>();
            }

			_anchorManager.enabled = true;
			_anchorManager.AnchorLocated += OnAnchorLocatedEvent;
			//_anchorManager.LogDebug += (sender, args) => Debug.Log($"ASA - Debug: {args.Message}");
			//_anchorManager.Error += (sender, args) => Debug.LogError($"ASA - Error: {args.ErrorMessage}");
		}

		private void Update()
		{
			lock (_dispatchQueue)
			{
				while (_dispatchQueue.Any())
				{
					Debug.Log("Dispatching update queue action");
					_dispatchQueue.Dequeue().Invoke();
				}
			}
		}

		private void OnDestroy()
		{
			if (_anchorManager != null && _anchorManager.Session != null)
			{
				_anchorManager.DestroySession();
			}

			if (_anchorWatcher == null)
			{
				return;
			}

			_anchorWatcher.Stop();
			_anchorWatcher = null;
		}

		private void OnSessionStarted(object sender, EventArgs args)
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				OnInitialized?.Invoke();
			}
		}

		private void OnSessionStopped(object sender, EventArgs args)
		{
		}

		private void OnSessionUpdate(object sender, SessionUpdatedEventArgs args)
		{
			var status = args.Status;
			_canCreateProgress.OnNext(status.RecommendedForCreateProgress);

			Debug.Log($"Recommend Create={status.RecommendedForCreateProgress: 0.#%}");
		}

        public IObservable<float> CanCreateProgress()
        {
            return _canCreateProgress;
        }

        public IObservable<LocationEvent<string>> OnLocated()
        {
            return _anchorLocated;
        }

        private async void StopAzureSession()
		{
			Debug.Log("Stopping Azure session... please wait...");
			_anchorManager.StopSession();
			await _anchorManager.ResetSessionAsync();
			Debug.Log("Azure session stopped successfully");
		}

		private void OnAnchorLocatedEvent(object sender, AnchorLocatedEventArgs args)
		{
			Debug.Log($"Anchor recognized as a possible ASA anchor");
			var foundAnchor = args.Anchor;
			var foundAnchorId = args.Identifier;

			if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
			{
				QueueOnUpdate(() =>
				{
					try
					{
						Debug.Log($"Azure anchor located successfully: {foundAnchorId}");
						var anchorPose = foundAnchor.GetPose();

						Debug.Log(
							$"Setting object to anchor pose with position '{anchorPose.position}' and rotation '{anchorPose.rotation}'");
						transform.SetPositionAndRotation(anchorPose.position, anchorPose.rotation);
						gameObject.name = "Anchor: " + foundAnchorId;
						gameObject.CreateNativeAnchor();

						lock (_cachedLocations)
						{
							_cachedLocations[foundAnchorId] = anchorPose;
						}
                        _anchorLocated.OnNext(new LocationEvent<string>()
                        {
                            Identifier = foundAnchorId,
                            Pose = anchorPose
                        });
                    }
					catch (Exception e)
					{
						Debug.LogError("Error occured in anchor located event: " + e.Message);
					}
				});
			}
			else
			{
				Debug.Log($"Attempt to locate Anchor with ID '{foundAnchorId}' failed, locate anchor status was not 'Located' but '{args.Status}'");
			}
		}


		public void AddInitializedListener(Action callback)
		{
			OnInitialized += callback;
			if (_isInitialized)
			{
				callback();
			}
		}

		public void RemoveInitializedListener(Action callback)
		{
			OnInitialized -= callback;
		}

		public async void Init()
		{
			if (_anchorManager.Session != null)
			{
				Debug.Log("Already started azure session");
				return;
			}

			await Task.Delay(3000);

			Debug.Log("Starting anchor session. Please wait...");

			_anchorManager.SessionStarted += OnSessionStarted;
			_anchorManager.SessionStopped += OnSessionStopped;
			_anchorManager.SessionUpdated += OnSessionUpdate;
			Debug.Log("Listeners registered session finished.");

			Debug.Log("Starting session called");

			await _anchorManager.StartSessionAsync();
		}

		public bool IsSupported()
		{
			return true;
		}

		public async void StartLocating(string anchorId)
		{
			if (string.IsNullOrEmpty(anchorId))
			{
				Debug.LogWarning("Started locating called with empty id");
				return;
			}

			await EnsureIsInitialized();

			if (_watchedAnchorsRefCount.TryGetValue(anchorId, out var refCount))
			{
				Debug.Log("(+) Increasing watch refcount for " + anchorId);
				_watchedAnchorsRefCount[anchorId] = refCount + 1;
				return;
			}

			_watchedAnchorsRefCount.Add(anchorId, 1);

			Debug.Log("Creating watcher with added anchor: " + anchorId);
			UpdateAnchorWatcher();
		}

		public async void StopLocating(string anchorId)
		{
			if (string.IsNullOrEmpty(anchorId))
			{
				Debug.LogWarning("Stopped locating called with empty id");
				return;
			}

			await EnsureIsInitialized();

			if (!_watchedAnchorsRefCount.TryGetValue(anchorId, out var refCount))
			{
				Debug.LogWarning("Tried to stop locating without having started. AnchorId:" + anchorId);
				return;
			}

			var newCount = refCount - 1;
			_watchedAnchorsRefCount[anchorId] = newCount;
			Debug.Log("(-) Decreased watch refcount for " + anchorId);

			if (newCount >= 0)
			{
				return;
			}

			_watchedAnchorsRefCount.Remove(anchorId);

			Debug.Log("Creating watcher with removed anchor: " + anchorId);
			UpdateAnchorWatcher();
		}

		public Pose? GetCachedLocation(string identifier)
		{
			lock (_cachedLocations)
			{
				if (_cachedLocations.TryGetValue(identifier, out var pose))
				{
					return pose;
				}
				return null;
			}
		}

		private void UpdateAnchorWatcher()
		{
			_anchorWatcher?.Stop();
			_anchorWatcher = null;
			var anchorLocateCriteria = new AnchorLocateCriteria
			{
				Identifiers = _watchedAnchorsRefCount.Keys.ToArray()
			};
			_anchorWatcher = _anchorManager.Session.CreateWatcher(anchorLocateCriteria);
			Debug.Log("Anchor watcher updated");
		}

		public async Task<string> CreateAnchor(Pose origin, TimeSpan duration)
		{
			await EnsureIsReadyForCreate();

			var anchorObject = new GameObject("Anchor (Local)");
			anchorObject.transform.SetPositionAndRotation(origin.position, origin.rotation);
			var cloudNativeAnchor = anchorObject.AddComponent<CloudNativeAnchor>();
			await cloudNativeAnchor.NativeToCloud();

			var cloudAnchor = cloudNativeAnchor.CloudAnchor;
			cloudAnchor.Expiration = DateTimeOffset.Now.Add(duration);

			Debug.Log($"ASA - Saving anchor to ASA cloud");
			try
			{
				// Now that the cloud spatial anchor has been prepared, we can try the actual save here.
				await _anchorManager.CreateAnchorAsync(cloudAnchor);
				if (string.IsNullOrEmpty(cloudAnchor.Identifier))
				{
					Debug.LogError("ASA - Failed to save, but no exception was thrown.");
					return null;
				}

				Debug.Log($"ASA - Saved cloud anchor with ID: {cloudAnchor.Identifier}");
				anchorObject.name = $"Anchor ({cloudAnchor.Identifier})";
				return cloudAnchor.Identifier;
			}
			catch (Exception exception)
			{
				Debug.Log("ASA - Failed to save anchor: " + exception);
				Debug.LogException(exception);
				return null;
			}
		}

		private Task EnsureIsInitialized()
		{
			if (!_isInitialized)
			{
				throw new Exception("Not initialized");
			}

			return Task.CompletedTask;
		}

		private async Task EnsureIsReadyForCreate()
		{
			await EnsureIsInitialized();

			while (!_anchorManager.IsReadyForCreate)
			{
				var createProgress = _anchorManager.SessionStatus.RecommendedForCreateProgress;
				Debug.Log($"ASA - Move your device to capture more environment data: {createProgress:0%}");
				await Task.Delay(1000);
			}
		}
	}
#else
    public class AsaColocateService : MonoBehaviour, IColocateService<string>
    {
        public void Init()
        {
            throw new PlatformNotSupportedException();
        }

        public bool IsSupported()
        {
            return false;
        }

        public void StartLocating(string identifier)
        {
            throw new PlatformNotSupportedException();
        }

        public void StopLocating(string identifier)
        {
            throw new PlatformNotSupportedException();
        }

        public Task<string> CreateAnchor(Pose origin, TimeSpan duration)
        {
            throw new PlatformNotSupportedException();
        }

        public Pose? GetCachedLocation(string identifier)
        {
            throw new PlatformNotSupportedException();
        }

        public IObservable<LocationEvent<string>> OnLocated()
        {
            throw new PlatformNotSupportedException();
        }

        public void AddInitializedListener(Action callback)
        {
            throw new PlatformNotSupportedException();
        }

        public void RemoveInitializedListener(Action callback)
        {
            throw new PlatformNotSupportedException();
        }

        public IObservable<float> CanCreateProgress()
        {
            throw new PlatformNotSupportedException();
        }
    }
#endif
}
