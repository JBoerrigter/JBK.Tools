namespace JBK.Tools.ModelLoader.Enums;

public enum VertexType
{
    // NOTE: The numeric values are *normalized* values used by the loader/exporter.
    // File formats differ between versions (e.g. v9/v10 had VT_STATIC and VT_LM_STATIC).
    Rigid = 0,
    Blend1 = 1,
    Blend2 = 2,
    Blend3 = 3,
    Blend4 = 4,

    /// <summary>
    /// v9/v10: VT_LM_STATIC (static vertex with a second UV set for lightmaps).
    /// Layout matches the later VT_RIGID_DOUBLE (v11+).
    /// </summary>
    LmStatic = 5,

    /// <summary>
    /// v11+: VT_RIGID_DOUBLE (rigid vertex with 2 UV sets).
    /// </summary>
    RigidDouble = 6,
}
