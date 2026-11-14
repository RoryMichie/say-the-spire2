using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Debug;
using SayTheSpire2.Events;
using SayTheSpire2.Multiplayer;
using SayTheSpire2.Speech;
using SayTheSpire2.UI.Elements;

namespace SayTheSpire2.Patches;

public static class DevConsoleHooks
{
    private static readonly System.Reflection.FieldInfo? OutputBufferField =
        AccessTools.Field(typeof(NDevConsole), "_outputBuffer");

    public static void Initialize(Harmony harmony)
    {
        HarmonyHelper.PatchIfFound(harmony, typeof(DevConsole), "ProcessCommand",
            typeof(DevConsoleHooks), nameof(ProcessCommandPostfix), "DevConsole ProcessCommand",
            parameterTypes: new[] { typeof(string) });
        HarmonyHelper.PatchIfFound(harmony, typeof(NDevConsole), "ShowConsole",
            typeof(DevConsoleHooks), nameof(ShowConsolePostfix), "NDevConsole ShowConsole");
        // Networked commands (gold, card, relic, energy, heal, ...) are sent
        // to every peer as a game action and run there through this method,
        // with the player who typed them. Prefix, so a command that throws
        // is still reported.
        HarmonyHelper.PatchIfFound(harmony, typeof(DevConsole), "ProcessNetCommand",
            typeof(DevConsoleHooks), nameof(ProcessNetCommandPrefix), "DevConsole ProcessNetCommand",
            isPrefix: true);
    }

    public static void ProcessNetCommandPrefix(Player? __0, string __1)
    {
        try
        {
            if (__0 == null || string.IsNullOrWhiteSpace(__1)) return;
            if (!MultiplayerHelper.IsMultiplayer()) return;

            EventDispatcher.Enqueue(new ConsoleCommandEvent(
                MultiplayerHelper.GetPlayerName(__0), __1.Trim(),
                MultiplayerHelper.IsLocalPlayer(__0), __0.Creature));
        }
        catch (System.Exception e)
        {
            Log.Error($"[AccessibilityMod] DevConsole net command announce failed: {e.Message}");
        }
    }

    public static void ShowConsolePostfix(NDevConsole __instance)
    {
        try
        {
            var outputBuffer = OutputBufferField?.GetValue(__instance) as RichTextLabel;
            if (outputBuffer == null) return;

            var text = ProxyElement.StripBbcode(outputBuffer.Text).Trim();
            if (!string.IsNullOrEmpty(text))
                SpeechManager.Output(text);
        }
        catch (System.Exception e)
        {
            Log.Error($"[AccessibilityMod] DevConsole show speech failed: {e.Message}");
        }
    }

    public static void ProcessCommandPostfix(CmdResult __result)
    {
        try
        {
            if (!string.IsNullOrEmpty(__result.msg))
            {
                var text = ProxyElement.StripBbcode(__result.msg);
                if (!string.IsNullOrEmpty(text))
                    SpeechManager.Output(text);
            }
        }
        catch (System.Exception e)
        {
            Log.Error($"[AccessibilityMod] DevConsole output speech failed: {e.Message}");
        }
    }
}
