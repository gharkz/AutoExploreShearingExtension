using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Elin_AutoExplore;
using Explorer = Elin_AutoExplore.Plugin;

namespace AutoExploreShearing
{
    [BepInPlugin(Id, DisplayName, "1.0.0")]
    // Elin creates a separate chainloader per package. A hard dependency cannot
    // see plugins loaded by another package's chainloader; validate at runtime.
    [BepInDependency("yuof.elin.autoExplore.mod", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Id = "gharkz.autoexploreshearing";
        public const string DisplayName = "Auto-Explore Shearing Extension";
        private static ConfigEntry<bool> enableShearing;
        private sealed class FailedTarget { public int X, Z; public DateTime Until; }
        private static readonly ConditionalWeakTable<Chara, FailedTarget> failures = new ConditionalWeakTable<Chara, FailedTarget>();
        private static Harmony harmony;
        public static bool Installed { get; private set; }

        private void Start()
        {
            try
            {
                Install(Config);
                Logger.LogInfo(DisplayName + " 1.0.0 active; early-exit and full-search hooks installed.");
            }
            catch (Exception e)
            {
                Logger.LogError(DisplayName + " disabled: " + e);
            }
        }

        private void OnDestroy() => Uninstall();

        public static void Install(ConfigFile configuration)
        {
            if (Installed) return;
            // Start runs after Elin's synchronous package-loading loop, allowing
            // normal user load order to place this package before AutoExplore.
            if (Explorer.Instance == null)
                throw new InvalidOperationException("Enable AutoExplore 1.6 or newer before using Auto Explore Shearing.");
            foreach (var method in PatchTargets())
                if (method == null) throw new MissingMethodException("Unsupported AutoExplore targeting API");
            enableShearing = configuration.Bind("General", "Enabled", true,
                "Add native shearing jobs to AutoExplore in Gather/Combined mode while shears are selected.");
            harmony = new Harmony(Id);
            try
            {
                harmony.PatchAll(typeof(Plugin).Assembly);
                Installed = true;
            }
            catch { Uninstall(); throw; }
        }

        public static void Uninstall()
        {
            harmony?.UnpatchSelf();
            Installed = false;
            enableShearing = null;
        }

        public static MethodInfo[] PatchTargets() => new[] {
            AccessTools.Method(typeof(AIActionFinder), "ClassifyApproach", new[] { typeof(Point) }),
            AccessTools.Method(typeof(AIActionFinder), nameof(AIActionFinder.FindHarvestables), Type.EmptyTypes)
        };

        internal static bool Ready => enableShearing != null && enableShearing.Value &&
            Explorer.Instance != null && Explorer.Instance.IsRunning &&
            Explorer.Instance.AutoExplorerConfig.HandleHarvestables.Value &&
            EClass.pc != null && !EClass.pc.isDead && EClass.pc.Tool?.trait is TraitToolShears;

        internal static bool Eligible(Chara target)
        {
            var pc = EClass.pc;
            return target != null && target != pc && !RecentlyFailed(target) && target.IsAliveInCurrentZone &&
                target.pos.IsInBounds && target.CanBeSheared() &&
                !target.IsHostile(pc) && !pc.IsHostile(target) &&
                !Explorer.Instance.IgnoreList.Gathering.Contains(target.Name) &&
                CanAffordOrRest(pc, target);
        }

        private static bool CanAffordOrRest(Chara pc, Chara target)
        {
            var cost = ExploreShear.StaminaCost(pc, target);
            return pc.stamina.value > cost || (pc.stamina.max > cost &&
                Explorer.Instance.AutoExplorerConfig.UseMeditation.Value && Explorer.CanRestOn(pc.pos, pc));
        }

        internal static bool RecentlyFailed(Chara target) => failures.TryGetValue(target, out var failed) &&
            failed.X == target.pos.x && failed.Z == target.pos.z && failed.Until > DateTime.UtcNow;

        internal static void NoteFailure(Chara target)
        {
            failures.Remove(target);
            failures.Add(target, new FailedTarget { X = target.pos.x, Z = target.pos.z,
                Until = DateTime.UtcNow.AddSeconds(10) });
        }

        // Never call IsPointReachable here: this hook executes INSIDE the early-exit
        // flood callback, and starting another flood would corrupt its working state.
        [HarmonyPatch(typeof(AIActionFinder), "ClassifyApproach")]
        internal static class EarlySearch
        {
            private static void Postfix(Point t, ref AIAct __result)
            {
                if (__result != null || t == null || !t.IsInBounds || !t.HasChara || !Ready) return;
                foreach (var target in t.Charas)
                {
                    if (!Eligible(target)) continue;
                    __result = new ExploreShear(target);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(AIActionFinder), nameof(AIActionFinder.FindHarvestables))]
        internal static class FullSearch
        {
            private static void Postfix(AIActionFinder __instance, List<AIAct> __result)
            {
                if (!Ready) return;
                // Only used with early-exit disabled. The caller already sorts all
                // jobs by flood distance; preserve that ordering and native checks.
                foreach (var target in EClass._map.charas)
                    if (Eligible(target) && __instance.IsPointReachable(target.pos, 1))
                        __result.Add(new ExploreShear(target));
            }
        }
    }

    // Not an AI_Shear at the SetAI boundary, so AutoAct's global SetAI prefix
    // cannot turn this into its own map-wide AutoActShear job. The native child
    // starts via AIAct.Do/SetChild, not Chara.SetAI.
    public sealed class ExploreShear : AIAct
    {
        public Chara Target { get; }
        public ExploreShear(Chara target) => Target = target ?? throw new ArgumentNullException(nameof(target));
        public override Point GetDestination() => Target.pos;
        public override bool ShouldAllyAttack(Chara target) => target != Target;
        public override bool CanManualCancel() => true;

        public override IEnumerable<Status> Run()
        {
            isFail = () => !Plugin.Ready || !Target.IsAliveInCurrentZone;
            if (!Plugin.Ready || !Plugin.Eligible(Target)) yield break;
            // Native shearing spends one stamina per affected NPC. Preserve a
            // positive reserve, including wide shears, using native meditation.
            if (!EnoughStamina(owner, Target))
            {
                if (!Elin_AutoExplore.Plugin.Instance.AutoExplorerConfig.UseMeditation.Value ||
                    !Elin_AutoExplore.Plugin.CanRestOn(owner.pos, owner)) yield break;
                yield return Do(new AI_Meditate(), Failed);
                if (!Plugin.Ready || !Plugin.Eligible(Target) || !EnoughStamina(owner, Target)) yield break;
            }
            yield return Do(new NativeShear { target = Target }, Failed);
        }

        private Status Failed() { Plugin.NoteFailure(Target); return Cancel(); }

        internal static bool EnoughStamina(Chara owner, Chara target)
            => owner.stamina.value > StaminaCost(owner, target);

        internal static int StaminaCost(Chara owner, Chara target)
        {
            var wide = owner.Tool?.Evalue(770) ?? 0;
            var cost = 1;
            if (wide > 0)
            {
                cost = 0;
                var radius = 1 + wide / 10;
                foreach (var point in EClass._map.ListPointsInSquare(target.pos, radius))
                    foreach (var c in point.ListCharas())
                        if (c.CanBeSheared()) cost++;
            }
            return cost;
        }

        private sealed class NativeShear : AI_Shear
        {
            // Native progress rechecks IsValidTC, so switching tools/modes or an
            // animal moving into the wide area cannot spend the stamina reserve.
            public override bool IsValidTC(Card c) => base.IsValidTC(c) && Plugin.Ready &&
                EnoughStamina(owner, c.Chara);
        }
    }
}
