using System.Text;
using core;
using tokens;

namespace preprocessor;

public partial class Preprocessor(Token[] tokens) : Processor<Token, TokenType, string>(tokens, (t, type) => t != null && t.Type == type)
{
  private readonly Stack<string> namespaces = [];
  private string? currentClass = null;
  private readonly List<Generic> Generics = [];
  private readonly List<string> GeneratedGenerics = [];
  private int? GenericsGenPoint = null;
  private readonly Dictionary<string, string> GenericContext = [];
  private string? GenericContextName = null;


  private void RemoveSource()
  {
    while (TryConsume(TokenType.Source));
  }

  private void GenerateGeneric(Generic generic, string[] resolvedTypes, string id)
  {
    if (GenericsGenPoint == null)
      throw new Exception("No Generics generation point is set");
    
    for (int i = 0; i < generic.Generics.Length; i++)
    {
      string k = generic.Generics[i];
      string v = resolvedTypes[i];
      GenericContext[k] = v;
    }
    GenericContextName = id;
    StringBuilder builder = new();
    Switch(generic.Tokens, () =>
    {
      while (HasPeek())
      {
        string? s = ProcessOne();
        if (s != null)
          builder.Append(s);
      }
    });

    string result = builder.ToString();
    int prev = output.Count;
    output.Insert((int)GenericsGenPoint, result);
    int delta = output.Count - prev;
    GenericsGenPoint += delta;

    GenericContext.Clear();
    GenericContextName = null;

    GeneratedGenerics.Add(id);
  }

  private string ParseIdentifier(bool generics = true)
  {
    StringBuilder ident = new();
    ident.Append(TryConsumeErr(TokenType.Identifier)!.GetStr()!);
    if (GenericContext.ContainsKey(ident.ToString()))
      return GenericContext[ident.ToString()];
    while (PeekEqual(TokenType.Colon) && PeekEqual(TokenType.Colon, 1))
    {
      Consume(2);
      string name = TryConsumeErr(TokenType.Identifier)!.GetStr()!;
      ident.Append($"__{name}");
    }
    Generic? generic = Generics.Find((g) => g.Name == ident.ToString());
    if (!generics || generic == null)
      return ident.ToString();
    
    List<string> resolvedTypes = [];
    TryConsumeErr(TokenType.LAngle);
    RemoveSource();

    while (HasPeek())
    {
      StringBuilder builder = new();
      TokenType needed = LookAhead(TokenType.RAngle) ? TokenType.RAngle : TokenType.Comma;
      bool last = needed == TokenType.RAngle;
      
      string? temp = ProcessOne();
      if (temp != null)
        builder.Append(temp);
      
      TryConsumeErr(needed);
      
      resolvedTypes.Add(builder.ToString());
      builder.Clear();
      if (last)
        break;
    }

    if (resolvedTypes.Count != generic.Generics.Length)
      throw new Exception($"Generic expects {generic.Generics.Length} types but {resolvedTypes.Count} were given");
    
    foreach (string type in resolvedTypes)
      ident.Append($"__{type}");

    string id = ident.ToString();
    id = id.Replace("*", "_ptr").Replace("(", "_").Replace(")", "_");
    if (!GeneratedGenerics.Contains(id))
      GenerateGeneric(generic, [ .. resolvedTypes ], id);
    return id;
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
