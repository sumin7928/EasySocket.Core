# EasySocket.Core

![Nuget](https://img.shields.io/nuget/v/EasySocket.Core.svg) ![Nuget](https://img.shields.io/nuget/dt/EasySocket.Core.svg)

# Introduction

- Even non-professionals provide only a simple interface to easily implement TCP network communications, and have universality by allowing internal access if necessary.

- Provides a detailed log of the flow between network communications and includes test code to verify implementation.

- Provide concise code of lambda expressions.

# Requirements


| .NETStandard.Version |
|----------------------|
| 2.+                  |


# Server usage

## Configure

```json
{
  "EasyServer": {
    "MaxConnection": 1000,
    "ListenBackLog": 128,
    "LingerTime": 0,

    "EasySocket": {
      "ReceiveBufferSize": 2048,
      "SendBufferSize": 2048,
      "IdleTimeout": 0
    }
  }
}
```

서버는 최대 커넥션

## Use Dependency Injection
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
    .AddTransient<EasyServer>()
    .AddTransient<Startup>()
    .BuildServiceProvider();
```


### sample source
```cs
_server.ConnectHandler(socket =>
{
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

설명 블라블라...

# Client usage

## Configure

```json
{
  "EasyClient": {
    "NoDelay": true,

    "EasySocket": {
      "ReceiveBufferSize": 2048,
      "SendBufferSize": 2048,
      "ReadTimeout": 0
    }
  }
}
```

서버는 최대 커넥션

## Use Dependency Injection
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
    .AddTransient<EasyClient>()
    .AddTransient<Startup>()
    .BuildServiceProvider();
```


### sample source
```cs
_client.ConnectHandler(socket =>
{
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

설명 블라블라...

# Next Version

- More structured design using IHostbuilder when running server