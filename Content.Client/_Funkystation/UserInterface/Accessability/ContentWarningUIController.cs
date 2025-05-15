using Content.Client.Info;
using Content.Shared.Guidebook;
using Content.Shared._Funkystation.Accessability.Messages;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.UserInterface.Accessability;

public sealed class ContentWarningUIController : UIController
{
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ILogManager _logMan = default!;

    private ContentWarningPopup? _warningPopup;
    private ISawmill _sawmill = default!;

    [ValidatePrototypeId<GuideEntryPrototype>]
    private const string ContentWarning = "ContentWarning";

    public ProtoId<GuideEntryPrototype> WarningEntryId = ContentWarning;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logMan.GetSawmill("content_warnings");
        _netManager.RegisterNetMessage<ContentWarningAcknowledgedMessage>();
        _netManager.RegisterNetMessage<SendContentWarningMessage>(OnRulesInformationMessage);
    }

    private void OnRulesInformationMessage(SendContentWarningMessage message)
    {
        WarningEntryId = message.WarningMessage;

        ShowWarning(message.PopupTime);
    }

    private void ShowWarning(float time)
    {
        if (_warningPopup != null)
            return;

        _warningPopup = new ContentWarningPopup
        {
            Timer = time
        };

        _warningPopup.OnQuitPressed += OnQuitPressed;
        _warningPopup.OnAcceptPressed += OnAcceptPressed;
        UIManager.WindowRoot.AddChild(_warningPopup);
        LayoutContainer.SetAnchorPreset(_warningPopup, LayoutContainer.LayoutPreset.Wide);
    }

    private void OnQuitPressed()
    {
        _consoleHost.ExecuteCommand("quit");
    }

    private void OnAcceptPressed()
    {
        _netManager.ClientSendMessage(new ContentWarningAcknowledgedMessage());

        _warningPopup?.Orphan();
        _warningPopup = null;
    }

    public GuideEntryPrototype GetContentWarningEntry()
    {
        if (!_prototype.TryIndex(WarningEntryId, out var guideEntryPrototype))
        {
            guideEntryPrototype = _prototype.Index<GuideEntryPrototype>(ContentWarning);
            _sawmill.Error($"Couldn't find the following prototype: {WarningEntryId}. Falling back to {ContentWarning}, please check that the server has the warning set up correctly");
            return guideEntryPrototype;
        }

        return guideEntryPrototype;
    }
}
