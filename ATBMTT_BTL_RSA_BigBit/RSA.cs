using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ATBMTT_BTL_RSA_BigBit
{
    class RSA
    {
        private BigInteger p, q;
        public BigInteger a, b;
        private BigInteger minN = BigInteger.Pow(2, 10);
        public BigInteger n;

        public RSA(BigInteger p, BigInteger q)
        {
            this.p = p;
            this.q = q;
            this.n = BigInteger.Multiply(p, q);
            FindB();
            FindA();
        }
        public RSA()
        {

        }
        public bool IndependencePQ()
        {
            return p != q;
        }

        public bool PrimeNumber(BigInteger n)
        {
            if (n < 2) return false;
            var sqrtN = Sqrt(n);
            for (BigInteger l = 2; l <= sqrtN; l++)
            {
                if (n % l == 0) return false;
            }
            return true;
        }

        public bool CheckMinN()
        {
            return n > minN;
        }

        private BigInteger GCD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger r = a % b;
                a = b;
                b = r;
            }
            return a;
        }

        private BigInteger TotientEuler()
        {
            return BigInteger.Multiply(p - 1, q - 1);
        }

        private void FindB()
        {
            var rand = new Random();
            var maxB = BigInteger.Pow(2, 21);
            do
            {
                b = BigIntegerUtils.RandomInRange(2, TotientEuler() - 1, rand);
            } while (GCD(b, TotientEuler()) != 1 || b >= maxB);
        }

        private void FindA()
        {
            a = ModInverse(b, TotientEuler());
        }

        public BigInteger CalculatePow(BigInteger a, BigInteger b, BigInteger n)
        {
            return BigInteger.ModPow(a, b, n);
        }

        private BigInteger Sqrt(BigInteger n)
        {
            if (n == 0 || n == 1)
                return n;

            BigInteger x = n / 2;
            BigInteger y = (x + n / x) / 2;

            while (y < x)
            {
                x = y;
                y = (x + n / x) / 2;
            }

            return x;
        }

        private BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = y;

                y = x - q * y;
                x = t;
            }

            if (x < 0)
                x += m0;

            return x;
        }
    }

    // Helper class for generating random BigInteger within a range
    static class BigIntegerUtils
    {
        public static BigInteger RandomInRange(BigInteger start, BigInteger end, Random rand)
        {
            var max = end - start;

            byte[] bytes = max.ToByteArray();
            BigInteger result;
            do
            {
                rand.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F; // Ensure positive number
                result = new BigInteger(bytes);
            } while (result >= max);

            return result + start;
        }
    }
}
