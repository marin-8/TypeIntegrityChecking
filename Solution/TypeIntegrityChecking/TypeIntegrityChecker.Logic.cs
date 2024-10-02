
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TypeIntegrityChecking.Models;
using TypeIntegrityChecking.Services;

namespace TypeIntegrityChecking
{
	public static partial class TypeIntegrityChecker // Logic
	{
		private static void CheckAllTypesInRelatedAssembliesInternal<T> (
			Action<string> reportErrorMessagesMethod,
			Action<string>? reportInfoMessagesMethod,
			Action<TypeIntegrityCheckerOptions>? configureOptionsMethod)
			where T : notnull
		{
			var options = new TypeIntegrityCheckerOptions();
			configureOptionsMethod?.Invoke(options);

			var diagnosticReporterService =
				new DiagnosticReporterService(
					reportErrorMessagesMethod,
					reportInfoMessagesMethod,
					options.ReportingOptions);

			var membersByTypeByAssembly =
				GetMembersByTypeByAssembly<T>(
					options.SourceOptions);

			foreach (var assemblyWithTypes in membersByTypeByAssembly)
			{
				var assembly = assemblyWithTypes.Key;

				foreach (var typeWithMembers in assemblyWithTypes.Value)
				{
					var type = typeWithMembers.Key;
					var members = typeWithMembers.Value;

					CheckType(
						assembly,
						type,
						members,
						diagnosticReporterService);
				}
			}

			diagnosticReporterService.ReportResults();
		}

		private static Dictionary<Assembly, Dictionary<Type, Members>> GetMembersByTypeByAssembly<T> (
			TypeIntegrityCheckerSourceOptions sourceOptions)
		{
			var membersByTypeByAssembly =
				new Dictionary<Assembly, Dictionary<Type, Members>>();

			var assemblies = GetAssembliesForType<T>();

			if (sourceOptions.AssemblyPredicate != null)
			{
				assemblies = assemblies.Where(sourceOptions.AssemblyPredicate);
			}

			foreach (var assembly in assemblies)
			{
				var membersByType = new Dictionary<Type, Members>();

				var types = assembly.GetTypes().AsEnumerable();

				if (sourceOptions.TypePredicate != null)
				{
					types = types.Where(type => sourceOptions.TypePredicate(assembly, type));
				}

				foreach (var type in types)
				{
					var constructors =
						GetAndFilter(
							assembly, type,
							type.GetConstructors,
							sourceOptions.CommonMemberGetterBindingFlags,
							sourceOptions.ConstructorsGetterBindingFlags,
							sourceOptions.CommonMemberPredicate,
							sourceOptions.ConstructorPredicate);

					var properties =
						GetAndFilter(
							assembly, type,
							type.GetProperties,
							sourceOptions.CommonMemberGetterBindingFlags,
							sourceOptions.PropertiesGetterBindingFlags,
							sourceOptions.CommonMemberPredicate,
							sourceOptions.PropertyPredicate);

					var fields =
						GetAndFilter(
							assembly, type,
							type.GetFields,
							sourceOptions.CommonMemberGetterBindingFlags,
							sourceOptions.FieldsGetterBindingFlags,
							sourceOptions.CommonMemberPredicate,
							sourceOptions.FieldPredicate);

					var events =
						GetAndFilter(
							assembly, type,
							type.GetEvents,
							sourceOptions.CommonMemberGetterBindingFlags,
							sourceOptions.EventsGetterBindingFlags,
							sourceOptions.CommonMemberPredicate,
							sourceOptions.EventPredicate);

					var methods =
						GetAndFilter(
							assembly, type,
							type.GetMethods,
							sourceOptions.CommonMemberGetterBindingFlags,
							sourceOptions.MethodsGetterBindingFlags,
							sourceOptions.CommonMemberPredicate,
							sourceOptions.MethodPredicate);

					var members =
						new Members(
							constructors,
							properties,
							fields,
							events,
							methods);

					membersByType.Add(type, members);
				}

				membersByTypeByAssembly.Add(assembly, membersByType);
			}

			return membersByTypeByAssembly;
		}

		private static IEnumerable<Assembly> GetAssembliesForType<T> ()
		{
			var mainAssembly = typeof(T).Assembly;
			var assemblies = new HashSet<Assembly>();
			LoadReferencedAssemblies(mainAssembly, assemblies);
			return assemblies.AsEnumerable();
		}

