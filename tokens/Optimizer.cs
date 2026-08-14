
using System.Text;
using core;

namespace tokens;

class Optimizer(Token[] tokens) : Processor<Token, TokenType, Token>(tokens, (token, type) => token != null && token.Type == type)
{
  protected override Token? ProcessOne()
  {
    if (PeekEqual(TokenType.Source) && PeekEqual(TokenType.Source, 1))
    {
      int line = Peek()!.Line;
      StringBuilder builder = new();
      while (PeekEqual(TokenType.Source))
        builder.Append(Consume()!.GetStr());
      
      return new(TokenType.Source, line, builder.ToString());
    }

    return Consume();
  }
}
