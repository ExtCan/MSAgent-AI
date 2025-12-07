using System;
using System.Runtime.InteropServices;

namespace MSAgentAI.Agent
{
    /// <summary>
    /// COM interface for MS Agent Control
    /// </summary>
    [ComImport]
    [Guid("D45FD31B-5C6E-11D1-9EC1-00C04FD7081F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IAgentCtlEx
    {
        [DispId(1)]
        IAgentCtlCharacterEx Characters { get; }

        [DispId(2)]
        bool Connected { get; set; }

        [DispId(3)]
        void ShowDefaultCharacterProperties();
    }

    /// <summary>
    /// COM interface for Agent Characters collection
    /// </summary>
    [ComImport]
    [Guid("C4ABF875-8100-11D0-AC63-00C04FD97575")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IAgentCtlCharacterEx
    {
        [DispId(0)]
        IAgentCtlCharacter this[string characterID] { get; }

        [DispId(1)]
        IAgentCtlRequest Load(string characterID, object loadKey);

        [DispId(2)]
        void Unload(string characterID);
    }

    /// <summary>
    /// COM interface for a single Agent Character
    /// </summary>
    [ComImport]
    [Guid("C4ABF876-8100-11D0-AC63-00C04FD97575")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IAgentCtlCharacter
    {
        [DispId(1)]
        void Show(object fast);

        [DispId(2)]
        void Hide(object fast);

        [DispId(3)]
        IAgentCtlRequest Speak(object text, object url);

        [DispId(4)]
        IAgentCtlRequest Play(string animation);

        [DispId(5)]
        void Stop(object request);

        [DispId(6)]
        void StopAll(object types);

        [DispId(7)]
        void MoveTo(short x, short y, object speed);

        [DispId(8)]
        void GestureAt(short x, short y);

        [DispId(9)]
        IAgentCtlRequest Think(string text);

        [DispId(10)]
        bool Visible { get; }

        [DispId(11)]
        short Left { get; set; }

        [DispId(12)]
        short Top { get; set; }

        [DispId(13)]
        short Width { get; }

        [DispId(14)]
        short Height { get; }

        [DispId(15)]
        string Name { get; }

        [DispId(16)]
        string Description { get; }

        [DispId(17)]
        bool IdleOn { get; set; }

        [DispId(18)]
        bool SoundEffectsOn { get; set; }

        [DispId(19)]
        string TTSModeID { get; set; }

        [DispId(20)]
        object Balloon { get; }

        [DispId(21)]
        object AnimationNames { get; }

        [DispId(22)]
        short Speed { get; set; }

        [DispId(23)]
        short Pitch { get; set; }
    }

    /// <summary>
    /// COM interface for Agent Request
    /// </summary>
    [ComImport]
    [Guid("1DAB85C3-803A-11D0-AC63-00C04FD97575")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IAgentCtlRequest
    {
        [DispId(1)]
        int Number { get; }

        [DispId(2)]
        string Description { get; }

        [DispId(3)]
        int Status { get; }
    }

    /// <summary>
    /// COM class for MS Agent Control
    /// </summary>
    [ComImport]
    [Guid("D45FD31D-5C6E-11D1-9EC1-00C04FD7081F")]
    public class AgentControl
    {
    }
}
