using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace TheGodfather.Common
{
    public sealed class SecureRandom : IDisposable
    {
        private readonly RandomNumberGenerator rng;


        public SecureRandom()
        {
            this.rng = RandomNumberGenerator.Create();
        }

        ~SecureRandom()
        {
            this.rng.Dispose();
        }


        public void Dispose()
            => this.rng.Dispose();

        public bool NextBool(int trueRatio = 1)
        {
            if (trueRatio <= 0)
                throw new ArgumentOutOfRangeException(nameof(trueRatio), "Ratio must be positive.");

            return this.Next() % (trueRatio + 1) > 0;
        }

        public byte[] GetBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            byte[] bytes = new byte[count];
            this.rng.GetBytes(bytes);
            return bytes;
        }

        public void GetBytes(int count, out byte[] bytes)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must get at least 1 byte.");

            bytes = new byte[count];
            this.rng.GetBytes(bytes);
        }

        public byte GetU8()
            => this.GetBytes(1)[0];

        public sbyte GetS8()
            => (sbyte)this.GetBytes(1)[0];

        public ushort GetU16()
            => BitConverter.ToUInt16(this.GetBytes(2), 0);

        public short GetS16()
            => BitConverter.ToInt16(this.GetBytes(2), 0);

        public uint GetU32()
            => BitConverter.ToUInt32(this.GetBytes(4), 0);

        public int GetS32()
            => BitConverter.ToInt32(this.GetBytes(4), 0);

        public ulong GetU64()
            => BitConverter.ToUInt64(this.GetBytes(8), 0);

        public long GetS64()
            => BitConverter.ToInt64(this.GetBytes(8), 0);

        public int Next()
            => this.Next(0, int.MaxValue);

        public int Next(int max)
            => this.Next(0, max);

        public int Next(int min, int max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum needs to be greater than minimum.");

            int offset = 0;
            if (min < 0)
                offset = -min;

            min += offset;
            max += offset;

            return Math.Abs(this.GetS32()) % (max - min) + min - offset;
        }

        public T ChooseRandomElement<T>(IEnumerable<T> collection)
            => collection.ElementAt(this.Next(collection.Count()));

        public char ChooseRandomChar(string str)
            => str[this.Next(0, str.Length)];

        public T? ChooseRandomEnumValue<T>() where T : Enum
        {
            Array v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(this.Next(v.Length));
        }
    }
}