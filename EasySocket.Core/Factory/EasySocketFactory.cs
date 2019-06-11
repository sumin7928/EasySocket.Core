using EasySocket.Core.Networks;
using EasySocket.Core.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasySocket.Core.Factory
{
    public class EasySocketFactory
    {
        /// <summary>
        /// create server with default options.
        /// </summary>
        public static IEasySocketServer CreateServer()
        {
            return new EasySocketServer( new ServerOptions() );
        }

        /// <summary>
        /// create server with custom options.
        /// </summary>
        /// <param name="options"> TcpServerOptions </param>
        public static IEasySocketServer CreateServer(ServerOptions options)
        {
            return new EasySocketServer( options );
        }

        /// <summary>
        /// create server with custom options.
        /// </summary>
        /// <param name="options"> TcpServerOptions </param>
        public static IEasySocketServer CreateServer( IConfiguration config, string section = "EasySocketServer" )
        {
            IConfigurationSection configurationSection = config.GetSection( section );
            ServerOptions serverOptions = new ServerOptions();
            if ( configurationSection[ "Host" ] != null )
            {
                serverOptions.Host = configurationSection[ "Host" ];
            }
            if ( configurationSection[ "Port" ] != null )
            {
                serverOptions.Port = int.Parse( configurationSection[ "Port" ] );
            }
            if ( configurationSection[ "IdleTimeout" ] != null )
            {
                serverOptions.IdleTimeout = int.Parse( configurationSection[ "IdleTimeout" ] );
            }
            if ( configurationSection[ "ReceiveBufferSize" ] != null )
            {
                serverOptions.ReceiveBufferSize = int.Parse( configurationSection[ "ReceiveBufferSize" ] );
            }
            if ( configurationSection[ "SendBufferSize" ] != null )
            {
                serverOptions.SendBufferSize = int.Parse( configurationSection[ "SendBufferSize" ] );
            }
            if ( configurationSection[ "ListenBackLog" ] != null )
            {
                serverOptions.ListenBackLog = int.Parse( configurationSection[ "ListenBackLog" ] );
            }
            if ( configurationSection[ "NoDelay" ] != null )
            {
                serverOptions.NoDelay = bool.Parse( configurationSection[ "NoDelay" ] );
            }
            if ( configurationSection[ "ListenBackLog" ] != null )
            {
                serverOptions.ListenBackLog = int.Parse( configurationSection[ "ListenBackLog" ] );
            }
            if ( configurationSection[ "Linger" ] != null )
            {
                // need to check
                bool enabled = bool.Parse( configurationSection[ "Linger.Enabled" ] );
                int lingerTime = int.Parse( configurationSection[ "Linger.Time" ] );
                LingerOption lingerOption = new LingerOption( enabled, lingerTime );
                serverOptions.Linger = lingerOption;
            }

            return new EasySocketServer( serverOptions );
        }

        /// <summary>
        /// create client with default options.
        /// </summary>
        public static IEasySocketClient CreateClient()
        {
            return new EasySocketClient( new ClientOptions() );
        }

        /// <summary>
        /// create client with custom options.
        /// </summary>
        /// <param name="options"> TcpClientOptions </param>
        public static IEasySocketClient CreateClient( ClientOptions options )
        {
            return new EasySocketClient( options);
        }

        public static IEasySocketClient CreateClient( IConfiguration config, string section = "EasySocketClient" )
        {
            IConfigurationSection configurationSection = config.GetSection( section );
            ClientOptions clientOptions = new ClientOptions();
            if ( configurationSection[ "Host" ] != null )
            {
                clientOptions.Host = configurationSection[ "Host" ];
            }
            if ( configurationSection[ "Port" ] != null )
            {
                clientOptions.Port = int.Parse( configurationSection[ "Port" ] );
            }
            if ( configurationSection[ "ReadTimeout" ] != null )
            {
                clientOptions.ReadTimeout = int.Parse( configurationSection[ "ReadTimeout" ] );
            }
            if ( configurationSection[ "ReceiveBufferSize" ] != null )
            {
                clientOptions.ReceiveBufferSize = int.Parse( configurationSection[ "ReceiveBufferSize" ] );
            }
            if ( configurationSection[ "SendBufferSize" ] != null )
            {
                clientOptions.SendBufferSize = int.Parse( configurationSection[ "SendBufferSize" ] );
            }
            if ( configurationSection[ "NoDelay" ] != null )
            {
                clientOptions.NoDelay = bool.Parse( configurationSection[ "NoDelay" ] );
            }

            return new EasySocketClient( clientOptions );
        }

    }
}
