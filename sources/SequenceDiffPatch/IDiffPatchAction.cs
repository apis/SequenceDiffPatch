using System;
using System.Collections.Generic;

namespace SequenceDiffPatch
{
	public interface IDiffPatchAction<T> : IEquatable<IDiffPatchAction<T>>
	{
		DiffPatchActionType ActionType { get; }

		IList<T> Items { get; }

		int Index { get; }
	}
}