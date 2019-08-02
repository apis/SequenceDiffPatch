namespace SequenceDiffPatch.Implementation
{
	internal class CellAction<T>
	{
		public DiffActionType ActionType { get; set; }

		public T SourceItem { get; set; }

		public T DestinationItem { get; set; }
	}
}