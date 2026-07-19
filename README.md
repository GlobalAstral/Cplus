
# C+

>## A tiny preprocessor for C which gives it the few features it lacks.

## Features

- ### Namespaces

- ### Classes

- ### Generics

## Namespace

### Syntax

``` CPP
namespace Foo
  // bar will be mangled because it is marked
  // with the "mangle" keyword.
  // baz will NOT be mangled because it is not marked
  // with the "mangle" keyword.
  int mangle bar = 0;
  int baz = 0;
endnamespace
// Namespaces may be nested and to access identifiers, double
// colons are used.
Foo::bar
```

Becomes

``` CPP
int Foo__bar = 0;
Foo__bar
```

## Class

### Syntax

``` CPP
class Point // Name is mangled automatically if in namespace.
  int x;
  int y;
impl // May be "endclass" if there are no methods.

  // Will be prefixed with "Point__" because of "method" keyword
  void method move(Self, int x, int y) {
    self->x = x;
    self->y = y;
  }
  // Called with new keyword
  constructor(int x, int y) {
    SelfAlloc;
    self->x = x;
    self->y = y;
    return self;
  }
  // Called with delete keyword
  destructor(Self) {
    free(self);
  }
endclass

Point p = new Point(10, 10);
Point::move(p, 20, 20);
delete Point(p);
```

Becomes

``` CPP

typedef struct Point {
  int x;
  int y;
} *Point; 

// Classes are pointer types by definition. 
// If stack allocation is needed, use a struct.

void Point__move(Point self, int x, int y) {
  self->x = x;
  self->y = y;
}

Point Point__ctor(int x, int y) {
  Point self = (Point)malloc(sizeof(*self));
  self->x = x;
  self->y = y;
  return self;
}

void Point__dtor(Point self) {
  free(self);
}

Point p = Point__ctor(10, 10);
Point__move(p, 20, 20);
Point__dtor(p);
```

## Generics

### Syntax

``` CPP
// First Identifier is the name of the generic code.
// It may be a function or a class. Every other identifier before
// the colon is a generic typename.
// In emission, the first generic marks a spot for generic code generation called
// GenericGenPoint. The declaration of generic code generates
// nothing on the spot.
generic add T:
// genericAlias is just a syntactic sugar for the name of the generic code.
T genericAlias (T a, T b) {
  return a + b;
}
endgeneric

// Upon calling the function with a type, a function for that 
// specified type will be created once at GenericGenPoint downwards
// Types inside the < ... > may be any processable input, even nested
// generic code. * symbol will be converted to "__ptr" and parenthesis
// symbols will be converted to a single underscore.
add<int>(10, 10);
add<float>(3.1, 1.2);
```

Becomes

``` CPP
int add__int(int a, int b) {
  return a + b;
}

float add__float(float a, float b) {
  return a + b;
}

add__int(10, 10);
add__float(3.1, 1.2);
```

## Notes
---
>#### **C+ is not designed as a beginner-friendly replacement for C. It is a tool for experienced C programmers who want to reduce repetitive boilerplate while keeping the control and transparency of C.**
>#### **C+ assumes that the programmer understands the concepts behind the abstractions being introduced. It does not attempt to replace knowledge with compiler enforcement; instead, it provides a more expressive syntax for common patterns while preserving the underlying C model.**
>#### **The goal of C+ is not to become the new C, but to serve as a productivity tool for C veterans: reducing tedious code without hiding what the program actually does.**
>#### **If you decide to experiment with C+, please report any bugs you encounter. Since C+ is built around transforming code into C, even small issues in translation can be valuable to discover and fix.**
---
