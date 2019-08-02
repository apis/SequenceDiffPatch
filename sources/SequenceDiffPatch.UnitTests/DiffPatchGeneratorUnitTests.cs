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
		public void Scenario1()
		{
			var patch = _diffPatchGenerator.ProduceDiffPatch("ABCDCBA".ToList(), "DCBAABB".ToList());

			Assert.That(patch, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 0, "ABC".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 7, "ABB".ToList())
			}));
		}

		[Test]
		public void Scenario2()
		{
			var patch = _diffPatchGenerator.ProduceDiffPatch("ABCDCBA2345".ToList(), "DCBAABD".ToList());

			Assert.That(patch, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 0, "ABC".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 7, "2".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Replace, 8, "ABD".ToList())
			}));
		}

		[Test]
		public void Scenario3()
		{
			var patch = _diffPatchGenerator.ProduceDiffPatch("ABCDEFGH".ToList(), "AB1CDE2FG".ToList());

			Assert.That(patch, Is.EquivalentTo(new List<IDiffPatchAction<char>>
			{
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 2, "1".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Insert, 5, "2".ToList()),
				new DiffPatchAction<char>(DiffPatchActionType.Remove, 7, "H".ToList())
			}));
		}

		[Test]
		public void Scenario4()
		{
			var sourceList = new List<int>();
			var destinationList = new List<int>();
			for (var index = 0; index < 10000; index++)
			{
				var sourceValue = index;
				var destinationValue = index;

				if (index == 0) destinationValue = 500;

				if (index == 1000) sourceValue = 2000;

				sourceList.Add(sourceValue);
				destinationList.Add(destinationValue);
			}

			var patch = _diffPatchGenerator.ProduceDiffPatch(sourceList, destinationList);

			Assert.That(patch, Is.EquivalentTo(new List<IDiffPatchAction<int>>
			{
				new DiffPatchAction<int>(DiffPatchActionType.Replace, 0, new List<int> {500}),
				new DiffPatchAction<int>(DiffPatchActionType.Replace, 1000, new List<int> {1000})
			}));
		}
	}
}