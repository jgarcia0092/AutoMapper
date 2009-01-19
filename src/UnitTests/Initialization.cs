using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace Initialization
	{
		public class When_configuring_through_the_initializer : AutoMapperSpecBase
		{
			protected override void Establish_context()
			{
				AutoMapper.CreateMap<Model1, Dto1>();

				AutoMapper.Initalize(x => x.CreateMap<Model2, Dto2>());
			}

			[Test]
			public void Should_use_the_configuration_specified_in_the_initializer()
			{
				AutoMapper.FindTypeMapFor<Model2, Dto2>().ShouldNotBeNull();
			}

			[Test]
			public void Should_wipe_out_any_existing_mappings()
			{
				AutoMapper.FindTypeMapFor<Model1, Dto1>().ShouldBeNull();
			}

			private class Dto2
			{
			}

			private class Model2
			{
			}

			private class Dto1
			{
			}

			private class Model1
			{
			}
		}

	}
}