using System.Collections.Generic;
using System.Linq;
using WalletWasabi.Blockchain.Analysis.Clustering;
using Xunit;

namespace WalletWasabi.Tests.UnitTests.Blockchain.Analysis.Clustering;

public class SmartLabelTests
{
	[Fact]
	public void LabelParsingTests()
	{
		var label = new SmartLabel();
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel("");
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel(null!);
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel(null!, null!);
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel(" ");
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel(",");
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel(":");
		Assert.Equal("", label);
		Assert.Equal(Array.Empty<string>(), label);
		Assert.Equal(Array.Empty<string>(), label.AsSpan().ToArray());

		label = new SmartLabel("foo");
		Assert.Equal("foo", label);
		Assert.Equal(new[] { "foo" }, label);
		Assert.Equal(new[] { "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel("foo", "bar");
		Assert.Equal("bar, foo", label);
		Assert.Equal(new[] { "bar", "foo" }, label);
		Assert.Equal(new[] { "bar", "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel("foo bar");
		Assert.Equal("foo bar", label);
		Assert.Equal(new[] { "foo bar" }, label);
		Assert.Equal(new[] { "foo bar" }, label.AsSpan().ToArray());

		label = new SmartLabel("foo bar", "Buz quX@");
		Assert.Equal("Buz quX@, foo bar", label);
		Assert.Equal(new[] { "Buz quX@", "foo bar" }, label);
		Assert.Equal(new[] { "Buz quX@", "foo bar" }, label.AsSpan().ToArray());

		label = new SmartLabel(new List<string>() { "foo", "bar" });
		Assert.Equal("bar, foo", label);
		Assert.Equal(new[] { "bar", "foo" }, label);
		Assert.Equal(new[] { "bar", "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel("  foo    ");
		Assert.Equal("foo", label);
		Assert.Equal(new[] { "foo" }, label);
		Assert.Equal(new[] { "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel("foo      ", "      bar");
		Assert.Equal("bar, foo", label);
		Assert.Equal(new[] { "bar", "foo" }, label);
		Assert.Equal(new[] { "bar", "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel(new List<string>() { "   foo   ", "   bar    " });
		Assert.Equal("bar, foo", label);
		Assert.Equal(new[] { "bar", "foo" }, label);
		Assert.Equal(new[] { "bar", "foo" }, label.AsSpan().ToArray());

		label = new SmartLabel(new List<string>() { "foo:", ":bar", null!, ":buz:", ",", ": , :", "qux:quux", "corge,grault", "", "  ", " , garply, waldo,", " : ,  :  ,  fred  , : , :   plugh, : , : ," });
		Assert.Equal("bar, buz, corge, foo, fred, garply, grault, plugh, quux, qux, waldo", label);
		Assert.Equal(new[] { "bar", "buz", "corge", "foo", "fred", "garply", "grault", "plugh", "quux", "qux", "waldo" }, label);
		Assert.Equal(new[] { "bar", "buz", "corge", "foo", "fred", "garply", "grault", "plugh", "quux", "qux", "waldo" }, label.AsSpan().ToArray());

		label = new SmartLabel(",: foo::bar :buz:,: , :qux:quux, corge,grault  , garply, waldo, : ,  :  ,  fred  , : , :   plugh, : , : ,");
		Assert.Equal("bar, buz, corge, foo, fred, garply, grault, plugh, quux, qux, waldo", label);
		Assert.Equal(new[] { "bar", "buz", "corge", "foo", "fred", "garply", "grault", "plugh", "quux", "qux", "waldo" }, label);
		Assert.Equal(new[] { "bar", "buz", "corge", "foo", "fred", "garply", "grault", "plugh", "quux", "qux", "waldo" }, label.AsSpan().ToArray());
	}

	[Fact]
	public void LabelEqualityTests()
	{
		var label = new SmartLabel("foo");
		var label2 = new SmartLabel(label.ToString());
		Assert.Equal(label, label2);

		label = new SmartLabel("foo, bar, buz");
		label2 = new SmartLabel(label.ToString());
		Assert.Equal(label, label2);

		label2 = new SmartLabel("bar, buz, foo");
		Assert.Equal(label, label2);

		var label3 = new SmartLabel("bar, buz");
		Assert.NotEqual(label, label3);

		SmartLabel? label4 = null;
		SmartLabel? label5 = null;
		Assert.Equal(label4, label5);
		Assert.NotEqual(label, label4);
		Assert.False(label.Equals(label4));
		Assert.NotEqual(label, label4);
	}

	[Fact]
	public void SpecialLabelTests()
	{
		var label = new SmartLabel("");
		Assert.Equal(label, SmartLabel.Empty);
		Assert.True(label.IsEmpty);

		label = new SmartLabel("foo, bar, buz");
		var label2 = new SmartLabel("qux");

		label = SmartLabel.Merge(label, label2);
		Assert.Equal("bar, buz, foo, qux", label);

		label2 = new SmartLabel("qux", "bar");
		label = SmartLabel.Merge(label, label2);
		Assert.Equal(4, label.Count);
		Assert.Equal("bar, buz, foo, qux", label);

		label2 = new SmartLabel("Qux", "Bar");
		label = SmartLabel.Merge(label, label2, null!);
		Assert.Equal(4, label.Count);
		Assert.Equal("bar, buz, foo, qux", label);
	}

	[Fact]
	public void CaseSensitivityTests()
	{
		var smartLabel = new SmartLabel("Foo");
		var smartLabelToCheck = new SmartLabel("fOO");
		var stringLabelToCheck = "fOO";
		Assert.False(smartLabel.Equals(smartLabelToCheck));
		Assert.False(smartLabel.Equals(stringLabelToCheck));
		Assert.Equal(0, smartLabel.CompareTo(smartLabelToCheck));
		Assert.Equal(0, smartLabel.CompareTo(stringLabelToCheck));
		Assert.True(smartLabel.Equals(smartLabelToCheck, StringComparer.OrdinalIgnoreCase));
		Assert.False(smartLabel.Equals(smartLabelToCheck, StringComparer.Ordinal));
		Assert.True(smartLabel.Equals(stringLabelToCheck, StringComparison.OrdinalIgnoreCase));
		Assert.False(smartLabel.Equals(stringLabelToCheck, StringComparison.Ordinal));

		smartLabel = new SmartLabel("bAr, FOO, Buz");
		smartLabelToCheck = new SmartLabel("buZ, BaR, fOo");
		stringLabelToCheck = "buZ, BaR, fOo";
		Assert.False(smartLabel.Equals(smartLabelToCheck));
		Assert.False(smartLabel.Equals(stringLabelToCheck));
		Assert.Equal(0, smartLabel.CompareTo(smartLabelToCheck));
		Assert.NotEqual(0, smartLabel.CompareTo(stringLabelToCheck));
		Assert.True(smartLabel.Equals(smartLabelToCheck, StringComparer.OrdinalIgnoreCase));
		Assert.False(smartLabel.Equals(smartLabelToCheck, StringComparer.Ordinal));
		Assert.False(smartLabel.Equals(stringLabelToCheck, StringComparison.OrdinalIgnoreCase)); // stringLabelToCheck is a string, the order of the element is different, this should be False.
		Assert.True(smartLabel.Equals(smartLabelToCheck.ToString(), StringComparison.OrdinalIgnoreCase)); // SmartLabel.cs sorts the elements, this should be True.
		Assert.False(smartLabel.Equals(stringLabelToCheck, StringComparison.Ordinal));
	}
}
