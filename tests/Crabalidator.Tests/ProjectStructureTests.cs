namespace Crabalidator.Tests
{
    public class ProjectStructureTests
    {
        [Fact]
        public void Marker_Type_Exposes_Crabalidator_Assembly()
        {
            Assert.Equal("Crabalidator", typeof(CrabalidatorMarker).Assembly.GetName().Name);
        }
    }
}
