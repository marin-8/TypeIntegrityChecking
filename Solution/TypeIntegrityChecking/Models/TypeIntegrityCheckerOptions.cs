
namespace TypeIntegrityChecking.Models
{
	public sealed class TypeIntegrityCheckerOptions
	{
		public TypeIntegrityCheckerSourceOptions SourceOptions { get; } =
			new TypeIntegrityCheckerSourceOptions();

		public TypeIntegrityCheckerReportingOptions ReportingOptions { get; } =
			new TypeIntegrityCheckerReportingOptions();
	}
}
