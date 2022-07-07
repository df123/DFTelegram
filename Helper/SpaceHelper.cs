namespace DFTelegram.Helper
{
    public class SpaceHelper
    {
        public static double GetHomeAvailableMB()
        {
            return StorageUnitConversionHelper.ByteToMB(GetAnyDriveAvailable("/home"));
        }

        public static double GetRootAvailableMB()
        {
            return StorageUnitConversionHelper.ByteToMB(GetAnyDriveAvailable("/"));
        }

        public static double GetAnyDriveAvailable(string name)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true && d.Name == name)
                {
                    return d.AvailableFreeSpace;
                }
            }
            return -1;
        }

        public static void DeleteFile(string path)
        {
            if ((!string.IsNullOrEmpty(path)) && (!string.IsNullOrWhiteSpace(path)))
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                if (System.IO.File.Exists(path + ".temp"))
                {
                    System.IO.File.Delete(path + ".temp");
                }
            }
        }
    }
}