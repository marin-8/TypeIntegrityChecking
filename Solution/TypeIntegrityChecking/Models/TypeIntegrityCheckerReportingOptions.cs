
using System;
using System.Reflection;

namespace TypeIntegrityChecking.Models
{
	public sealed class TypeIntegrityCheckerReportingOptions
	{
		//public bool StopOnFirstError { get; init; }

		public Func<Exception, Assembly, Type, MemberInfo?, bool>? ReportErrorPredicate { get; set; }

		public Func<Exception, Assembly, Type, MemberInfo?, bool>? TreatErrorAsInfoPredicate { get; set; } =
			(exception, assembly, type, member) =>
				exception is ArgumentException
				&& exception.Message == "The given generic instantiation was invalid."
				||
				exception is PlatformNotSupportedException
				&& member != null
				&& (member.Name == "BeginInvoke" || member.Name == "EndInvoke");

		public Func<Exception, Assembly, Type, MemberInfo?, bool>? ReportInfoPredicate { get; set; }
	}
}
