using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlgorithm
{

    internal class Keccak
    {
        static private byte MATRIX_SIZE = 5;
        static private byte TWO_POW_L = 64; // 1600 / 25 = 2**l
        static private byte ROUNDS_NUMBER = 24; //12 + 2l
        static private int RATE = 576;
        static private int CAPACITY = 1024;

        static private byte[,] R = {
        {0, 36, 3, 41, 18},
        {1, 44, 10, 45, 2},
        {62, 6, 43, 15, 61},
        {28, 55, 25, 21, 56},
        {27, 20, 39, 8, 14}
    };

        static private ulong[] RC = {
        0x0000000000000001,
        0x0000000000008082,
        0x800000000000808A,
        0x8000000080008000,
        0x000000000000808B,
        0x0000000080000001,
        0x8000000080008081,
        0x8000000000008009,
        0x000000000000008A,
        0x0000000000000088,
        0x0000000080008009,
        0x000000008000000A,
        0x000000008000808B,
        0x800000000000008B,
        0x8000000000008089,
        0x8000000000008003,
        0x8000000000008002,
        0x8000000000000080,
        0x000000000000800A,
        0x800000008000000A,
        0x8000000080008081,
        0x8000000000008080,
        0x0000000080000001,
        0x8000000080008008
    };

        static private ulong[,] B = new ulong[MATRIX_SIZE, MATRIX_SIZE];
        static private ulong[] C = new ulong[MATRIX_SIZE];
        static private ulong[] D = new ulong[MATRIX_SIZE];

        static private ulong[,] KeccakTransformation(ulong[,] A)
        {
            for (byte i = 0; i < ROUNDS_NUMBER; i++)
                A = Round(A, RC[i]);
            return A;
        }

        static private ulong[,] Round(ulong[,] A, ulong RC_i)
        {
            byte i, j;

            //0 step
            for (i = 0; i < 5; i++)
                C[i] = A[i, 0] ^ A[i, 1] ^ A[i, 2] ^ A[i, 3] ^ A[i, 4];
            for (i = 0; i < 5; i++)
                D[i] = C[(i + 4) % 5] ^ ROT(C[(i + 1) % 5], 1);
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                    A[i, j] = A[i, j] ^ D[i];

            //p, п steps
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                    B[j, (2 * i + 3 * j) % 5] = ROT(A[i, j], R[i, j]);

            //x step
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                    A[i, j] = B[i, j] ^ ((~B[(i + 1) % 5, j]) & B[(i + 2) % 5, j]);

            //l step
            A[0, 0] = A[0, 0] ^ RC_i;

            return A;
        }

        static private ulong ROT(ulong l, byte b)
        {
            return ((l << (b % TWO_POW_L)) | (l >> (TWO_POW_L - (b % TWO_POW_L))));
        }

        static public string GetKeccakHash(string message)
        {
            var messageBytes = ConvertStrigToByteList(message);
            var result = Hash(messageBytes);
            return ConvertByteArrayToString(result);
        }

        static List<byte> ConvertStrigToByteList(string str)
        {
            List<byte> result = new List<byte>(str.Length);

            foreach (char ch in str)
            {
                result.Add((byte)ch);
            }
            return result;
        }

        static public string ConvertByteArrayToString(byte[] b)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(16);
            for (int i = 0; i < Math.Min(b.Length, int.MaxValue - 1); i++)
                sb.Append(string.Format("{0:X2}", b[i]));
            return sb.ToString();
        }
        static private byte[] Hash(List<byte> message)
        {
            var rateInBytes = (int)(RATE / 8);
            // Message completes 01 00 .. 00 80
            message = (List<byte>)MultiplyStringTillRate(rateInBytes, message);

            var size = message.Count / rateInBytes;

            ulong[] P = new ulong[size * MATRIX_SIZE * MATRIX_SIZE];
            var count = 0;
            byte i = 0, j = 0;

            foreach (var ch in message)
            {
                if (j > (RATE / TWO_POW_L - 1))
                {
                    j = 0;
                    i++;
                }
                count++;
                if ((count * 8 % TWO_POW_L) == 0)
                {
                    P[size * i + j] = ConvertBytesArrayToLong(
                        message.GetRange(count - TWO_POW_L / 8, 8).ToArray()
                        );
                    j++;
                }
            }

            //Initialization
            ulong[,] S = new ulong[5, 5];
            for (i = 0; i < 5; i++)
                for (j = 0; j < 5; j++)
                    S[i, j] = 0;

            //Absorb
            for (int z = 0; z < size; z++)
            {
                for (i = 0; i < 5; i++)
                    for (j = 0; j < 5; j++)
                        if ((i + j * 5) < (RATE / TWO_POW_L))
                        {
                            S[i, j] = S[i, j] ^ P[size * z + i + j * 5];
                        }
                KeccakTransformation(S);
            }

            //Squeeze
            byte a = 0;
            byte d_max = (byte)(CAPACITY / (2 * 8));
            List<byte> result = new List<byte>(d_max);

            for (; ; )
            {
                for (i = 0; i < 5; i++)
                    for (j = 0; j < 5; j++)
                        if ((5 * i + j) < (RATE / TWO_POW_L))
                        {
                            if (a >= d_max)
                                i = j = 5;
                            else
                            {
                                result.AddRange(ConvertLongToBytesArray(S[j, i]));
                                a = (byte)result.Count;
                            }
                        }
                if (a >= d_max)
                    break;
                KeccakTransformation(S);
            }

            return result.GetRange(0, d_max).ToArray();
        }

        private static IEnumerable<byte> MultiplyStringTillRate(int rateInbytes, List<byte> message)
        {
            byte start = 0x01;
            byte finish = 0x80;

            var diff = rateInbytes - message.Count;

            switch (diff)
            {
                case 0:
                    return message;
                case 1:
                    message.Add(finish);
                    return message;
                default:
                    message.Add(start);
                    break;
            }

            int n = rateInbytes / message.Count;

            if (n > 0)
            {
                message.AddRange(GetMiddlePart(rateInbytes - message.Count));
            }
            else
            {
                message.AddRange(GetMiddlePart(message.Count - rateInbytes * n));
            }

            message.Add(finish);
            return message;
        }

        private static IEnumerable<byte> GetMiddlePart(int v)
        {
            byte middle = 0x00;
            var result = new List<byte>();
            while (v-- > 1)
            {
                result.Add(middle);
            }
            return result;
        }

        static private ulong ConvertBytesArrayToLong(byte[] bVal)
        {
            ulong ulVal = 0L;
            for (byte i = 8, j = 0; i > 0; i--)
            {
                ulVal += (ulong)((bVal[i - 1] & 0xF0) >> 4) * (ulong)Math.Pow(16.0F, 15 - (j++));
                ulVal += (ulong)(bVal[i - 1] & 0x0F) * (ulong)Math.Pow(16.0F, 15 - (j++));
            }
            return ulVal;
        }

        static private byte[] ConvertLongToBytesArray(ulong ulVal)
        {
            byte[] bVal = new byte[8];
            byte a = 0;
            do
            {
                bVal[a] = (byte)((ulVal % 16) * 1);
                ulVal = ulVal / 16;
                bVal[a] += (byte)((ulVal % 16) * 16);
                a++;
            }
            while (15 < (ulVal = ulVal / 16));
            while (a < 8)
            {
                bVal[a++] = (byte)ulVal;
                ulVal = 0;
            }

            return bVal;
        }
    }
}
