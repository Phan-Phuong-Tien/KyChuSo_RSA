using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ATBMTT_BTL_RSA_BigBit
{
    class BigIntegerHelper
    {
        public static BigInteger GenerateRandomBigInteger(int bitLength, Random random)
        {
            byte[] data = new byte[(bitLength + 7) / 8];
            random.NextBytes(data);
            data[data.Length - 1] &= (byte)(0xFF >> (data.Length * 8 - bitLength));
            return new BigInteger(data);
        }
    }
}
