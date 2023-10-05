namespace MultiAR.Core.Helper
{
    using Models;
    using System.Collections.Generic;

    public class ImmutableInteractionSet
    {
        private readonly HashSet<NetworkObjectInteraction> _data;

        private readonly NetworkObjectInteraction _cachedLocal;

        public static readonly ImmutableInteractionSet Empty = new ImmutableInteractionSet();

        public HashSet<NetworkObjectInteraction> Data
        {
            get { return _data; }
        }

        public NetworkObjectInteraction FindLocal()
        {
            return _cachedLocal;
        }

        public int Count() => _data.Count;

        ImmutableInteractionSet()
        {
            _data = new HashSet<NetworkObjectInteraction>();
        }

        public ImmutableInteractionSet(IEnumerable<NetworkObjectInteraction> seed)
        {
            _data = new HashSet<NetworkObjectInteraction>(seed);
        }

        public ImmutableInteractionSet(IEnumerable<NetworkObjectInteraction> seed, NetworkObjectInteraction cachedLocal)
        {
            _data = new HashSet<NetworkObjectInteraction>(seed);
            _cachedLocal = cachedLocal;
        }


        public ImmutableInteractionSet TryAdd(NetworkObjectInteraction value)
        {
            if (this._data.Contains(value))
            {
                return this;
            }

            var copy = new HashSet<NetworkObjectInteraction>(this._data) {value};
            return new ImmutableInteractionSet(copy, value.IsLocal ? value : _cachedLocal);
        }

        public ImmutableInteractionSet TryRemove(NetworkObjectInteraction value)
        {
            if (!this._data.Contains(value))
            {
                return this;
            }

            var copy = new HashSet<NetworkObjectInteraction>();
            foreach (var element in this._data)
            {
                if (!element.Equals(value))
                {
                    copy.Add(element);
                }
            }
            return new ImmutableInteractionSet(copy, value.IsLocal ? null : _cachedLocal);
        }
    }
}
