#region header

// Troubleshooting.Common - Colors.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 9:09 PM

#endregion

#region using

using System.Drawing;

#endregion

namespace Troubleshooting.Common.Messaging
{
    /// <summary>
    ///     This class is intended to hold values for colors to be used for console formatting.
    /// </summary>
    public class Colors
    {
        public Color Error = Color.FromArgb(216, 80, 80);

        public Color Info = Color.PaleGreen;

        public Color Processing = Color.AliceBlue;

        public Color Warning = Color.Goldenrod;
    }
}