using System.Numerics;

namespace GNFSCore.Interfaces
{
	public interface ITerm : ICloneable<ITerm>
	{
		int Exponent { get; }
		BigInteger CoEfficient { get; set; }
	}
}