		private static void LoadReferencedAssemblies (
			Assembly assembly, HashSet<Assembly> assemblies)
		{
			if (!assemblies.Add(assembly)) return;

			var referencedAssembliesNames = assembly.GetReferencedAssemblies();

			foreach (var assemblyName in referencedAssembliesNames)
			{
				try
				{
					var loadedAssembly = Assembly.Load(assemblyName);
					LoadReferencedAssemblies(loadedAssembly, assemblies);
				}
				catch { }
			}
		}

		private static TMember[] GetAndFilter<TMember> (
			Assembly assembly,
			Type type,
			Func<BindingFlags, TMember[]> getMethod,
			BindingFlags commonBindingFlags,
			BindingFlags? specificBindingFlags,
			Func<Assembly, Type, MemberInfo, bool>? commonPredicate,
			Func<Assembly, Type, TMember, bool>? specificPredicate)
			where TMember : MemberInfo
		{
			var members = getMethod(specificBindingFlags ?? commonBindingFlags);

			if (commonPredicate is null && specificPredicate is null)
			{
				return members;
			}

			return
				members
				.Where(member =>
					(commonPredicate?.Invoke(assembly, type, member) ?? true)
					&& (specificPredicate?.Invoke(assembly, type, member) ?? true))
				.ToArray();
		}

		private static void CheckType (
			Assembly assembly,
			Type type,
			Members members,
			DiagnosticReporterService diagnosticReporterService)
		{
			TryAndReport(
				assembly, type, null,
				() => RuntimeHelpers.RunClassConstructor(type.TypeHandle),
				diagnosticReporterService);

			foreach (var constructor in members.Constructors)
			{
				if (!constructor.IsAbstract)
				{
					TryAndReport(
						assembly, type, constructor,
						() => JitMethod(constructor),
						diagnosticReporterService);
				}
			}

			foreach (var property in members.Properties)
			{
				var getMethod = property.GetGetMethod();

				if (getMethod != null && !getMethod.IsAbstract)
				{
					TryAndReport(
						assembly, type, getMethod,
						() => JitMethod(getMethod),
						diagnosticReporterService);
				}

				var setMethod = property.GetSetMethod();

				if (setMethod != null && !setMethod.IsAbstract)
				{
					TryAndReport(
						assembly, type, setMethod,
						() => JitMethod(setMethod),
						diagnosticReporterService);
				}
			}

			foreach (var field in members.Fields)
			{
				TryAndReport(
					assembly, type, field,
					() =>
					{
						if (field.FieldType == null)
						{
							throw new MissingFieldException(null, field.Name);
						}
					},
					diagnosticReporterService);
			}

			foreach (var @event in members.Events)
			{
				var addMethod = @event.GetAddMethod();

				if (addMethod != null && !addMethod.IsAbstract)
				{
					TryAndReport(
						assembly, type, addMethod,
						() => JitMethod(addMethod),
						diagnosticReporterService);
				}

				var removeMethod = @event.GetRemoveMethod();

				if (removeMethod != null && !removeMethod.IsAbstract)
				{
					TryAndReport(
						assembly, type, removeMethod,
						() => JitMethod(removeMethod),
						diagnosticReporterService);
				}
			}

			foreach (var method in members.Methods)
			{
				if (!method.IsAbstract)
				{
					TryAndReport(
						assembly, type, method,
						() => JitMethod(method),
						diagnosticReporterService);
				}
			}
		}

		private static void JitMethod (MethodBase method)
		{
			if (method.IsGenericMethodDefinition)
			{
				var classGenericArgs = method.DeclaringType!.GetGenericArguments();
				var methodGenericArgs = method.GetGenericArguments();
				var allGenericArgs = classGenericArgs.Concat(methodGenericArgs);
				var typeHandlers = allGenericArgs.Select(arg => arg.TypeHandle).ToArray();

				RuntimeHelpers.PrepareMethod(method.MethodHandle, typeHandlers);
			}
			else
			{
				RuntimeHelpers.PrepareMethod(method.MethodHandle);
			}
		}

		private static void TryAndReport (
			Assembly assembly,
			Type type,
			MemberInfo? member,
			Action action,
			DiagnosticReporterService diagnosticReporterService)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				diagnosticReporterService.AddError(
					exception, assembly, type, member);
			}
		}
	}
}
