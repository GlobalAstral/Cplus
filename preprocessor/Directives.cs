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

    Register(WakeupC(TokenType.Namespace), () => {
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

    Register(WakeupC(TokenType.Mangle), () =>
    {
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      return MangleNamespaces(name);
    });

    //! Identifiers

    Register(Wakeup(TokenType.Identifier), () => ParseIdentifier());
    
    //! Classes

    Register(WakeupC(TokenType.Class), () =>
    {
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      currentClass = MangleNamespaces(name);

      bool hasimpl = LookAhead(TokenType.Impl);

      StringBuilder builder = new();

      builder.Append($"typedef struct {currentClass} {{");

      void temp()
      {
        string? s = ProcessOne();
        if (s != null)
          builder.Append(s);
      }

      if (hasimpl)
        DoUntil(TokenType.Impl, temp);
      else
        DoUntil(TokenType.End, temp);

      builder.Append($"}} *{currentClass};");

      if (hasimpl)
      {
        DoUntil(TokenType.End, () =>
        {
          string? s = ProcessOne();
          if (s != null)
            builder.Append(s);
        });
      }

      currentClass = null;
      return builder.ToString();
    });
    
    //! Method

    Register(WakeupC(TokenType.Method), () =>
    {
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      if (currentClass == null)
        throw new Exception("Cannot use method outside of class");
      return $"{currentClass}__{name}";
    });

    //! Self
    //TODO CHECK IF NOT IN FIRST ARGUMENT ERROR
    Register(WakeupC(TokenType.Self), () =>
    {
      if (currentClass == null)
        throw new Exception("Cannot use Self outside of class");
      return $"{currentClass} self";
    });

    //! Constructor

    Register(WakeupC(TokenType.Constructor), () =>
    {
      if (currentClass == null)
        throw new Exception("Cannot use constructor outside of class");
      
      return $"{currentClass} ctor__{currentClass}";
    });

    //! Destructor

    Register(WakeupC(TokenType.Destructor), () =>
    {
      if (currentClass == null)
        throw new Exception("Cannot use destructor outside of class");
      
      return $"void dtor__{currentClass}";
    });

    //! New

    Register(WakeupC(TokenType.New), () =>
    {
      RemoveSource();
      Token? t = Peek();
      string name = ParseIdentifier();
      return $"ctor__{name}";
    });

    //! Delete

    Register(WakeupC(TokenType.Delete), () =>
    {
      RemoveSource();
      string name = ParseIdentifier();
      return $"dtor__{name}";
    });

    //! SelfAlloc

    Register(WakeupC(TokenType.SelfAlloc), () =>
    {
      if (currentClass == null)
        throw new Exception("Cannot use SelfAlloc outside of class");
      
      return $"{currentClass} self = malloc(sizeof(*self))";
    });

    //! Generic


  }
}
