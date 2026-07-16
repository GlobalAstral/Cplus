#include <stdio.h>

namespace Math
  int mangle add(int a, int b) {
    return a + b;
  }

  int mangle multiply(int a, int b) {
    return a * b;
  }
end

namespace Text  
  void mangle hello() {
    printf("Hello from Text namespace!\n");
  }
end

int main() {
  int result = Math::add(5, 3);

  printf("Result: %d\n", result);

  Math::multiply(4, 2);

  Text::hello();

  return 0;
}
