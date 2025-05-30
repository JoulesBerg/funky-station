using Content.Client.UserInterface.Controls;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Xenoarchaeology.Ui;

[GenerateTypedNameReferences]
public sealed partial class NodeScannerDisplay : FancyWindow
{
    [Dependency] private readonly IEntityManager _ent = default!;

    public NodeScannerDisplay()
    {
        RobustXamlLoader.Load(this);

        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Sets entity that represents hand-held xeno artifact node scanner for which window is opened.
    /// Closes window if <see cref="NodeScannerComponent"/> is not present on entity.
    /// </summary>
    public void SetOwner(EntityUid scannerEntityUid)
    {
        if (!_ent.TryGetComponent<NodeScannerComponent>(scannerEntityUid, out var scannerComponent))
        {
            Close();
            return;
        }

        Update((scannerEntityUid, scannerComponent));
    }

    /// <summary>
    /// Updates labels with scanned artifact data and list of triggered nodes from component.
    /// </summary>
    public void Update(Entity<NodeScannerComponent> ent)
    {
        ArtifactStateLabel.Text = GetState(ent);
        var scannedAt = ent.Comp.ScannedAt;
        NodeScannerState.Text = scannedAt > TimeSpan.Zero
            ? Loc.GetString("node-scanner-artifact-scanned-time", ("time", scannedAt.Value.ToString(@"hh\:mm\:ss")))
            : Loc.GetString("node-scanner-artifact-scanned-time-none");

        ActiveNodesList.Children.Clear();

        var triggeredNodesSnapshot = ent.Comp.TriggeredNodesSnapshot;
        if (triggeredNodesSnapshot.Count > 0)
        {
            // show list of triggered nodes instead of 'no data' placeholder
            NoActiveNodeDataLabel.Visible = false;
            ActiveNodesList.Visible = true;

            foreach (var nodeId in triggeredNodesSnapshot)
            {
                var nodeLabel = new Button
                {
                    Text = nodeId,
                    Margin = new Thickness(15, 5, 0, 0),
                    MaxHeight = 40,
                    Disabled = true
                };
                ActiveNodesList.Children.Add(nodeLabel);
            }
        }
        else
        {
            // clear list of activated nodes (done previously), show 'no data' placeholder
            NoActiveNodeDataLabel.Visible = true;
            ActiveNodesList.Visible = false;
        }
    }

    private string GetState(Entity<NodeScannerComponent> ent)
    {
        return ent.Comp.ArtifactState switch
        {
            ArtifactState.None => "\u2800", // placeholder for line to not be squeezed
            ArtifactState.Ready => Loc.GetString("node-scanner-artifact-state-ready"),
            ArtifactState.Unlocking => Loc.GetString("node-scanner-artifact-state-unlocking"),
            ArtifactState.Cooldown => Loc.GetString("node-scanner-artifact-state-cooldown")
        };
    }
}
