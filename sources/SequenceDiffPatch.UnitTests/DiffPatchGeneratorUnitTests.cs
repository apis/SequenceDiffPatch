using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SequenceDiffPatch.Implementation;

namespace SequenceDiffPatch.UnitTests
{
	[TestFixture]
	public class DiffPatchGeneratorUnitTests
	{
		[SetUp]
		public void SetUp()
		{
		}

		[TearDown]
		public void TearDown()
		{
		}

		private IDiffPatchGenerator _diffPatchGenerator;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_diffPatchGenerator = Factory.CreateDiffPatchGenerator();
		}

		[Test]
		public void EmptySource()
		{
			var sourceItems = "".ToList();
			var destinationItems = "321".ToList();
			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 0, "321".ToList())
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		[Test]
		public void EmptyDestination()
		{
			var sourceItems = "876".ToList();
			var destinationItems = "".ToList();
			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 0, "876".ToList())
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		[Test]
		public void Scenario1()
		{
			var sourceItems = "ABCDCBA".ToList();
			var destinationItems = "DCBAABB".ToList();
			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 0, "ABC".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 7, "ABB".ToList())
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		[Test]
		public void Scenario2()
		{
			var sourceItems = "ABCDCBA2345".ToList();
			var destinationItems = "DCBAABD".ToList();
			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 0, "ABC".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 7, "2".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Replace, 8, "ABD".ToList())
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		[Test]
		public void Scenario3()
		{
			var sourceItems = "ABCDEFGH".ToList();
			var destinationItems = "AB1CDE2FG".ToList();
			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 2, "1".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 5, "2".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 7, "H".ToList())
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		[Test]
		public void Scenario4()
		{
			var sourceItems = new List<int>();
			var destinationItems = new List<int>();
			for (var index = 0; index < 10000; index++)
			{
				var sourceValue = index;
				var destinationValue = index;

				if (index == 0)
					destinationValue = 500;

				if (index == 1000)
					sourceValue = 2000;

				sourceItems.Add(sourceValue);
				destinationItems.Add(destinationValue);
			}

			var diffPatchActions = _diffPatchGenerator.ProduceDiffPatch(sourceItems, destinationItems);

			Assert.That(diffPatchActions, Is.EquivalentTo(new List<IDiffPatchAction<int>>
			{
				new DiffPatchAction<int>(DiffPatchActionType.Replace, 0, new List<int> {500}),
				new DiffPatchAction<int>(DiffPatchActionType.Replace, 1000, new List<int> {1000})
			}));

			Assert.That(destinationItems, Is.EquivalentTo(BuildDestinationItems(sourceItems, diffPatchActions)));
		}

		private IList<T> BuildDestinationItems<T>(IList<T> sourceItems, IList<IDiffPatchAction<T>> diffPatchActions)
		{
			var destinationItems = new List<T>();

			var currentSourceItemsIndex = 0;

			foreach (var diffPatchAction in diffPatchActions)
			{
				var count = diffPatchAction.Index - currentSourceItemsIndex;

				destinationItems.AddRange(
					((List<T>)sourceItems).GetRange(currentSourceItemsIndex, count));

				currentSourceItemsIndex += count;

				switch (diffPatchAction.ActionType)
				{
					case DiffPatchActionType.Insert:
						destinationItems.AddRange(diffPatchAction.Items);
						break;

					case DiffPatchActionType.Remove:
						currentSourceItemsIndex += diffPatchAction.Items.Count;
						break;

					case DiffPatchActionType.Replace:
						destinationItems.AddRange(diffPatchAction.Items);
						currentSourceItemsIndex += diffPatchAction.Items.Count;
						break;

					default:
						throw new Exception("Not supported DiffPatchActionType!");
				}
			}

			destinationItems.AddRange(
				((List<T>)sourceItems).GetRange(currentSourceItemsIndex, sourceItems.Count - currentSourceItemsIndex));

			return destinationItems;
		}
	}
}