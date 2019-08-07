namespace SequenceDiffPatch.Implementation
{
	internal class CellAction<T>
	{
		public DiffPatchActionType ActionType { get; set; }

		public T SourceItem { get; set; }

		public T DestinationItem { get; set; }
	}
}