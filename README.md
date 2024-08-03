# door-notifier
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
 [![.NET](https://github.com/melgish/door-notifier/actions/workflows/dotnet.yml/badge.svg)](https://github.com/melgish/door-notifier/actions/workflows/dotnet.yaml)
 [![CodeQL](https://github.com/melgish/door-notifier/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/melgish/door-notifier/actions/workflows/github-code-scanning/codeql)
 [![codecov](https://codecov.io/github/melgish/door-notifier/graph/badge.svg?token=Q7HPKX12NH)](https://codecov.io/github/melgish/door-notifier)


A simple service to poll my garage door sensor and send me a notification when
I leave it open for too long

## Configuration

Application can be configured via appsettings.json

```json
{
  "Sensor": {
    // URI for the sensor endpoint.
    // Expects a text/plain response of either OPEN, CLOSED, or UNKNOWN
    "Uri": "http://192.168.1.111/garage-door-sensor"
  },
  "Notify": {
    // URI for NTFY notifier including topic.
    "Uri": "http://192.168.1.112/garage-door-sensor",
    // Security token for NTFY notifier with write access to topic.
    "Token": ""
  },
  "Worker": {
    // How long before notifying that the door is open.
    "After": "01:00:00",
    // How often to check the door state.
    "Interval": "00:01:00"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "DoorNotifier": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```



## Building

### Via Command line

```sh
dotnet build
dotnet run --project DoorNotifier
```

### Via Docker

In addition to the command below review ./publish.sh for creating container
with version tag.

```sh
docker compose build
docker compose up -d
```