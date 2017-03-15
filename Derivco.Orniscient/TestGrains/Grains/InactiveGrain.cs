using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace TestGrains.Grains
{
	public class InactiveGrain: Grain, IInactiveGrain
	{
		[OrniscientMethod]
		public Task DoFuckAll()
		{
			return TaskDone.Done;
		}
	}
}
