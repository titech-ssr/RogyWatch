using System;

/// <summary>
/// Provide API Server externally
/// </summary>
namespace APIServerModule
{
    /// <summary>
    /// APIs
    /// </summary>
    // API ex.               +-------argument size (byte unit)
    //                       |
    // 0b0000 0000          0b0000 0000         0b0000 1111     0b0010 1100......
    //   |L------L---- API                      L---------------------------- argument (optional)
    //   +------------ Kinect Version. 1 => V1, 0 => V2
    // GetDepth from KinectV2, argument size 0
    [Flags]
    public enum API : byte
    {
        GetDepth  = 0x00,
        RunMethod = 0x01,
    }

    /// <summary>
    /// API execution. success or failture
    /// </summary>
    [Flags]
    public enum Status : byte
    {
        Succeeded = 0x80,
        Failed = 0x00,
    }
}
