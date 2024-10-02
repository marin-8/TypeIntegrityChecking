
using System;
using System.Reflection;

namespace TypeIntegrityChecking.Models
{
	public sealed class TypeIntegrityCheckerSourceOptions
	{
		public Func<Assembly, bool>? AssemblyPredicate { get; set; }

		public Func<Assembly, Type, bool>? TypePredicate { get; set; }

		public BindingFlags CommonMemberGetterBindingFlags { get; set; } =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
		public Func<Assembly, Type, MemberInfo, bool>? CommonMemberPredicate { get; set; }

		public BindingFlags? ConstructorsGetterBindingFlags { get; set; }
		public Func<Assembly, Type, ConstructorInfo, bool>? ConstructorPredicate { get; set; }

		public BindingFlags? PropertiesGetterBindingFlags { get; set; }
		public Func<Assembly, Type, PropertyInfo, bool>? PropertyPredicate { get; set; }

		public BindingFlags? FieldsGetterBindingFlags { get; set; }
		public Func<Assembly, Type, FieldInfo, bool>? FieldPredicate { get; set; }

		public BindingFlags? EventsGetterBindingFlags { get; set; }
		public Func<Assembly, Type, EventInfo, bool>? EventPredicate { get; set; }

		public BindingFlags? MethodsGetterBindingFlags { get; set; }
		public Func<Assembly, Type, MethodInfo, bool>? MethodPredicate { get; set; }
	}
}
