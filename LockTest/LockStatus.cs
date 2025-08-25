namespace LockTest
{
    public enum LockCtrl : byte
    {
        Query = 0x80,
        Unlock = 0x81,
        AllUnlock = 0x8A
    }


    public enum LockStatus : byte
    {
        Lock = 0x11,
        Unlock = 0x00,
    }
}
