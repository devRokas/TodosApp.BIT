using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.AutoMoq.WebApi;
using AutoFixture.Xunit2;

namespace TestHelpers.Attributes
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }
}