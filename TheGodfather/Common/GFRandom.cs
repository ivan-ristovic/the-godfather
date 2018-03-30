#region USING_DIRECTIVES
using System;
using System.Security.Cryptography;
#endregion

namespace TheGodfather.Common
{
    public sealed class GFRandom : IDisposable
    {
        private static GFRandom _instance;
        public static GFRandom Generator
        {
            get {
                if (_instance == null) {
                    _instance = new GFRandom();
                }
                return _instance;
            }
        }

        public bool IsDisposed { get; private set; } = false;
        private RandomNumberGenerator _rng { get; } = RandomNumberGenerator.Create();


        private GFRandom() { }


        public bool GetBool()
            => Next(2) == 0;

        public byte[] GetBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            var bytes = new byte[count];
            _rng.GetBytes(bytes);
            return bytes;
        }

        public void GetBytes(int count, out byte[] bytes)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            bytes = new byte[count];
            _rng.GetBytes(bytes);
        }

        public byte GetU8()
            => GetBytes(1)[0];

        public sbyte GetS8()
            => (sbyte)GetBytes(1)[0];

        public ushort GetU16()
            => BitConverter.ToUInt16(GetBytes(2), 0);

        public short GetS16()
            => BitConverter.ToInt16(GetBytes(2), 0);

        public uint GetU32()
            => BitConverter.ToUInt32(GetBytes(4), 0);

        public int GetS32()
            => BitConverter.ToInt32(GetBytes(4), 0);

        public ulong GetU64()
            => BitConverter.ToUInt64(GetBytes(8), 0);

        public long GetS64()
            => BitConverter.ToInt64(GetBytes(8), 0);

        public int Next()
            => Next(0, int.MaxValue);

        public int Next(int max)
            => Next(0, max);

        public int Next(int min, int max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum needs to be greater than minimum.");

            var offset = 0;
            if (min < 0)
                offset = -min;

            min += offset;
            max += offset;

            return Math.Abs(GetS32()) % (max - min) + min - offset;
        }

        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Cannot dispose this object twice.");

            IsDisposed = true;
            _rng.Dispose();
        }
    }
}