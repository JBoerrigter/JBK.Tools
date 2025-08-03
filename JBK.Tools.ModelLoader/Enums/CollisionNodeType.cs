namespace JBK.Tools.ModelLoader.Enums
{
    [Flags]
    public enum CollisionNodeType
    {
        L_LEAF = 1,
        R_LEAF = 2,
        X_MIN = 4,
        X_MAX = 8,
        Y_MIN = 0x10,
        Y_MAX = 0x20,
        Z_MIN = 0x40,
        Z_MAX = 0x80,
        L_HIDDEN = 0x100,
        R_HIDDEN = 0x200,
        L_CAMERA = 0x400,
        R_CAMERA = 0x800,
        L_NOPICK = 0x1000,
        R_NOPICK = 0x2000,
        L_FLOOR = 0x4000,
        R_FLOOR = 0x8000
    }
}
