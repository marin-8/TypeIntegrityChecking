
using System;
using System.Reflection;
using System.Text;
using TypeIntegrityChecking.Models;

namespace TypeIntegrityChecking.Services
{
	internal sealed class DiagnosticReporterService
	{
		private readonly Action<string> _ReportErrorMessagesMethod;
		private readonly Action<string>? _ReportInfoMessagesMethod;

		private readonly MessagesBuilderService _ErrorMessagesBuilder;
		private readonly MessagesBuilderService? _InfoMessagesBuilder;

		private readonly TypeIntegrityCheckerReportingOptions _ReportingOptions;

		public DiagnosticReporterService (
			Action<string> reportErrorMessagesMethod,
			Action<string>? reportInfoMessagesMethod,
			TypeIntegrityCheckerReportingOptions reportingOptions)
		{
			_ReportErrorMessagesMethod = reportErrorMessagesMethod;
			_ReportInfoMessagesMethod = reportInfoMessagesMethod;

			_ErrorMessagesBuilder = new MessagesBuilderService();

			_InfoMessagesBuilder =
				reportInfoMessagesMethod != null
					? new MessagesBuilderService()
					: null;

			_ReportingOptions = reportingOptions;
		}

		public void AddError (
			Exception exception,
			Assembly assembly,
			Type type,
			MemberInfo? member)
		{
			var treatErrorAsInfo =
				_ReportingOptions.TreatErrorAsInfoPredicate != null
				&& _ReportingOptions.TreatErrorAsInfoPredicate(
					exception, assembly, type, member);

			if (!treatErrorAsInfo)
			{
				if (_ReportingOptions.ReportErrorPredicate is null
					|| _ReportingOptions.ReportErrorPredicate(
						exception, assembly, type, member))
				{
					var formattedErrorMessage =
						FormatReportMessage(exception, assembly, type, member);

					_ErrorMessagesBuilder.AddMessage(formattedErrorMessage);
				}
			}
			else
			{
				if (_InfoMessagesBuilder != null)
					AddInfo(exception, assembly, type, member);
			}
		}

		public void ReportResults ()
		{
			if (_ReportInfoMessagesMethod != null
				&& _InfoMessagesBuilder != null
				&& _InfoMessagesBuilder.Any())
			{
				var combinedInfoMessages = _InfoMessagesBuilder.ToString();
				_ReportInfoMessagesMethod(combinedInfoMessages);
			}

			if (_ErrorMessagesBuilder.Any())
			{
				var combinedErrorMessages = _ErrorMessagesBuilder.ToString();
				_ReportErrorMessagesMethod(combinedErrorMessages);
			}
		}

		private void AddInfo (
			Exception exception,
			Assembly assembly,
			Type type,
			MemberInfo? member)
		{
			if (_ReportingOptions.ReportInfoPredicate is null
				|| _ReportingOptions.ReportInfoPredicate(
					exception, assembly, type, member))
			{
				var formattedInfoMessage =
					FormatReportMessage(exception, assembly, type, member);

				_InfoMessagesBuilder!.AddMessage(formattedInfoMessage);
			}
		}

		private static string FormatReportMessage (
			Exception exception,
			Assembly assembly,
			Type type,
			MemberInfo? member)
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"EXCEPTION: {exception.GetType().FullName}");
			stringBuilder.AppendLine($"  MESSAGE: {exception.Message}");
			if (member != null) stringBuilder.AppendLine($"   MEMBER: {member.Name}");
			stringBuilder.AppendLine($"     TYPE: {type.FullName}");
			stringBuilder.Append($" ASSEMBLY: {assembly.FullName}");

			return stringBuilder.ToString();
		}
	}
}
