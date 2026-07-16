using System.Text;
using core;
using tokens;

namespace preprocessor;

public partial class Preprocessor(Token[] tokens) : Processor<Token, TokenType, string>(tokens, (t, type) => t != null && t.Type == type)
{
  private readonly Stack<string> namespaces = [];

  private void RemoveSource()
  {
    while (TryConsume(TokenType.Source));
  }

  private string MangleNamespaces(string add)
  {
    StringBuilder builder = new();
    foreach (string namesp in namespaces.Reverse())
      builder.Append($"{namesp}__");
    builder.Append(add);
    return builder.ToString();
  }

  private Func<bool> Wakeup(TokenType token) => () => PeekEqual(token);
  protected override string? ProcessOne()
  {
    foreach (Directive directive in directives)
    {
      if (directive.Wakeup())
        return directive.Factory();
    }
    return Consume()!.GetStr();
  }
}
