using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Accessability.Messages;

/// <summary>
///  Sent by the server when a gamerule is added that this player has requested to recieve warnings about.
/// </summary>
public sealed class SendContentWarningMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public float PopupTime { get; set; }
    public string WarningMessage { get; set; } = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        PopupTime = buffer.ReadFloat();
        WarningMessage = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(PopupTime);
        buffer.Write(WarningMessage);
    }
}

/// <summary>
///     Sent by the client when it has acknowledged the content warning.
/// </summary>
public sealed class ContentWarningAcknowledgedMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }
}