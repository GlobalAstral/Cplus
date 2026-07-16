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
    {
      StringBuilder builder = new();
      builder.Append('<');
      DoUntil('>', () => builder.Append(Consume()));
      builder.Append('>');
      return new(TokenType.Source, line, builder.ToString());
    }

    if (TryConsume(':'))
      return new(TokenType.Colon, line);
    
    if (char.IsAsciiLetter(Peek()) || PeekEqual('_'))
    {
      StringBuilder builder = new();
      while (char.IsAsciiLetterOrDigit(Peek()) || PeekEqual('_'))
        builder.Append(Consume());
      
      string buffer = builder.ToString();

      if (Token.Reserved.Contains(buffer))
        return new(TokenType.Source, line, buffer);

      if (buffer == "namespace")
        return new(TokenType.Namespace, line);

      if (buffer == "mangle")
        return new(TokenType.Mangle, line);

      if (buffer == "end")
        return new(TokenType.End, line);
      
      return new(TokenType.Identifier, line, buffer);
    }

    return new(TokenType.Source, line, Consume().ToString());
  }
} 
