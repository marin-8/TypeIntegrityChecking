
using System.Reflection;

namespace TypeIntegrityChecking.Models
{
	internal sealed class Members
	{
		public ConstructorInfo[] Constructors { get; set; }
		public PropertyInfo[] Properties { get; set; }
		public FieldInfo[] Fields { get; set; }
		public EventInfo[] Events { get; set; }
		public MethodInfo[] Methods { get; set; }

		public Members (
			ConstructorInfo[] constructors,
			PropertyInfo[] properties,
			FieldInfo[] fields,
			EventInfo[] events,
			MethodInfo[] methods)
		{
			Constructors = constructors;
			Properties = properties;
			Fields = fields;
			Events = events;
			Methods = methods;
		}
	}
}
