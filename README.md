# WinSocat [![Testing][ci-badge]][ci] [![Release][release-badge]][release] [![Nuget][nuget-badge]][nuget]

[ci]: https://github.com/firejox/WinSocat/actions/workflows/unit-test.yml
[ci-badge]: https://github.com/firejox/WinSocat/actions/workflows/unit-test.yml/badge.svg
[release]: https://github.com/firejox/WinSocat/releases
[release-badge]: https://img.shields.io/github/v/release/firejox/WinSocat?include_prereleases
[nuget]: https://www.nuget.org/packages/winsocat
[nuget-badge]: https://img.shields.io/nuget/vpre/winsocat

WinSocat is a socat-like program specific on Windows platform. It can bridge Windows named pipe and other general I/O, e.g., STDIO, TCP, the STDIO of Process.

## Installation

[Install .NET 6 or newer](https://get.dot.net) and install via `dotnet tool`

```
dotnet tool install -g winsocat
```

## Command Form

The WinSocat is accept two address pattern

```
winsocat.exe [address1] [address2]
```

The `address1` can accept `STDIO`, `TCP-LISTEN`, `TCP`, `NPIPE`, `NPIPE-LISTEN`, `EXEC`, `WSL`, `UNIX`, `UNIX-LISTEN` socket types.

The `address2` can accept `STDIO`, `TCP`, `NPIPE`, `EXEC`, `WSL`, `UNIX` socket types.

## Examples

* It can bridge standard input/output and tcp connection to address **127.0.0.1** on port **80**.
```
winsocat STDIO TCP:127.0.0.1:80
```

* It can forward from Windows named pipe to remote tcp socket.
```
winsocat NPIPE-LISTEN:myPipe TCP:127.0.0.1:80
```

* It can use Windows named pipe for network connection
```
winsocat NPIPE:RemoteServer:RemotePipe STDIO
```

* It can create reverse shell.
```
winsocat EXEC:C:\Windows\syetem32\cmd.exe TCP:127.0.0.1:8000
```

* It can bridge Windows named pipe and [unix socket on Windows](https://devblogs.microsoft.com/commandline/af_unix-comes-to-windows/)
```
winsocat NPIPE-LISTEN:fooPipe UNIX:foo.sock
```

### Interact with WSL(Windows Subsystem for Linux)

WinSocat provide the syntax sugar for WSL program. Hence, this example
```
winsocat STDIO WSL:cat,distribution=Ubuntu,user=root
```
would be equivalent to
```
winsocat STDIO EXEC:"C:\Windows\System32\wsl.exe -d Ubuntu -u root cat"
```
if `wsl.exe` is located on `C:\Windows\System32`.

The `distribution` and `user` are optional parameters. If these parameters are not specified, it will run with the default options.
You can combine the `socat` of WSL distribution for the communication between WSL and Windows Host.

* Windows named pipe forwarding to WSL Unix Socket
```
winsocat NPIPE-LISTEN:fooPipe WSL:"socat STDIO unix-connect:foo.sock"
```

* WSL Unix Socket forwarding to Windows named pipe
```
socat unix-listen:foo.sock,fork EXEC:"/path/to/winsocat.exe STDIO NPIPE:fooPipe"
```