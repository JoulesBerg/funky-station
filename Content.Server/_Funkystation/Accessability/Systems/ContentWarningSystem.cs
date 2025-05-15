using System.Linq;

using Robust.Server.Player;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;

using Content.Shared.GameTicking.Components;
using Content.Shared._Funkystation.Accessability.Messages;

using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;

using Robust.Shared.Configuration;
using Robust.Shared.Network;

using Content.Server._Funkystation.Accessability.Components.ContentWarningComponent;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server._Funkystation.Accessability.Systems;

public sealed partial class ContentWarningSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private TimeSpan _nextRuleCheck;
    private HashSet<string> _rules = new HashSet<string>();
    public override void Initialize()
    {
        base.Initialize();
        _nextRuleCheck = TimeSpan.FromSeconds(60);

        _netManager.RegisterNetMessage<SendContentWarningMessage>();
        _netManager.RegisterNetMessage<ContentWarningAcknowledgedMessage>(OnAcknowledged);

        SubscribeLocalEvent<GameRuleAddedEvent>(OnRuleAdded);
    }

    public void OnAcknowledged(ContentWarningAcknowledgedMessage msg)
    {
        // do nothing I guess.
    }

    public void OnRuleAdded(ref GameRuleAddedEvent args)
    {
        if (!TryComp<GameRuleComponent>(args.RuleEntity, out var comp))
        {
            return;
        }
        if (TryComp<MetaDataComponent>(args.RuleEntity, out var mComp))
        {
            string preProc = ToPrettyString(uid: args.RuleEntity, metadata: mComp);
            int splitPos = preProc.LastIndexOf(',');
            int closingParen = preProc.LastIndexOf(')');
            string final = preProc.Substring(splitPos + 1, closingParen - splitPos - 1).Trim();
            Log.Info(final);
            UpdateRules(final);
            IssueWarning(final);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        // check every minute, in case new players with the component have joined.
        if (_timing.CurTime > _nextRuleCheck)
        {
            IssueWarnings();
            _nextRuleCheck += TimeSpan.FromSeconds(60);
        }
    }

    private void PopupWarnWindow(EntityUid player)
    {
        var message = new SendContentWarningMessage
        {
            PopupTime = 5,
            WarningMessage = "ContentWarning",
        };

        if (!TryComp<MindContainerComponent>(player, out var playerMindContainer))
        {
            Log.Debug($"Attempted to issue content warning to {ToPrettyString(player)}, but could not retrieve mindcontainercomponent");
            return;
        }

        // make sure theres a mindcomp in there
        if (!TryComp<MindComponent>(playerMindContainer.Mind, out var mindComp))
        {
            Log.Debug($"Attempted to issue content warning to {ToPrettyString(player)}, but could not retrieve mindcomponent");
            return;
        }

        var playerId = mindComp.OriginalOwnerUserId;

        if (!_players.TryGetSessionById(playerId, out var playerSession))
        {
            // well I guess they logged out or smth idk
            Log.Debug($"Attempted to issue content warning to {ToPrettyString(player)}, but could not get session by ID");
            return;
        }
        _netManager.ServerSendMessage(message, playerSession.Channel);
    }

    private void IssueWarning(string rule)
    {
        var query = EntityQueryEnumerator<ContentWarningComponent>();
        while (query.MoveNext(out var playerEntity, out var comp))
        {
            if (comp.WarningIssued)
                continue;

            if (comp.GameRules.Contains(rule))
            {
                comp.WarningIssued = true;
                PopupWarnWindow(playerEntity);
            }
        }
    }

    private void IssueWarnings()
    {
        var query = EntityQueryEnumerator<ContentWarningComponent>();
        while (query.MoveNext(out var playerEntity, out var comp))
        {
            if (comp.WarningIssued)
                continue;

            if (comp.GameRules.Any(_rules.Contains))
            {
                comp.WarningIssued = true;
                PopupWarnWindow(playerEntity);
            }
        }
    }

    private void UpdateRules(string rule)
    {
        _rules.Add(rule);
    }
}