# EasySocket.Core

![Nuget](https://img.shields.io/nuget/v/EasySocket.Core.svg) ![Nuget](https://img.shields.io/nuget/dt/EasySocket.Core.svg)

## Introduction

이 라이브러리는 Java vertx tool-kit의 이벤트 핸들러 메소드와 유사하게 작성되었으며 가능한 한 사용하기 쉽게 만들어졌습니다.

This library is made similar to Java's vertx tool-kit's event handler method and is intended to be as easy to use as possible.

모든 통신은 비동기 적으로 수행됩니다. 서버의 경우 요청이 완료되고 응답 대기로 되기 때문에 흐름을 제어할 수 있습니다. 통신 성능을 위해 순서성이 필요없는 서비스에서는 성능이 조금 저하 될 수도 있다.

All communication is done asynchronously. For the server, the flow is controlled because the request completes and waits for a response. Performance may be slightly degraded for services that do not require ordering for communication performance.


### Feature

- 비전문가도 TCP 네트워크 통신을 쉽게 구현할 수있는 간단한 인터페이스 만 제공하며 필요한 경우 내부 액세스를 허용하여 범용성을 갖습니다.

  Even non-professionals provide only a simple interface to easily implement TCP network communications, and have universality by allowing internal access if necessary.

- 네트워크 통신 간의 흐름에 대한 자세한 로그를 제공하고 구현을 확인하기위한 테스트 코드를 포함합니다.

  Provides a detailed log of the flow between network communications and includes test code to verify implementation.

- 람다 식의 간결한 코드를 제공합니다.

  Provide concise code of lambda expressions.


## Usage


### Configuration

##### Server
```json
{
  "EasyServer": {
    "MaxConnection": 1000,
    "ListenBackLog": 128,
    "LingerTime": 0,

    "EasySocket": {
      "NoDelay": true,
      "ReceiveBufferSize": 2048,
      "SendBufferSize": 2048,
      "IdleTimeout": 0
    }
  }
}
```

서버는 최대 연결 수로 풀링 된 소켓을 관리하고 각 소켓의 IdleTimeout을 사용하여 좀비 소켓을 관리합니다.

The server manages pooled sockets with the maximum number of connections and manages zombie sockets using the IdleTimeout of each socket.

##### Client
```json
{
  "EasyClient": {

    "EasySocket": {
      "NoDelay": true,
      "ReceiveBufferSize": 2048,
      "SendBufferSize": 2048,
      "ReadTimeout": 0
    }
  }
}
```

클라이언트는 요청 후 응답 시간 초과 인 ReadTimeout을 설정할 수 있습니다.

The client can set the ReadTimeout, which is the response timeout after request.

### Sample Sources

##### Request Using Dependency Injection

```cs
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

ServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton(config)
    .AddLogging(logger =>
    {
        logger.AddConsole();
        logger.SetMinimumLevel(LogLevel.Debug);
    })
    // case of server
    .AddTransient<EasyServer>()
    // case of client
    .AddTransient<EasyClient>()
    .AddTransient<Startup>()
    .BuildServiceProvider();
```

appsettings.json에 구성 설정을로드하고 ServiceProvider를 사용하여 Logger 및 EasyServer 또는 EasyClient의 서비스를 등록하십시오.

Load configuration settings in appsettings.json and register service of Logger and EasyServer or EasyClient using ServiceProvider.

> In an environment that already allows DI, only the server or client needs to register as a service.

##### Server codes
```cs
_server.ConnectHandler(socket =>
{
    // If the client connection is successful, a socket is created and the logic proceeds.
    string socketId = socket.SocketId;

    _logger.LogInformation($"[{socketId}] Connected");

    socket.ExceptionHandler(exception =>
    {
        _logger.LogError($"[{socketId}] Exception - {exception.Message}");
    });
    socket.IdleTimeoutHandler(() =>
    {
        _logger.LogInformation($"[{socketId}] Idle Timeout");
    });
    socket.CloseHandler(() =>
    {
        _logger.LogInformation($"[{socketId}] Closed");
    });
    socket.Receive(receiveData =>
    {
        string recvStringData = Encoding.UTF8.GetString(receiveData);
        _logger.LogInformation($"[{socketId}] Receive data - {recvStringData}, size:{receiveData.Length}");

        socket.Send(receiveData, size =>
        {
            string sendStringData = Encoding.UTF8.GetString(receiveData);
            _logger.LogInformation($"[{socketId}] Send data - {sendStringData}, size:{size}");
        });
    });

    int count = _server.GetConnectedCount();
    _logger.LogInformation($"[{socketId}] now total connection count - {count}");

});
_server.ExceptionHandler(exception =>
{
    _logger.LogError($"Exception - {exception.Message}");
});

_server.Start(addr, port);
```

서버는 시작할 때 ConnectHandler를 등록해야만 시작할 수 있습니다.

The server can be started only by registering the ConnectHandler when starting.

##### Client codes
```cs
_client.ConnectHandler(socket =>
{
    // If the server connection is successful, a socket is created and the logic proceeds.
    string socketId = socket.SocketId;

    _logger.LogInformation($"[{socketId}] Connected");

    socket.ExceptionHandler(exception =>
    {
        _logger.LogError($"[{socketId}] Exception - {exception.Message}");
    });
    socket.ReadTimeoutHandler(() =>
    {
        _logger.LogInformation($"[{socketId}] Read Timeout");
    });
    socket.CloseHandler(() =>
    {
        _logger.LogInformation($"[{socketId}] Closed");
    });

    byte[] sendData = Encoding.UTF8.GetBytes("testData");
    socket.Send(sendData, size =>
    {
        string sendStringData = Encoding.UTF8.GetString(sendData);
        _logger.LogInformation($"[{socketId}] Send data - {sendStringData}, size:{size}");
    });

    socket.Receive(receiveData =>
    {
        string recvStringData = Encoding.UTF8.GetString(receiveData);
        _logger.LogInformation($"[{socketId}] Receive data - {recvStringData}, size:{receiveData.Length}");
    });
});
_client.ExceptionHandler(exception =>
{
    _logger.LogError($"Exception - {exception.Message}");
});

_client.Connect(addr, port);
```

클라이언트도 반드시 ConnectHandler를 미리 구현해야 합니다.

Clients must also implement ConnectHandler in advance.

##### Please refer to the actual usage as an example source included in the project.


## License

Everything found in this repo is licensed under an MIT license. See the LICENSE file for specifics.

## Next Version

서버를 실행할 때 IHostbuilder를 사용하는 구조화 된 설계

More structured design using IHostbuilder when running server