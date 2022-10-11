# WinSocat [![Testing](https://github.com/firejox/WinSocat/actions/workflows/unit-test.yml/badge.svg)](https://github.com/firejox/WinSocat/actions/workflows/unit-test.yml) [![](https://img.shields.io/github/v/release/firejox/WinSocat?include_prereleases)](https://github.com/firejox/WinSocat/releases/latest)

WinSocat is a socat-like program specific on Windows platform. It can bridge Windows named pipe and other general I/O, e.g., STDIO, TCP, the STDIO of Process.

## Prerequisite

WinSocat is built under .Net 6.0. Make sure the corresponding .Net runtime is installed before using it.

## Installation

You can download binary from [release](https://github.com/firejox/WinSocat/releases), or build from source.

## Command Form

The WinSocat is accept two address pattern

```
winsocat.exe [address1] [address2]
```

The `address1` can accept `STDIO`, `TCP-LISTEN`, `TCP`, `NPIPE`, `NPIPE-LISTEN`, `EXEC`, `WSL` socket types.

The `address2` can accept `STDIO`, `TCP`, `NPIPE`, `EXEC`, `WSL` socket types.

## Examples

* It can bridge standard input/output and tcp connection to address **127.0.0.1** on port **80**.
```
winsocat.exe STDIO TCP:127.0.0.1:80
```

* It can forward from Windows named pipe to remote tcp socket.
```
winsocat.exe NPIPE-LISTEN:myPipe TCP:127.0.0.1:80
```

* It can use Windows named pipe for network connection
```
winsocat.exe NPIPE:RemoteServer:RemotePipe STDIO
```

* It can create reverse shell.
```
winsocat.exe EXEC:C:\Windows\syetem32\cmd.exe TCP:127.0.0.1:8000
```

### Interact with WSL(Windows Subsystem for Linux)

WinSocat provide the syntax sugar for WSL program. Hence, this example
```
winsocat.exe STDIO WSL:cat,distribution=Ubuntu,user=root
```
would be equivalent to
```
winsocat.exe STDIO EXEC:"C:\Windows\System32\wsl.exe -d Ubuntu -u root cat"
```
if `wsl.exe` is located on `C:\Windows\System32`.

The `distribution` and `user` are optional parameters. If these parameters are not specified, it will run with the default options.
You can combine the `socat` of WSL distribution for the communication between WSL and Windows Host.

* Windows named pipe forwarding to WSL Unix Socket
```
winsocat.exe NPIPE-LISTEN:fooPipe WSL:"socat STDIO unix-connect:foo.sock"
```

* WSL Unix Socket forwarding to Windows named pipe
```
socat unix-listen:foo.sock EXEC:"/path/to/winsocat.exe STDIO NPIPE:fooPipe"
```