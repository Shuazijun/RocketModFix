using Rocket.API;
using System;

namespace Rocket.Core.Assets
{
    public class Asset<T> : IAsset<T> where T : class
    {
        protected T? instance;

        public T Instance
        {
            get
            {
                if (instance == null) Load();
                return instance!;
            }
            set
            {
                if (value != null)
                {
                    instance = value;
                    Save();
                }
            }
        }

        public virtual T Save()
        {
            return instance!;
        }

        public virtual void Load(AssetLoaded<T>? callback = null)
        {
            callback?.Invoke(this);
        }

        public virtual void Unload(AssetUnloaded<T>? callback = null)
        {
            callback?.Invoke(this);
        }
    }
}