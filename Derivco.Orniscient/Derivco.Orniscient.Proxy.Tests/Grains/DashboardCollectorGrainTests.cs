using System;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures;
using Orleans;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
	public class DashboardCollectorGrainTests : IClassFixture<DashboardCollectorGrainTestFixture>
	{
		protected static IGrainFactory GrainFactory => GrainClient.GrainFactory;

		[Fact]
		public async Task GetAll_ShouldReturnMoreThanOneSilo()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetAll();

			Assert.NotNull(reply);
			Assert.NotEmpty(reply);
		}

		[Fact]
		public async Task GetAll_DashboardCollectorGrainType_ShouldReturnOnlyDashboardCollectorGrainSilos()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetAll("Derivco.Orniscient.Proxy.Grains.DashboardCollectorGrain");

			Assert.Equal(true, reply.TrueForAll(g => g.TypeShortName.Equals("DashboardCollectorGrain")));
		}

		[Fact]
		public async Task GetAll_ManagementGrainType_ShouldReturnOnlyManagementGrainSilos()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetAll("Derivco.Orniscient.Proxy.Grains.ManagementGrain");

			Assert.Equal(true, reply.TrueForAll(g => g.TypeShortName.Equals("ManagementGrain")));
		}

		[Fact]
		public async Task GetGrainTypes_ShouldReturnAllActiveGrainTypes()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetGrainTypes();

			Assert.NotNull(reply);
			Assert.NotEmpty(reply);
		}

		[Fact]
		public async Task
			GetGrainTypes_SetTypeFilterToDashboardCollectorGrainAndManagementGrain_ShouldReturnOnlyDashboardCollectorAndManagementGrains
			()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			await grain.SetTypeFilter(new[]
			{
				new GrainType("Derivco.Orniscient.Proxy.Grains.DashboardCollectorGrain"),
				new GrainType("Derivco.Orniscient.Proxy.Grains.ManagementGrain")
			});
			var reply = await grain.GetGrainTypes();

			Assert.Equal(true,
				reply.All(g => g.ShortName.Equals("DashboardCollectorGrain") || g.ShortName.Equals("ManagementGrain")));
		}

		[Fact]
		public async Task GetSilos_ShouldReturnMoreThanOneSilo()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetSilos();

			Assert.NotNull(reply);
			Assert.NotEmpty(reply);
		}

		[Fact]
		public async Task GetGrainIdsForType_ShouldReturnActiveGrainIds()
		{
			var grain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

			var reply = await grain.GetGrainIdsForType("Derivco.Orniscient.Proxy.Grains.DashboardCollectorGrain");

			Assert.NotNull(reply);
			Assert.NotEmpty(reply);
			Assert.Equal(Guid.Empty.ToString(), reply[0]);
		}
	}
}