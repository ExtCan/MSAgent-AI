using System;
using System.Runtime.InteropServices;

namespace MSAgentAI.Voice
{
    /// <summary>
    /// SAPI4 COM interface definitions
    /// </summary>

    // CLSID and IID constants
    public static class Sapi4Constants
    {
        // {D67C0280-C743-11cd-80E5-00AA003E4B50}
        public static readonly Guid CLSID_TTSEnumerator = new Guid("D67C0280-C743-11cd-80E5-00AA003E4B50");
        
        // {6B837B20-2A59-11cf-A2CC-00AA00A8D5E5}
        public static readonly Guid IID_ITTSEnum = new Guid("6B837B20-2A59-11cf-A2CC-00AA00A8D5E5");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TTSMODEINFO
    {
        public Guid gModeID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModeName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szMfgName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szProductName;
        public Guid gMfgID;
        public Guid gProductID;
        public ushort wEngineVersion;
        public ushort wGender;
        public ushort wAge;
        public ushort wStyle;
        public ushort wSpeaker;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] dwLanguage;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] dwDialect;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] abReserved;
    }

    [ComImport]
    [Guid("6B837B20-2A59-11cf-A2CC-00AA00A8D5E5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITTSEnum
    {
        [PreserveSig]
        int Next(
            uint celt,
            [Out] out TTSMODEINFO pNext,
            [Out] out uint pceltFetched);

        [PreserveSig]
        int Skip(uint celt);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out, MarshalAs(UnmanagedType.Interface)] out ITTSEnum ppEnum);

        [PreserveSig]
        int Select(
            [In] ref Guid gModeID,
            [Out, MarshalAs(UnmanagedType.Interface)] out object ppITTSCentral,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pAudioDest);
    }
}
