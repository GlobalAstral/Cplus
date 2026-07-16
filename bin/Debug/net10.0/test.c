#include <stdio.h>


  int Math__add(int a, int b) {
    return a + b;
  }

  int Math__multiply(int a, int b) {
    return a * b;
  }


  
  void Text__hello() {
    printf("Hello from Text namespace!\n");
  }


int main() {
  int result = Math__add(5, 3);

  printf("Result: %d\n", result);

  Math__multiply(4, 2);

  Text__hello();

  return 0;
}
