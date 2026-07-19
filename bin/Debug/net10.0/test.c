#include <stdio.h>

 
    typedef struct Test__Point__int {
      int x;
      int y;
    } *Test__Point__int;
      void move(Test__Point__int self, int x, int y) {
        self->x = x;
        self->y = y;
      }
      Test__Point__int ctor__Test__Point__int(int x, int y) {
        Test__Point__int self = malloc(sizeof(*self));
        self->x = x;
        self->y = y;
        return self;
      }
      void dtor__Test__Point__int(Test__Point__int self) {
        free(self);
      }
    
   
    typedef struct Test__Point__Test__Point__int {
      Test__Point__int x;
      Test__Point__int y;
    } *Test__Point__Test__Point__int;
      void move(Test__Point__Test__Point__int self, Test__Point__int x, Test__Point__int y) {
        self->x = x;
        self->y = y;
      }
      Test__Point__Test__Point__int ctor__Test__Point__Test__Point__int(Test__Point__int x, Test__Point__int y) {
        Test__Point__Test__Point__int self = malloc(sizeof(*self));
        self->x = x;
        self->y = y;
        return self;
      }
      void dtor__Test__Point__Test__Point__int(Test__Point__Test__Point__int self) {
        free(self);
      }
    
  
  


int main() {
  Test__Point__Test__Point__int p = ctor__Test__Point__Test__Point__int(10, 10);
  dtor__Test__Point__Test__Point__int(p);
  return 0;
}
