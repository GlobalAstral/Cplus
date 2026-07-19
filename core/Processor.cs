
using System.Diagnostics;

namespace core;

public abstract class Processor<I, C, O>(I[] Input, Func<I?, C, bool> Equ)
{
  protected int peek = 0;
  protected List<O> output = [];

  protected bool HasPeek(int offset = 0) => peek + offset >= 0 && peek + offset < Input.Length;
  protected I? Peek(int offset = 0) => HasPeek(offset) ? Input[peek + offset] : default;
  protected bool PeekEqual(C value, int offset = 0) => Equ(Peek(offset), value);
  protected I? Consume() => HasPeek() ? Input[peek++] : default;
  protected bool TryConsume(C value)
  {
    if (PeekEqual(value))
    {
      Consume();
      return true;
    }
    return false;
  }
  protected I TryConsumeErr(C value)
  {
    if (PeekEqual(value))
      return Consume()!;
    throw new Exception($"Expected {value}");
  }
  protected abstract O? ProcessOne();

  public O[] Process()
  {
    while (HasPeek())
    {
      O? o = ProcessOne();
      if (o != null)
        output.Add(o);
    }
    return [.. output];
  }

  protected void DoUntil(C find, Action action)
  {
    bool found = false;
    while (HasPeek())
    {
      if (TryConsume(find))
      {
        found = true;
        break;
      }
      action();
    }
    if (!found)
      throw new Exception($"Expected {find}");
  }

  protected void DoUntilP(C prefix, C find, Action action)
  {
    bool found = false;
    while (HasPeek())
    {
      if (PeekEqual(prefix) && PeekEqual(find, 1))
      {
        Consume(2);
        found = true;
        break;
      }
      action();
    }
    if (!found)
      throw new Exception($"Expected {prefix} {find}");
  }

  protected void Switch(I[] content, Action action)
  {
    I[] prev = Input;
    int prev_peek = peek;

    Input = content;
    peek = 0;

    action();

    Input = prev;
    peek = prev_peek;
  }

  protected void Consume(int amount = 1)
  {
    for (int i = 0; i < amount; i++)
    {
      I? _ = Consume();
    }
  }

  protected bool LookAhead(C value)
  {
    int prev_peek = peek;
    while (HasPeek())
    {
      if (PeekEqual(value))
      {
        peek = prev_peek;
        return true;
      }
      Consume();
    }
    peek = prev_peek;
    return false;
  }
}