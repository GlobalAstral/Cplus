typedef struct Point {
  int x;
  int y;
} *Point;
  Point ctor__Point(int x, int y) {
    Point self = malloc(sizeof(*self));
    self->x = x;
    self->y = y;
    return self;
  }
  void dtor__Point(Point self) {
    free(self);
  }
  void Point__move(Point self, int x, int y) {
    self->x = x;
    self->y = y;
  }


int main() {

  Point p = ctor__Point(10, 10);
  Point__move(p, 10, 10);
  dtor__Point(p);

  return 0;
}
