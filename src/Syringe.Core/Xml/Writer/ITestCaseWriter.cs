using Syringe.Core.TestCases;

namespace Syringe.Core.Xml.Writer
{
	public interface ITestCaseWriter
	{
		string Write(CaseCollection caseCollection);
	}
}