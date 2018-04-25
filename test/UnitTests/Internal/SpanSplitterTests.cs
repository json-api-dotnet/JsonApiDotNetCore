using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using Xunit;

namespace UnitTests.Internal
{
    public class SpanSplitterTests : SpanSplitterTestsBase
    {
        [Fact]
        public void StringWithDelimeterSplitsIntoCorrectNumberSubstrings()
        {
            GivenMultipleCommaDelimetedString();
            WhenSplittingIntoSubstrings();
            AssertCorrectSubstringsReturned();
        }

        [Fact]
        public void StringWithSingleDelimeterSplitsIntoCorrectNumberSubstrings()
        {
            GivenSingleCommaDelimetedString();
            WhenSplittingIntoSubstrings();
            AssertCorrectSubstringsReturned();
        }

        [Fact]
        public void StringWithNoDelimeterSplitsIntoSingleSubstring()
        {
            GivenNonCommaDelimetedString();
            WhenSplittingIntoSubstrings();
            AssertCorrectSubstringsReturned();
        }

        [Fact]
        public void StringWithDelimeterAtEndSplitsIntoCorrectSubstring()
        {
            GivenStringWithCommaDelimeterAtEnd();
            WhenSplittingIntoSubstrings();
            AssertCorrectSubstringsReturned();
        }

        [Fact]
        public void StringWithDelimeterAtBeginningSplitsIntoCorrectSubstring()
        {
            GivenStringWithCommaDelimeterAtBeginning();
            WhenSplittingIntoSubstrings();
            AssertCorrectSubstringsReturned();
        }
    }

    public abstract class SpanSplitterTestsBase
    {
        private string _baseString;
        private char _delimeter;
        private readonly List<string> _substrings = new List<string>();

        protected void GivenMultipleCommaDelimetedString()
        {
            _baseString = "This,Is,A,TestString";
            _delimeter = ',';
        }

        protected void GivenSingleCommaDelimetedString()
        {
            _baseString = "This,IsATestString";
            _delimeter = ',';
        }

        protected void GivenNonCommaDelimetedString()
        {
            _baseString = "ThisIsATestString";
        }

        protected void GivenStringWithCommaDelimeterAtEnd()
        {
            _baseString = "This,IsATestString,";
            _delimeter = ',';
        }

        protected void GivenStringWithCommaDelimeterAtBeginning()
        {
            _baseString = "/api/v1/articles";
            _delimeter = '/';
        }

        protected void WhenSplittingIntoSubstrings()
        {
            SpanSplitter spanSplitter;
            spanSplitter = _baseString.SpanSplit(_delimeter);
            for (var i = 0; i < spanSplitter.Count; i++)
            {
                var span = spanSplitter[i];
                _substrings.Add(span.ToString());
            }
        }

        protected void AssertCorrectSubstringsReturned()
        {
            Assert.NotEmpty(_substrings);
            var stringSplitArray = _baseString.Split(_delimeter);
            Assert.Equal(stringSplitArray.Length, _substrings.Count);
            Assert.True(stringSplitArray.SequenceEqual(_substrings));
        }
    }
}
