namespace DoorNotifier;

/// <summary>
/// Constants for logging service events.
/// </summary>
internal static class LogEvent
{
    // 1000-1999 Debug
    public const int DoorClosed = 1001;
    public const int DoorLeftOpen = 1002;
    public const int DoorChanged = 1003;

    // 2000-2999 Info
    public const int PollingStarted = 2001;
    public const int ShuttingDown = 2002;
    public const int DoorState = 2003;

    // 3000-3999 Warning
    public const int GetStatusFailed = 3001;
    public const int SendStatusFailed = 3002;
    public const int SendStatusCode = 3003;

    // 4000-5000 Error
    public const int WorkerError = 4001;
}