using System;

namespace Derivco.Orniscient.Proxy.Extensions
{
    public static class GuidExtensions
    {
        public static int ToInt(this Guid that)
        {
            var bytes = that.ToByteArray();
            return bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;
        }

        public static Guid ToGuid(this int that)
        {
            return new Guid(that, 0, 0, new byte[] {0, 0, 0, 0, 0, 0, 0, 0});
        }
    }
}