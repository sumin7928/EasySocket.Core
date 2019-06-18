# EasySocket.Core

![Nuget](https://img.shields.io/nuget/v/EasySocket.Core.svg) ![Nuget](https://img.shields.io/nuget/dt/EasySocket.Core.svg)

## Introduction

- 네트워크 비전문가도 TCP 네트워크 통신을 쉽게 구현할 수 있게 간략한 인터페이스만 제공하며 꼭 필요한 부분에 있어서는 내부 접근을 허용해 범용성을 가졌다.

- 예외성이 많은 TCP 처리(잘린 패킷 및 중간 끊어짐 현상 등)를 라이브러리 안에서 다 케어해주며 사용자는 핸들러를 통해 응답만 받으면 된다.

- 네트워크 통신간 흐름에서 디테일한 로그를 제공하며 구현을 확인 할 수 있는 테스트 코드도 포함된다.

- 람다 표현 방식을 권장하며 코드의 간결함 제공한다.

## Requirements

필요 SDK 및 Nuget 패키지

| .NETStandard.Version |
|----------------------|
| 2.+                  |

## Getting Started

인터페이스 사용에 주석으로 기본적인 설명이 있지만 실 구현에 있어서 적용 방식을 설명한다.

### Server

서버 객체 생성은 EasySocketFactory에서 IEasySocketServer 인터페이스를 반환하는 디폴트 옵션을 가진 방식과 사용자가 설정한 옵션을 넣어서 생성하는 방식이 있다.

> configuration으로 설정하는 방식은 개발 예정 

##### EasySocketFactory

```
public static IEasySocketServer CreateServer();
public static IEasySocketServer CreateServer(ServerOptions options);
```

###### example

```
// create default server
var server =  EasySocketFactory.CreateServer();
```

```
// create server with options
var options = new ServerOptions();
... set options
var server = EasySocketFactory.CreateServer(options);
```

##### ServerOptions

| Options | Description | DefaultValue |  
|--------------|---------------------|----------|
| Host     | 서버 listen Host        | 127.0.0.1|
| Port     | 서버 listen port        | 12000 |
| ListenBackLog     | 서버 listen 대기 큐 size | 128 |
| NoDelay     | Nagle 알고리즘 사용 여부| true ( 사용 안함 ) |
| Linger     | LingerOption 적용 처리 | LingerOption( true, 2 ) |

##### IEasySocketServer

```
ILogger<EasySocketServer> Logger {get; set;}
void ConnectHandler(Action<INPTcpSocket> action);
void ExceptionHandler(Action<Exception> action);
void Run();
void Run(int port);
void Stop();
```

###### example

- 핸들러 등록

```
// register connect handler
server.ConnectHandler(socket =>
{
    // socket = IEasySocket was wrapped from System.Net.Socket
    ... process socket logic
});

// register exception handler
server.ExceptionHandler(exception =>
{
    // exception = System.Exception
    log.Error("received server exception", exception);
});
```

- 서버 구동

```
// run server direct port
server.Run(<port>);

or

// run server with option port
server.Run();
```

> **Notice** : 서버 구동에 있어서는 반드시 ConnectHandler 등록이 필요하다.

### Client

클라이언트 객체 생성은 EasySocketFactory에서 IEasySocketClient 인터페이스를 반환하는 서버와 동일하게 디폴트 옵션을 가진 방식과 사용자가 설정한 옵션을 넣어서 생성하는 방식이 있다.

##### EasySocketFactory

```
public static IEasySocketClient CreateClient();
public static IEasySocketClient CreateClient(TcpClientOptions options);
```

###### example

```
// create default client
var client = NPStandardTcp.CreateClient();
```

```
// create client with options
var options = new TcpClientOptions();
... set options
var client = NPStandardTcp.CreateClient(options);
```

##### ClientOptions

| Options | Description | DefaultValue |  
|--------------|---------------------|----------|
| Host     | 서버 주소 정보        | 127.0.0.1 |
| Port     | 서버 포트 정보        | 12000 |
| NoDelay     | Nagle 알고리즘 사용 여부| true ( 사용 안함 ) |

##### IEsaySocketClient

```
ILogger<EasySocketClient> Logger {get; set;}
void Connect();
void Connect(string host, int port);
void ConnectHandler(Action<INPTcpSocket> action);
void ExceptionHandler(Action<Exception> action);
```

###### example

- 핸들러 등록

```
// register connect handler
client.ConnectHandler(socket =>
{
    // socket = IEasySocket was wrapped from System.Net.Socket
    ... process socket logic
});

// register exception handler
client.ExceptionHandler(exception =>
{
    // exception = System.Exception
    log.Error("received client exception", exception);
});
```

- 클라이언트 연결

```
// connect server direct host and port
client.Connect(<host>,<port>);

or

// connect server with client option
client.Connect();
```

> **Notice** : 클라이언트 연결에 있어서는 반드시 ConnectHandler 등록이 필요하다.

### Socket

##### SocketOptions

| Options | Description | DefaultValue |  
|--------------|---------------------|----------|
| IdleTimeout     | Idle 상태의 소켓 close 처리 타임아웃 | Server:900,000ms (15분), Client:0 |
| ReadTimeout     | 요청 수 응답대기 타임아웃 | Server:0, Client:300,000ms (5분) |
| ReceiveBufferSize     | 소켓 receive buffer size | 2048 |
| SendBufferSize     | 소켓 send buffer size | 2048 |

##### IEsaySocket

```
ILogger Logger { get; }
Socket Socket { get; }
string SocketId { get; }
SocketOptions SocketOptions { get; }
Dictionary<object, object> Items { get; }

void ReadTimeoutHandler(Action<string> action);
void IdleTimeoutHandler(Action<string> action);
void CloseHandler(Action<string> action);
void ExceptionHandler(Action<Exception> action);
void Receive(Action<byte[]> action);
void Receive(TotalLengthObject totalLengthObject, Action<byte[]> receiveBuffer);
void Send(byte[] sendData, Action<int> length);
void Send(byte[] sendData, int offset, int size, Action<int> length);
void Send(byte[] sendData);
void Send(byte[] sendData, int offset, int size);
void Close();
```

###### example

- 핸들러 등록

```
// register read timeout handler
socket.ReadTimeoutHandler(socketId =>
{
    // socketId = socket unique number
    log.InfoFormat("read timeout socket - id: {0}", socketId);
});

// register idle timeout handler
socket.IdleTimeoutHandler(socketId =>
{
    // socketId = socket unique number
    log.InfoFormat("idle timeout socket - id: {0}", socketId);
});

// register close handler
socket.CloseHandler(socketId =>
{
    // socketId = socket unique number
    log.InfoFormat("closed socket - id: {0}", socketId);
});

// register exception handler
socket.ExceptionHandler(exception =>
{
    log.Error("socket exception", exception);
});
```

- Send 함수

```
// send fixed byte array
socket.Send(sendData, sendSize =>
{
    // sendSize = completed send size
    log.InfoFormat("complete send - size : {0}", sendSize);

});

// send dynamic byte array
int sendOffset = 0;
int sendLength = sendData.Length;

socket.Send(sendData, sendOffset, sendLength, sendSize =>
{
    // sendSize = completed send size
    log.InfoFormat("complete send - size : {0}", sendSize);

});

```

- Receive 함수

```
socket.Receive(receivedData =>
{
    // receivedData = socket received byte[]
    log.InfoFormat("complete receive - size : {0}", receivedData.Length);
});

// receive fixed length packet
TotalLengthObject totalLengthObject = new TotalLengthObject();
totalLengthObject.TotalLengthOffset = 0;
totalLengthObject.TotalLengthSize = 4;
totalLengthObject.IsBigEndian = false;

socket.Receive(totalLengthObject, totalLengthSize, receivedData =>
{
    // receivedData = socket received byte[] (fixed length)
    log.InfoFormat("complete receive - size : {0}", receivedData.Length);
});
```

- Close 함수

```
socket.Close();
```

### Issue & Future

- 서버 Run시에 Main Thread Block으로 프로세스가 종료되는 것을 막는걸 따로 구현 해야 함

