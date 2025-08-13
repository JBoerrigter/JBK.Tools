namespace JBK.Tools.ModelLoader.Tests
{
    public class GbFileLoaderTests
    {
        [Fact]
        public void GbFileLoader_ShouldParse_Header_v12()
        {
            var model = GbFileLoader.LoadFromFile("TestFiles/v12.gb");

            Assert.Equal(12, model.header.Version);
            Assert.Equal(1, model.header.MeshCount);
            Assert.Equal(2511u, model.header.IndexCount);
            Assert.Equal(19528u, model.header.ClsSize);
            Assert.Equal(71u, model.header.StringSize);
            Assert.Equal(13u, model.header.SzOption);
            Assert.Equal(1, model.header.Flags);
            Assert.Equal(2, model.header.BoneCount);
            Assert.Equal(1u, model.header.BoneIndexCount);
            Assert.Equal(1u, model.header.MaterialCount);
            Assert.Equal(1u, model.header.MaterialFrameCount);
            Assert.Equal(8u, model.header.AnimCount);
            Assert.Equal(1, model.header.AnimFileCount);
        }

        [Fact]
        public void GbFileLoader_ShouldParse_Header_v8()
        {
            var model = GbFileLoader.LoadFromFile("TestFiles/v8.gb");

            Assert.Equal(8, model.header.Version);
            Assert.Equal(1, model.header.MeshCount);
            Assert.Equal(1946u, model.header.IndexCount);
            Assert.Equal(0u, model.header.ClsSize);
            Assert.Equal(51u, model.header.StringSize);
            Assert.Equal(0u, model.header.SzOption);
            Assert.Equal(0, model.header.Flags);
            Assert.Equal(28, model.header.BoneCount);
            Assert.Equal(22u, model.header.BoneIndexCount);
            Assert.Equal(1u, model.header.MaterialCount);
            Assert.Equal(1u, model.header.MaterialFrameCount);
            Assert.Equal(0u, model.header.AnimCount);
            Assert.Equal(0, model.header.AnimFileCount);
        }
    }
}
