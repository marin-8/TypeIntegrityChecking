
using System.Text;

namespace TypeIntegrityChecking.Services
{
	internal sealed class MessagesBuilderService
	{
		private readonly StringBuilder _MessagesStringBuilder =
			new StringBuilder();

		public void AddMessage (string message)
		{
			_MessagesStringBuilder.AppendLine();
			_MessagesStringBuilder.AppendLine(message);
		}

		public bool Any () => _MessagesStringBuilder.Length > 0;

		public override string ToString () => _MessagesStringBuilder.ToString();
	}
}
