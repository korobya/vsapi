﻿using Vintagestory.API.Common;

namespace Vintagestory.API.Server
{
    /// <summary>
    /// API for general Server features
    /// </summary>
    public interface IServerAPI
    {
        string ServerIp { get; }

        /// <summary>
        /// All players known to the server (which joined at least once)
        /// </summary>
        IServerPlayer[] Players { get; }

        /// <summary>
        /// The servers current configuration as configured in the serverconfig.json. You can set the values but you need to call MarkDirty() to have them saved
        /// </summary>
        IServerConfig Config { get; }

        /// <summary>
        /// Marks the config dirty for saving
        /// </summary>
        void MarkConfigDirty();


        /// <summary>
        /// Returns the servers current run phase
        /// </summary>
        /// <value></value>
        EnumServerRunPhase CurrentRunPhase { get; }

        /// <summary>
        /// Returns whether the current server a dedicated server
        /// </summary>
        /// <value></value>
        bool IsDedicated { get; }


        /// <summary>
        /// Determines if the server process has been asked to terminate.
        /// Use this when you need to save data in a method registered using RegisterOnSave() before server quits.
        /// </summary>
        /// <value><i>true</i>
        ///   if server is about to shutdown</value>
        bool IsShuttingDown { get; }

        /// <summary>
        /// Gracefully shuts down the server
        /// </summary>
        /// <returns></returns>
        void ShutDown();


        long TotalReceivedBytes { get; }
        long TotalSentBytes { get; }

        /// <summary>
        /// Returns the number of seconds the server has been running since last restart
        /// </summary>  
        /// <value>Server uptime in seconds</value>
        int ServerUptimeSeconds { get; }

        /// <summary>
        /// Server uptime in milliseconds
        /// </summary>
        /// <value></value>
        long ServerUptimeMilliseconds { get; }

        /// <summary>
        /// Returns the number of seconds the current world has been running. This is the playtime displayed on the singleplayer world list.
        /// </summary>
        int TotalWorldPlayTime { get; }
        

        /// <summary>
        /// Returns a logging interface to log any log level message
        /// </summary>
        /// <returns></returns>
        ILogger Logger { get; }


        /// <summary>
        /// Log given message with type = EnumLogType.Chat
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogChat(string message, params object[] args);

        
        /// <summary>
        /// Log given message with type = EnumLogType.Build
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogBuild(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.VerboseDebug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogVerboseDebug(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.Notification
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogNotification(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.Warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Log given message with type = EnumLogType.Fatal
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogFatal(string message, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        void LogEvent(string message, params object[] args);
    }
}