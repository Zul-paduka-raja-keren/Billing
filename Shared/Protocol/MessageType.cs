namespace Shared.Protocol;

public static class MessageType
{
    // Server -> Client
    public const string LOCK = "LOCK";
    public const string UNLOCK = "UNLOCK";
    public const string TIME_UPDATE = "TIME_UPDATE";
    public const string SHUTDOWN = "SHUTDOWN";
    public const string FORCE_LOGOUT = "FORCE_LOGOUT";
    
    // Client -> Server
    public const string HEARTBEAT = "HEARTBEAT";
    public const string IDLE_DETECTED = "IDLE_DETECTED";
    public const string REQUEST_EXTEND = "REQUEST_EXTEND";
    public const string CLIENT_INFO = "CLIENT_INFO";
}