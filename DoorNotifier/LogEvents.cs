namespace DoorNotifier;

internal static class LogEvent
{
    // 1000-1999 Debug
    internal const int DoorClosed = 1001;
    internal const int DoorLeftOpen = 1002;
    internal const int DoorChanged = 1003;

    // 2000-2999 Info
    internal const int PollingStarted = 2001;
    internal const int ShuttingDown = 2002;
    internal const int DoorState = 2003;

    // 3000-3999 Warning
    internal const int GetStatusFailed = 3001;
    internal const int SendStatusFailed = 3002;
    internal const int SendStatusCode = 3003;

    // 4000-5000 Error
    internal const int WorkerError = 4001;
}
