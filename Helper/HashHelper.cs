using System.Text;
using System.Security.Cryptography;

namespace DFTelegram.Helper
{
    public class HashHelper
    {
        #nullable disable
        private static StringBuilder _sb;
        private static SHA1 _mySHA1;
        #nullable restore

        public HashHelper()
        {
            if (_sb == null)
            {
                _sb = new StringBuilder();
            }
            if (_mySHA1 == null)
            {
                _mySHA1 = SHA1.Create();
            }
        }

        public static string CalculationHash(Stream fileStream){
            fileStream.Position = 0;
            byte[] hashValue = _mySHA1.ComputeHash(fileStream);
            return PrintHash(hashValue);
        }

        private static string PrintHash(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                _sb.Append($"{array[i]:X2}");
            }
            string result = _sb.ToString();
            _sb.Clear();
            return result;
        }
    }
}