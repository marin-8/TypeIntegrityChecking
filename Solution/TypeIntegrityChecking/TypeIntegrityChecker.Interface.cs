
using System;
using TypeIntegrityChecking.Models;

namespace TypeIntegrityChecking
{
	public static partial class TypeIntegrityChecker // Interface
	{
		public static void CheckAllTypesInRelatedAssemblies<T> (
			Action<string> reportErrorMessagesMethod)
			where T	: notnull
		{
			CheckAllTypesInRelatedAssembliesInternal<T>(
				reportErrorMessagesMethod,
				null,
				null);
		}

		public static void CheckAllTypesInRelatedAssemblies<T> (
			Action<string> reportErrorMessagesMethod,
			Action<TypeIntegrityCheckerOptions> configureOptionsMethod)
			where T : notnull
		{
			CheckAllTypesInRelatedAssembliesInternal<T>(
				reportErrorMessagesMethod,
				null,
				configureOptionsMethod);
		}

		public static void CheckAllTypesInRelatedAssemblies<T> (
			Action<string> reportErrorMessagesMethod,
			Action<string> reportInfoMessagesMethod)
			where T : notnull
		{
			CheckAllTypesInRelatedAssembliesInternal<T>(
				reportErrorMessagesMethod,
				reportInfoMessagesMethod,
				null);
		}

		public static void CheckAllTypesInRelatedAssemblies<T> (
			Action<string> reportErrorMessagesMethod,
			Action<string> reportInfoMessagesMethod,
			Action<TypeIntegrityCheckerOptions> configureOptionsMethod)
			where T : notnull
		{
			CheckAllTypesInRelatedAssembliesInternal<T>(
				reportErrorMessagesMethod,
				reportInfoMessagesMethod,
				configureOptionsMethod);
		}
	}
}
