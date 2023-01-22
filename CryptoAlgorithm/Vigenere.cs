using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoAlgorithm
{
    internal class Vigenere
    {
        static private string _firstRow = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static private string _key = "cryptii";

        public static string GetVigenere(string message)
        {
            var fullKey = GetFullKey(message.Length);

            return  GetCodedPhrase(message, fullKey);
        }
        private static string GetCodedPhrase(string textToCode, string fullKey)
        {
            var matrix = GetMatrix();
            var result = new char[textToCode.Length];
            var nonLetterCount = 0;
            for (int z = 0; z < textToCode.Length; z++)
            {
                if (!char.IsLetter(textToCode[z]))
                {
                    result[z] = textToCode[z];
                    nonLetterCount++;
                }
                else
                {
                    var i = _firstRow.IndexOf(fullKey[z - nonLetterCount]);
                    var j = _firstRow.IndexOf(char.ToUpper(textToCode[z]));

                    result[z] = char.IsLower(textToCode[z]) ?
                        char.ToLower(matrix[i][j]) :
                        result[z] = matrix[i][j];
                }
            }
            return new string(result);
        }

        private static string GetFullKey(int length)
        {
            if (_key.Length >= length)
                return _key.Substring(0, length).ToUpperInvariant();

            var result = _key;
            do
            {
                result = string.Concat(result, _key);
            } while (result.Length < length);
            return result.Substring(0, length).ToUpperInvariant();
        }

        private static List<string> GetMatrix()
        {
            var strings = new List<string>();
            var temp = _firstRow;
            foreach (var ch in _firstRow.ToCharArray())
            {
                strings.Add(temp);
                temp = GoLeft(temp);
            }
            return strings;
        }

        private static string GoLeft(string str)
        {
            var arr = str.ToCharArray();
            var last = arr[0];
            for (int i = 0; i < str.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
            arr[str.Length - 1] = last;
            return new string(arr);
        }
    }
}
