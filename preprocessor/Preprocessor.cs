using System.Text;
using core;
using tokens;

namespace preprocessor;

public partial class Preprocessor(Token[] tokens) : Processor<Token, TokenType, string>(tokens, (t, type) => t != null && t.Type == type)
{
  private readonly Stack<string> namespaces = [];
  private string? currentClass = null;

  private void RemoveSource()
  {
    while (TryConsume(TokenType.Source));
  }

  private string ParseIdentifier()
  {
    StringBuilder ident = new();
    ident.Append(TryConsumeErr(TokenType.Identifier)!.GetStr()!);
    while (PeekEqual(TokenType.Colon) && PeekEqual(TokenType.Colon, 1))
    {
      Consume(2);
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      ident.Append($"__{name}");
    }
    return ident.ToString();
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
  private Func<bool> WakeupC(TokenType token) => () => TryConsume(token);
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
