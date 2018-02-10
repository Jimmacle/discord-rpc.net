using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace DiscordRpc
{
    public class RichPresence : IDisposable
    {
        private string LibName = "discord-rpc.dll";
        private readonly ClearPresenceDel _clearPresence;
        private readonly bool _enableIoThread;
        private readonly IntPtr _hDiscordLib;

        private readonly InitializeDel _initialize;
        private readonly RespondDel _respond;
        private readonly RunCallbacksDel _runCallbacks;
        private readonly ShutdownDel _shutdown;
        private readonly UpdatePresenceDel _updatePresence;
        private bool _disposed;
        private readonly string _tempPath;

        private DiscordEventHandlers _eventHandlers;

        public RichPresence()
        {
            if(Environment.Is64BitProcess)
            {
                LibName = "discord-rpc-64.dll";
            }
            // Embedding the native lib makes deployment easier. TODO possibly embed multiple platforms for a cross-platform release?
            // https://stackoverflow.com/questions/4651803/loading-a-library-dynamically-in-linux-or-osx
            // https://stackoverflow.com/questions/9954548/sigsegv-when-p-invoking-dlopen
            // RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

            _tempPath = Path.Combine(Path.GetTempPath(), LibName);
            if (File.Exists(_tempPath))
                File.Delete(_tempPath);

            using (var fs = File.Create(_tempPath))
            using (var s = typeof(RichPresence).Assembly.GetManifestResourceStream($"DiscordRpc.{LibName}.gz"))
            using (var gz = new GZipStream(s, CompressionMode.Decompress))
            {
                gz.CopyTo(fs);
            }

            _hDiscordLib = NativeMethods.LoadLibrary(_tempPath);

            if (_hDiscordLib == IntPtr.Zero)
                throw new TypeLoadException($"Failed to load Discord RPC lib from {LibName}: {NativeMethods.GetLastError()}\nTemp Path: {_tempPath}");

            // Marshal methods from native lib
            _initialize = WrapNativeFunction<InitializeDel>(_hDiscordLib, RpcMethods.Initialize);
            _shutdown = WrapNativeFunction<ShutdownDel>(_hDiscordLib, RpcMethods.Shutdown);
            _runCallbacks = WrapNativeFunction<RunCallbacksDel>(_hDiscordLib, RpcMethods.RunCallbacks);
            _updatePresence = WrapNativeFunction<UpdatePresenceDel>(_hDiscordLib, RpcMethods.UpdatePresence);
            _clearPresence = WrapNativeFunction<ClearPresenceDel>(_hDiscordLib, RpcMethods.ClearPresence);
            _respond = WrapNativeFunction<RespondDel>(_hDiscordLib, RpcMethods.Respond);

            // Marshal methods to native lib
            _eventHandlers = new DiscordEventHandlers
            {
                Ready = OnReady,
                Disconnected = OnDisconnected,
                Errored = OnErrored,
                JoinGame = OnJoinGame,
                SpectateGame = OnSpectateGame,
                JoinRequest = OnJoinRequest
            };
        }

        public event ReadyDel Ready;

        public event DisconnectedDel Disconnected;

        public event ErroredDel Errored;

        public event JoinGameDel JoinGame;

        public event SpectateGameDel SpectateGame;

        public event JoinRequestDel JoinRequest;

        public void Initialize(string applicationId, string steamAppId = null) => _initialize(applicationId, ref _eventHandlers, 1, steamAppId);

        public void Shutdown() => _shutdown();

        public void RunCallbacks() => _runCallbacks();

        public void Update(RichPresenceInfo presence) => _updatePresence(ref presence);

        public void Update(RichPresenceBuilder builder) => Update(builder.Build());

        public void Clear() => _clearPresence();

        public void Respond(string userId, DiscordReply reply) => _respond(userId, reply);

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RichPresence));

            _disposed = true;
            Shutdown();
            NativeMethods.FreeLibrary(_hDiscordLib);
            File.Delete(_tempPath);
        }

        private T WrapNativeFunction<T>(IntPtr hModule, string procName)
        {
            var handle = NativeMethods.GetProcAddress(hModule, procName);
            if (handle == IntPtr.Zero)
                throw new TypeLoadException($"ProcAddress {procName} not found in module {hModule}");

            return Marshal.GetDelegateForFunctionPointer<T>(handle);
        }

        private void OnReady() => Ready?.Invoke();

        private void OnDisconnected(int errorCode, string message) => Disconnected?.Invoke(errorCode, message);

        private void OnErrored(int errorCode, string message) => Errored?.Invoke(errorCode, message);

        private void OnJoinGame(string joinSecret) => JoinGame?.Invoke(joinSecret);

        private void OnSpectateGame(string spectateSecret) => SpectateGame?.Invoke(spectateSecret);

        private void OnJoinRequest(ref DiscordJoinRequest request) => JoinRequest?.Invoke(ref request);

        private static class RpcMethods
        {
            public const string Initialize = "Discord_Initialize";
            public const string Shutdown = "Discord_Shutdown";
            public const string RunCallbacks = "Discord_RunCallbacks";
            // Only used with non-threaded builds of discord-rpc which this library doesn't currently support.
            public const string UpdateConnection = "Discord_UpdateConnection";
            public const string UpdatePresence = "Discord_UpdatePresence";
            public const string ClearPresence = "Discord_ClearPresence";
            public const string Respond = "Discord_Respond";
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DiscordEventHandlers
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ReadyDel Ready;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public DisconnectedDel Disconnected;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ErroredDel Errored;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public JoinGameDel JoinGame;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public SpectateGameDel SpectateGame;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public JoinRequestDel JoinRequest;
        }

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DisconnectedDel(
            [MarshalAs(UnmanagedType.I4)] int errorCode,
            [MarshalAs(UnmanagedType.LPStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ErroredDel(
            [MarshalAs(UnmanagedType.I4)] int errorCode,
            [MarshalAs(UnmanagedType.LPStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JoinGameDel(
            [MarshalAs(UnmanagedType.LPStr)] string joinSecret);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JoinRequestDel(ref DiscordJoinRequest request);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReadyDel();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SpectateGameDel(
            [MarshalAs(UnmanagedType.LPStr)] string spectateSecret);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void InitializeDel(
            [MarshalAs(UnmanagedType.LPStr)] string applicationId,
            ref DiscordEventHandlers handlers,
            [MarshalAs(UnmanagedType.I4)] int autoRegister,
            [MarshalAs(UnmanagedType.LPStr)] string optionalSteamId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void ShutdownDel();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RunCallbacksDel();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void UpdatePresenceDel(ref RichPresenceInfo presence);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void ClearPresenceDel();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void RespondDel(
            [MarshalAs(UnmanagedType.LPStr)] string userId,
            [MarshalAs(UnmanagedType.I4)] DiscordReply reply);

        #endregion
    }
}