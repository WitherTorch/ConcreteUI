using System.Diagnostics;

using ConcreteUI.Graphics.Internals.Native;

using WitherTorch.Common.Native;

namespace ConcreteUI.Graphics.Internals
{
    internal sealed class DelayedSystemBooster : DelayedCollectingObject
    {
        public static readonly DelayedSystemBooster Instance = new DelayedSystemBooster();

        private DelayedSystemBooster() { }

        protected override void GenerateObject()
        {
            const uint DesiredTime = 10000; // 10000 ticks = 10000 * 0.1us = 1000us = 1ms

            Debug.WriteLine($"[{nameof(DelayedSystemBooster)}.{nameof(GenerateObject)}()] {nameof(NtDll.NtSetTimerResolution)} is called!, desiredTime = {DesiredTime:D} ticks");
            NtDll.NtSetTimerResolution(desiredTime: DesiredTime, setResolution: true);
        }

        protected override void DestroyObject()
        {
            Debug.WriteLine($"[{nameof(DelayedSystemBooster)}.{nameof(DestroyObject)}()] {nameof(NtDll.NtSetTimerResolution)} is called!, the resolution is reseted!");
            NtDll.NtSetTimerResolution(desiredTime: 0, setResolution: false);
        }
    }
}
