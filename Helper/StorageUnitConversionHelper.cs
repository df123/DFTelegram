namespace DFTelegram.Helper
{
    public class StorageUnitConversionHelper
    {
        public static double ByteToGB(double sizes)
        {
            return ByteToMB(sizes) / 1024d;
        }

        public static double ByteToMB(double sizes)
        {
            return ByteToKB(sizes) / 1024d;
        }

        public static double ByteToKB(double sizes)
        {
            return sizes / 1024d;
        }
    }
}