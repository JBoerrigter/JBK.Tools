using System.Numerics;

namespace JBK.Tools.ModelLoader
{
    public static class Extensions
    {
        /// <summary>
        /// Convert from DirectX to System.Numerics (row-major to column-major)
        /// </summary>
        public static Matrix4x4 ToColumnMajor(this Matrix4x4 matrix)
        {
            return new Matrix4x4(
                matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                matrix.M14, matrix.M24, matrix.M34, matrix.M44);
        }
    }
}
