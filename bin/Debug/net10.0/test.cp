#include <stdio.h>

namespace Test
  generic Point T : 
    class genericAlias
      T x;
      T y;
    impl
      void move(Self, T x, T y) {
        self->x = x;
        self->y = y;
      }
      constructor(T x, T y) {
        SelfAlloc;
        self->x = x;
        self->y = y;
        return self;
      }
      destructor(Self) {
        free(self);
      }
    endclass
  endgeneric
endnamespace

int main() {
  Test::Point<Test::Point<int>> p = new Test::Point<Test::Point<int>>(10, 10);
  delete Test::Point<Test::Point<int>>(p);
  return 0;
}
