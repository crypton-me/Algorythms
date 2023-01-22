namespace CryptoAlgorithm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string message = "The quick brown fox jumps over 13 lazy dogs.";

            var keccak   = Keccak.GetKeccakHash(message);
            var vigenere = Vigenere.GetVigenere(message);

            Console.WriteLine("message: " + message + "\r\n" + "Keccak: " + keccak + "\r\n" + "Vigenere: " + vigenere + "\r\n");
        }
    }
}