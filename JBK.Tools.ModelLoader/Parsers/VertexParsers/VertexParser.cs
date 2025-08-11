using System.Collections;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public abstract class VertexParser
{
    public abstract int GetVertexSize();
    public abstract IEnumerable GetVertexData(byte[] data);

    protected static T[] ReadVertexBuffer<T>(byte[] data) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        int count = data.Length / size;
        T[] result = new T[count];

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        nint basePtr = handle.AddrOfPinnedObject();

        for (int i = 0; i < count; i++)
        {
            result[i] = Marshal.PtrToStructure<T>(basePtr + i * size);
        }

        handle.Free();
        return result;
    }
}
