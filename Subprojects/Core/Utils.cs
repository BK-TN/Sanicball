using System;
using System.Collections;

namespace SanicballCore
{
    public static class Utils
    {
        public static string GetTimeString(TimeSpan timeToUse)
        {
            return string.Format("{0:00}:{1:00}.{2:000}", timeToUse.Minutes, timeToUse.Seconds, timeToUse.Milliseconds);
        }

        public static string GetPosString(int pos)
        {
            if (pos % 10 == 1 && pos % 100 != 11) return pos + "st";
            if (pos % 10 == 2 && pos % 100 != 12) return pos + "nd";
            if (pos % 10 == 3 && pos % 100 != 13) return pos + "rd";
            return pos + "th";
        }

        public static void Write(this Lidgren.Network.NetBuffer target, Guid guid)
        {
            byte[] guidBytes = guid.ToByteArray();
            target.Write(guidBytes.Length);
            target.Write(guidBytes);
        }

        public static Guid ReadGuid(this Lidgren.Network.NetBuffer target)
        {
            int guidLength = target.ReadInt32();
            byte[] guidBytes = target.ReadBytes(guidLength);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Gets a random float between -1.0f and 1.0f
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static float NextFloatUniform(this Random rand)
        {
            return ((float)rand.NextDouble() - 0.5f) * 2f;
        }
    }
}