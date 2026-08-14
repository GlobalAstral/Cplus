using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

      RemoveSource();
      Token[] content = TryConsumeErr(TokenType.CurlyBlock).GetTokens()!;
      StringBuilder builder = new();

      Switch(content, () =>
      {
        while (HasPeek())
        {
          string? s = ProcessOne();
          if (s != null)
            builder.Append(s);
        }
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

      StringBuilder builder = new();

      builder.Append($"typedef struct {currentClass} {{");

      RemoveSource();
      Token[] structContent = TryConsumeErr(TokenType.CurlyBlock).GetTokens()!;

      Switch(structContent, () =>
      {
        while (HasPeek())
        {
          string? s = ProcessOne();
          if (s != null)
            builder.Append(s);
        }
      });

      builder.Append($"}} *{currentClass};");

      if (LookAhead(TokenType.Impl))
      {
        RemoveSource();
        TryConsumeErr(TokenType.Impl);
        RemoveSource();
        Token[] implContent = TryConsumeErr(TokenType.CurlyBlock).GetTokens()!;

        Switch(implContent, () =>
        {
          while (HasPeek())
          {
            string? s = ProcessOne();
            if (s != null)
              builder.Append(s);
          }
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
      
      DoUntilP(TokenType.CurlyBlock, () =>
      {
        generics.Add(TryConsumeErr(TokenType.Identifier).GetStr()!);
        RemoveSource();
      });
      
      List<Token> toks = [];

      Token[] content = TryConsumeErr(TokenType.CurlyBlock).GetTokens()!;
      Switch(content, () =>
      {
        while (HasPeek())
          toks.Add(Consume()!);
      });

      Generics.Add(new Generic(name, [ .. generics ], [ .. toks ]));
      GenericsGenPoint ??= output.Count;

      return null;
    });

    //! GenericAlias

    Register(WakeupC(TokenType.GenericAlias), () =>
    {
      if (GenericContext.Count == 0)
        throw new Exception("Cannot use genericAlias outside of GenericContext");
      return GenericContextName;
    });

    //! CurlyBlock

    Register(Wakeup(TokenType.CurlyBlock), () =>
    {
      Token[] content = Consume()!.GetTokens()!;

      StringBuilder builder = new();
      builder.Append('{');

      Switch(content, () =>
      {
        while (HasPeek())
        {
          string? s = ProcessOne();
          if (s != null)
            builder.Append(s);
        }
      });

      builder.Append('}');

      return builder.ToString();
    });

    //! Struct Shortcut

    Register(WakeupC(TokenType.Dollar), () => "struct ");

    //! String Literal

    Register(Wakeup(TokenType.StringLiteral), () => $"\"{Consume()!.GetStr()}\"");

    //! Import

    Register(WakeupC(TokenType.Import), () =>
    {
      RemoveSource();
      string path = TryConsumeErr(TokenType.StringLiteral).GetStr()!;

      if (path.Split('.').Last() != "hcp")
        throw new Exception($"Cannot import {path} file is not a C+ header");

      if (!File.Exists(path))
        throw new Exception($"Cannot import {path} file does not exist");

      string content = File.ReadAllText(path);

      Tokenizer tokenizer = new([.. content]);
      Token[] tokens = tokenizer.Process();
      InsertInput(peek, tokens);

      return null;
    });

    //! Lambda

    Register(WakeupC(TokenType.Lambda), () =>
    {
      RemoveSource();
      TryConsumeErr(TokenType.LAngle);
      List<Token> retType = [];
      DoUntil(TokenType.RAngle, () => retType.Add(Consume()!));

      List<Token> content = [];

      DoUntilP(TokenType.CurlyBlock, () => content.Add(Consume()!));
      content.Add(Consume()!);

      string lambdaName = GenerateLambdaName();

      GenerateLambda(lambdaName, [.. retType], [.. content]);

      return lambdaName;
    });

    //! LambdaMarker

    Register(WakeupC(TokenType.LambdaMarker), () =>
    {
      LambdaGenPoint ??= output.Count;
      return null;
    });
  }
}
