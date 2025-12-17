namespace DAM.Core.Constants
{
    public static class WmiQueries
    {
        public const string LogicalDiskToPartition = "ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{0}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";
        public const string PartitionToDisk = "ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{0}'}} WHERE AssocClass = Win32_PartitionToDisk";
        public const string DiskDriveToDiskMedia = "ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{0}'}} WHERE AssocClass = Win32_DiskDriveToDiskMedia";
        public const string DeviceConnect = "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
        "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";

        public const string DeviceDisconnect = "SELECT * FROM __InstanceDeletionEvent WITHIN 2 " +
            "WHERE TargetInstance ISA 'Win32_LogicalDisk' AND TargetInstance.DriveType = 2";
    }
}
