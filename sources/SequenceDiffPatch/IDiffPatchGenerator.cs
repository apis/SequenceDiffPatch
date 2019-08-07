using System.Collections.Generic;

namespace SequenceDiffPatch
{
	public interface IDiffPatchGenerator
	{
		IList<IDiffPatchAction<T>> ProduceDiffPatch<T>(IList<T> source, IList<T> destination, bool includeSameItems = false);
	}
}