using System.Threading.Tasks;
using Orleans;

namespace TestGrains.Grains
{
	public interface IInactiveGrain : IGrainWithIntegerKey
	{
		Task DoNothing();
	}
}
