using System.Text;
using System.Text.RegularExpressions;

namespace tokens;

public enum TokenType
{
  Namespace,
  Mangle,
  Identifier,
  Colon,
  Source,
  End,
  Class,
  Impl,
  Method,
  Self,
  Constructor,
  Destructor,
  New,
  Delete,
  SelfAlloc,
}

public record Token(TokenType Type, int Line, object? Value)
{
  public Token(TokenType Type, int Line) : this(Type, Line, null) { }
  public string? GetStr() => (string?) Value;
  public Token[]? GetTokens() => (Token[]?) Value;
  public override string ToString()
  {
    if (Value == null)
      return $"{Type} at line {Line}";
    if (Value is string v)
      return $"{Type}({Regex.Escape(v)}) at {Line}";
    if (Value is Token[] t)
    {
      StringBuilder builder = new($"{Type} at line ${Line}\n");
      foreach (Token tk in t)
        builder.Append($"\t{tk.ToString()}\n");
      return builder.ToString();
    }
    return $"{Type} at line {Line}";
  }

  public static readonly HashSet<string> Reserved =
  [
    "auto",
    "break",
    "case",
    "char",
    "const",
    "continue",
    "default",
    "do",
    "double",
    "else",
    "enum",
    "extern",
    "float",
    "for",
    "goto",
    "if",
    "int",
    "long",
    "register",
    "return",
    "short",
    "signed",
    "sizeof",
    "static",
    "struct",
    "switch",
    "typedef",
    "union",
    "unsigned",
    "void",
    "volatile",
    "while",

    // C99
    "inline",
    "restrict",
    "_Bool",
    "_Complex",
    "_Imaginary",

    // C11
    "_Alignas",
    "_Alignof",
    "_Atomic",
    "_Generic",
    "_Noreturn",
    "_Static_assert",
    "_Thread_local",

    // C23
    "alignas",
    "alignof",
    "bool",
    "constexpr",
    "false",
    "nullptr",
    "static_assert",
    "thread_local",
    "true",
    "typeof",
    "typeof_unqual",
    "_BitInt",

    // Common predefined identifiers/macros that you should not mangle
    "__func__",
    "__VA_ARGS__",

    // Preprocessor
    "define",
    "undef",

    "include",

    "if",
    "ifdef",
    "ifndef",
    "elif",
    "else",
    "endif",

    "line",
    "error",
    "pragma",

    // C23
    "embed"
  ];
}
