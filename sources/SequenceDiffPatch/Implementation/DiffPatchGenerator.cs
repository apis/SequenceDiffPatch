using System;
using System.Collections.Generic;

namespace SequenceDiffPatch.Implementation
{
	//
	// https://en.wikipedia.org/wiki/Longest_common_subsequence_problem
	//

	internal class DiffPatchGenerator : IDiffPatchGenerator
	{
		private IList<CellAction<T>> ProduceDiff<T>(IList<T> source, IList<T> destination)
		{
			var columns = source.Count + 1;
			var rows = destination.Count + 1;

			var cells = new Cell[rows * columns];
			var columnHeaders = new T[columns];
			var rowHeaders = new T[rows];

			FillCellMatrix(cells, rowHeaders, columnHeaders, rows, columns, source, destination);

			var stack = new Stack<CellAction<T>>();
			var currentCell = cells[rows * columns - 1];
			do
			{
				var direction = GetDirection(cells, currentCell.Y, currentCell.X, columns,
					out var newCurrentCell);

				if (direction == Direction.None)
					break;

				stack.Push(new CellAction<T>{
					ActionType = GetDiffActionType(direction, columnHeaders[currentCell.X], rowHeaders[currentCell.Y]),
					SourceItem = columnHeaders[currentCell.X],
					DestinationItem = rowHeaders[currentCell.Y]});
				currentCell = newCurrentCell;
			} while (true);

			var resultList = new List<CellAction<T>>();
			var length = stack.Count;
			for (var index = 0; index < length; index++)
			{
				var cellAction = stack.Pop();
				resultList.Add(cellAction);
			}

			return resultList;
		}

		public IList<IDiffPatchAction<T>> ProduceDiffPatch<T>(IList<T> source, IList<T> destination)
		{
			return ProducePatch(ProduceDiff(source, destination));
		}

		private DiffPatchActionType ConvertToDiffPatchActionType(DiffActionType diffActionType)
		{
			switch (diffActionType)
			{
				case DiffActionType.Insert:
					return DiffPatchActionType.Insert;

				case DiffActionType.Remove:
					return DiffPatchActionType.Remove;

				case DiffActionType.Replace:
					return DiffPatchActionType.Replace;

				default:
					throw new Exception("Unexpected DiffActionType");
			}
		}

		private IList<IDiffPatchAction<T>> ProducePatch<T>(IList<CellAction<T>> diffActions)
		{
			IList<IDiffPatchAction<T>> patchActions = new List<IDiffPatchAction<T>>();
			IList<T> items = new List<T>();
			var currentDiffActionType = DiffActionType.Same;
			var sourceIndex = 0;
			var chunkIndex = sourceIndex;

			foreach (var diffAction in diffActions)
			{
				if (diffAction.ActionType != currentDiffActionType)
				{
					if (currentDiffActionType != DiffActionType.Same)
					{
						patchActions.Add(new DiffPatchAction<T>(ConvertToDiffPatchActionType(currentDiffActionType), chunkIndex, items));
					}

					currentDiffActionType = diffAction.ActionType;
					items = new List<T>();
					chunkIndex = sourceIndex;
				}

				AddDiffActionType(diffAction, items, ref sourceIndex);
			}

			if (currentDiffActionType != DiffActionType.Same)
			{
				patchActions.Add(new DiffPatchAction<T>(ConvertToDiffPatchActionType(currentDiffActionType), chunkIndex, items));
			}

			return patchActions;
		}

		private static void AddDiffActionType<T>(CellAction<T> diffAction, IList<T> items, ref int sourceIndex)
		{
			switch (diffAction.ActionType)
			{
				case DiffActionType.Same:
					sourceIndex++;
					break;

				case DiffActionType.Insert:
					items.Add(diffAction.DestinationItem);
					break;

				case DiffActionType.Replace:
					sourceIndex++;
					items.Add(diffAction.DestinationItem);
					break;

				case DiffActionType.Remove:
					sourceIndex++;
					items.Add(diffAction.SourceItem);
					break;

				default:
					throw new Exception("Unexpected DiffActionType");
			}
		}

		private void FillCellMatrix<T>(Cell[] cells, T[] rowHeaders, T[] columnHeaders, int rows, int columns,
			IList<T> source, IList<T> destination)
		{
			cells[0] = new Cell {X = 0, Y = 0, Lcs = 0};
			InitializeColumn0(cells, columns, source, columnHeaders);
			InitializeRow0(cells, rows, columns, destination, rowHeaders);
			FillNonHeaderCells(cells, rows, columns, rowHeaders, columnHeaders);
		}

		private static void FillNonHeaderCells<T>(Cell[] cells, int rows, int columns, T[] rowHeaders,
			T[] columnHeaders)
		{
			for (var row = 1; row < rows; row++)
			for (var column = 1; column < columns; column++)
			{
				var cell = new Cell {X = column, Y = row};

				if (rowHeaders[row].Equals(columnHeaders[column]))
				{
					var diagonalCell = cells[(row - 1) * columns + column - 1];
					cell.Lcs = diagonalCell.Lcs + 1;
				}
				else
				{
					var upperCell = cells[(row - 1) * columns + column];
					var leftCell = cells[row * columns + column - 1];
					cell.Lcs = leftCell.Lcs > upperCell.Lcs ? leftCell.Lcs : upperCell.Lcs;
				}

				cells[row * columns + column] = cell;
			}
		}

		private static void InitializeRow0<T>(Cell[] cells, int rows, int columns, IList<T> destination, T[] rowHeaders)
		{
			for (var row = 1; row < rows; row++)
			{
				var cell = new Cell {X = 0, Y = row, Lcs = 0};
				cells[row * columns] = cell;
				rowHeaders[row] = destination[row - 1];
			}
		}

		private static void InitializeColumn0<T>(Cell[] cells, int columns, IList<T> source,
			T[] columnHeaders)
		{
			for (var column = 1; column < columns; column++)
			{
				var cell = new Cell {X = column, Y = 0, Lcs = 0};
				cells[column] = cell;
				columnHeaders[column] = source[column - 1];
			}
		}

		private DiffActionType GetDiffActionType<T>(Direction direction, T sourceItem, T destinationItem)
		{
			switch (direction)
			{
				case Direction.Diagonal:
					return sourceItem.Equals(destinationItem) ? DiffActionType.Same : DiffActionType.Replace;

				case Direction.Left:
					return DiffActionType.Remove;

				case Direction.Up:
					return DiffActionType.Insert;

				default:
					throw new Exception("Unexpected cell direction");
			}
		}

		private Direction GetDirection(Cell[] cells, int row, int column, int columns,
			out Cell cell)
		{
			if (row == 0 && column == 0)
			{
				cell = null;
				return Direction.None;
			}

			var upperCellLcs = row == 0 ? -1 : cells[(row - 1) * columns + column].Lcs;
			var leftCellLcs = column == 0 ? -1 : cells[row * columns + column - 1].Lcs;

			if (upperCellLcs == leftCellLcs)
			{
				cell = cells[(row - 1) * columns + column - 1];
				return Direction.Diagonal;
			}

			if (upperCellLcs > leftCellLcs)
			{
				cell = cells[(row - 1) * columns + column];
				return Direction.Up;
			}

			cell = cells[row * columns + column - 1];
			return Direction.Left;
		}
	}
}