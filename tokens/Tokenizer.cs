using System.Text;
using core;

namespace tokens;

class Tokenizer(char[] content) : Processor<char, char, Token>(content, (a, b) => a == b)
{
  private int line = 1;

  protected override Token? ProcessOne()
  {
    if (TryConsume('\n'))
    {
      line++;
      return new(TokenType.Source, line-1, "\n");
    }

    if (TryConsume('"'))
    {
      StringBuilder builder = new();
      builder.Append('"');
      DoUntil('"', () => builder.Append(Consume()));
      builder.Append('"');
      return new(TokenType.Source, line, builder.ToString());
    }

    if (TryConsume('\''))
    {
      StringBuilder builder = new();
      builder.Append('\'');
      DoUntil('\'', () => builder.Append(Consume()));
      builder.Append('\'');
      return new(TokenType.Source, line, builder.ToString());
    }

    if (TryConsume('<'))
      return new(TokenType.LAngle, line, "<");

    if (TryConsume('>'))
      return new(TokenType.RAngle, line, ">");

    if (TryConsume(':'))
      return new(TokenType.Colon, line, ":");
    
    if (TryConsume(','))
      return new(TokenType.Comma, line, ",");
    
    if (char.IsAsciiLetter(Peek()) || PeekEqual('_'))
    {
      StringBuilder builder = new();
      while (char.IsAsciiLetterOrDigit(Peek()) || PeekEqual('_'))
        builder.Append(Consume());
      
      string buffer = builder.ToString();

      if (Token.Reserved.Contains(buffer))
        return new(TokenType.Keyword, line, buffer);

      if (buffer == "namespace")
        return new(TokenType.Namespace, line);

      if (buffer == "mangle")
        return new(TokenType.Mangle, line);

      if (buffer == "endnamespace")
        return new(TokenType.EndNamespace, line);
      
      if (buffer == "endclass")
        return new(TokenType.EndClass, line);
      
      if (buffer == "endgeneric")
        return new(TokenType.EndGeneric, line);

      if (buffer == "class")
        return new(TokenType.Class, line);
      
      if (buffer == "impl")
        return new(TokenType.Impl, line);
      
      if (buffer == "method")
        return new(TokenType.Method, line);
      
      if (buffer == "Self")
        return new(TokenType.Self, line);
      
      if (buffer == "constructor")
        return new(TokenType.Constructor, line);
      
      if (buffer == "destructor")
        return new(TokenType.Destructor, line);

      if (buffer == "new")
        return new(TokenType.New, line);
      
      if (buffer == "delete")
        return new(TokenType.Delete, line);
      
      if (buffer == "SelfAlloc")
        return new(TokenType.SelfAlloc, line);
      
      if (buffer == "generic")
        return new(TokenType.Generic, line);

      if (buffer == "genericAlias")
        return new(TokenType.GenericAlias, line);
      
      return new(TokenType.Identifier, line, buffer);
    }

    return new(TokenType.Source, line, Consume().ToString());
  }
} 
