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
    Register(Wakeup(TokenType.Keyword), () => Consume()!.GetStr());
    Register(Wakeup(TokenType.Source), () => Consume()!.GetStr());

    //! Namespace

    Register(WakeupC(TokenType.Namespace), () => {
      RemoveSource();
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      namespaces.Push(name);

      StringBuilder builder = new();
      DoUntil(TokenType.EndNamespace, () =>
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
      string name;
      if (TryConsume(TokenType.GenericAlias))
      {
        if (GenericContext.Count == 0)
          throw new Exception("Cannot use genericAlias outside of GenericContext");
        name = GenericContextName!;
      } else
        name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
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
        DoUntil(TokenType.EndClass, temp);

      builder.Append($"}} *{currentClass};");

      if (hasimpl)
      {
        DoUntil(TokenType.EndClass, () =>
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
      
      return $"{currentClass} self = ({currentClass})malloc(sizeof(*self))";
    });

    //! Generic

    Register(WakeupC(TokenType.Generic), () =>
    {
      RemoveSource();
      string name = MangleNamespaces(TryConsumeErr(TokenType.Identifier).GetStr()!);
      List<string> generics = [];
      RemoveSource();
      
      DoUntil(TokenType.Colon, () =>
      {
        generics.Add(TryConsumeErr(TokenType.Identifier).GetStr()!);
        RemoveSource();
      });
      
      List<Token> toks = [];
      DoUntil(TokenType.EndGeneric, () => toks.Add(Consume()!));
      Generics.Add(new Generic(name, [ .. generics ], [ .. toks ]));
      GenericsGenPoint ??= output.Count;
      return null;
    });

    //! GenericAlias

    Register(WakeupC(TokenType.GenericAlias), () =>
    {
      RemoveSource();
      if (GenericContext.Count == 0)
        throw new Exception("Cannot use genericAlias outside of GenericContext");
      return GenericContextName;
    });
  }
}
