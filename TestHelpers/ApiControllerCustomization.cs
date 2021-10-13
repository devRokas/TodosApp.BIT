using AutoFixture;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TestHelpers
{
    public class ApiControllerCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new CustomCompositeMetadataDetailsProvider());
            fixture.Inject(new ViewDataDictionary(
                fixture.Create<IModelMetadataProvider>(),
                 fixture.Create<ModelStateDictionary>()));
        }

        private class CustomCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
        {
            public void CreateBindingMetadata(BindingMetadataProviderContext context)
            {
                throw new System.NotImplementedException();
            }

            public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
            {
                throw new System.NotImplementedException();
            }

            public void CreateValidationMetadata(ValidationMetadataProviderContext context)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}