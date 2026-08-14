using preprocessor;
using tokens;

class Program
{
  public static void Main(string[] args)
  {
    // string content = File.ReadAllText("test.cp");

    // Tokenizer tokenizer = new([.. content]);
    // Token[] tokens = tokenizer.Process();

    // Console.WriteLine("TOKENS: ");
    // foreach (var token in tokens)
    //   Console.WriteLine(token);      
    
    // Preprocessor preprocessor = new(tokens);
    // preprocessor.RegisterDirectives();
    // string[] strings = preprocessor.Process();
    // string result = string.Join("", strings);
    
    // Console.WriteLine(result);

    // File.WriteAllText("test.c", result);


    bool debug = args.Contains("--debug");

    foreach (var f in args)
    {
      if (f.StartsWith('-'))
        continue;
      if (!f.EndsWith(".cp") && !f.EndsWith(".hcp"))
        throw new ArgumentException($"Cannot process file {f}. Only .c and .h files are allowed");
      
      if (!File.Exists(f))
        throw new ArgumentException($"File {f} does not exist");

      string content = File.ReadAllText(f);

      Tokenizer tokenizer = new([.. content]);
      Token[] tokens = tokenizer.Process();

      if (debug)
      {
        Console.WriteLine("TOKENS: ");
        foreach (var token in tokens)
          Console.WriteLine(token);
      }
      
      Preprocessor preprocessor = new(tokens);
      preprocessor.RegisterDirectives();
      string[] strings = preprocessor.Process();
      string result = string.Join("", strings);
      
      if (debug)
        Console.WriteLine(result);

      string fname = f.EndsWith(".cp") ? f.Replace(".cp", ".c") : f.Replace(".hcp", ".h");
      File.WriteAllText(fname, result);
    }
  }
}
