using System.Runtime.CompilerServices;
using SequenceDiffPatch.Implementation;

[assembly: InternalsVisibleTo("SequenceDiffPatch.UnitTests, PublicKey=" +
                              "00240000048000009400000006020000002400005253413100040000010001006d8fdd1d01eff5" +
                              "6cb8510c5dcf87311678fc21cd3cf168fec12d4699d7622409b643da419aa9f3a20a50bdff3407" +
                              "eb2be15854159755232853c69149bf20230d055c9ae8a53b9cf52cc4930276f326d319bc5242c1" +
                              "2775b8abee7c2ae881bae33366484338e3d012ad40b510a2457e060858c004039584079133550e" +
                              "bc0c758c")]

namespace SequenceDiffPatch
{
	public static class Factory
	{
		public static IDiffPatchGenerator CreateDiffPatchGenerator()
		{
			return new DiffPatchGenerator();
		}
	}
}