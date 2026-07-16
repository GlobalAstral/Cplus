using System.Text;
using tokens;

namespace preprocessor;

public record Directive(Func<bool> Wakeup, Func<string?> Factory) { }

public partial class Preprocessor
{
  private readonly List<Directive> directives = [];
  private void Register(Func<bool> Wakeup, Func<string?> Factory) => directives.Add(new(Wakeup, Factory));
  public void RegisterDirectives()
  {
    //! Source

    Register(Wakeup(TokenType.Source), () => Consume()!.GetStr());

    //! Namespace

    Register(Wakeup(TokenType.Namespace), () => {
      Consume();
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      namespaces.Push(name);

      StringBuilder builder = new();
      DoUntil(TokenType.End, () =>
      {
        string? s = ProcessOne();
        if (s != null)
          builder.Append(s);
      });

      namespaces.Pop();
      return builder.ToString();
    });

    //! Mangle

    Register(Wakeup(TokenType.Mangle), () =>
    {
      Consume();
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      return MangleNamespaces(name);
    });

    //! Identifiers

    Register(Wakeup(TokenType.Identifier), () =>
    {
      StringBuilder ident = new();
      ident.Append(Consume()!.GetStr()!);
      while (PeekEqual(TokenType.Colon) && PeekEqual(TokenType.Colon, 1))
      {
        Consume(2);
        string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
        ident.Append($"__{name}");
      }
      return ident.ToString();
    });
  }
}
