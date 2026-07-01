using Rocket.Unturned.Utils;
using System.Runtime.CompilerServices;

namespace Rocket.Unturned
{
    internal static class RocketUnturnedModuleInit
    {
        [ModuleInitializer]
        internal static void InitializeModule()
        {
            UnityConsoleWarningFilter.Install();
        }
    }
}
