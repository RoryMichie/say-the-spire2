using MegaCrit.Sts2.Core.Entities.Creatures;
using SayTheSpire2.Localization;
using SayTheSpire2.Settings;

namespace SayTheSpire2.Events;

// A dev console command that changed the shared run in multiplayer. The game
// sends these to every peer and runs them there, but shows nothing unless you
// happen to have your own console open, so the issuer is named here.
// The local player's own commands are off by default: the console already
// speaks its response to whoever typed it.
[EventSettings("console_command", "Console Command", category: "Multiplayer",
    hasSourceFilter: true, allowEnemies: false, defaultCurrentPlayer: false)]
public class ConsoleCommandEvent : GameEvent
{
    private readonly string _playerName;
    private readonly string _command;
    private readonly bool _isLocal;

    public ConsoleCommandEvent(string playerName, string command, bool isLocal, Creature? source)
    {
        Source = source;
        _playerName = playerName;
        _command = command;
        _isLocal = isLocal;
    }

    public override Message? GetMessage() => _isLocal
        ? Message.Localized("ui", "EVENT.CONSOLE_COMMAND_LOCAL", new { command = _command })
        : Message.Localized("ui", "EVENT.CONSOLE_COMMAND", new { player = _playerName, command = _command });
}
