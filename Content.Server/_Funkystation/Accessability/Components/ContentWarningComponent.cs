namespace Content.Server._Funkystation.Accessability.Components.ContentWarningComponent;

[RegisterComponent]
public sealed partial class ContentWarningComponent : Component
{
    // boolean to track if a trigger warning has been issued for the round.
    [DataField] public bool WarningIssued = false;

    // list of gamerules for which to popup the trigger warning
    [DataField] public List<string> GameRules { get; set; }

    public ContentWarningComponent()
    {
        GameRules = new List<string>();
        GameRules.Add("ChangelingMidround");
    }
}