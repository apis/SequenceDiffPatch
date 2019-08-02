using System.Collections.Generic;
using System.Linq;

namespace SequenceDiffPatch.Implementation
{
	internal class DiffPatchAction<T> : IDiffPatchAction<T>
	{
		public DiffPatchAction(DiffPatchActionType actionType, int index, IList<T> items)
		{
			ActionType = actionType;
			Index = index;
			Items = items;
		}

		public DiffPatchActionType ActionType { get; }

		public IList<T> Items { get; }

		public int Index { get; }

		public bool Equals(IDiffPatchAction<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ActionType == other.ActionType && Items.SequenceEqual(other.Items) && Index == other.Index;
		}

		public override string ToString()
		{
			return string.Format($"{ActionType} {Index} [{string.Join(",", Items.Select(item => item.ToString()))}]");
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((IDiffPatchAction<T>) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int) ActionType;

				var itemsHashCode = 0;
				if (Items != null)
					foreach (var item in Items)
						itemsHashCode = (itemsHashCode * 397) ^ item.GetHashCode();

				hashCode = (hashCode * 397) ^ itemsHashCode;
				hashCode = (hashCode * 397) ^ Index;
				return hashCode;
			}
		}
	}
}