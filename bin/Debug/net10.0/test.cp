class Point
  int x;
  int y;
impl
  constructor(int x, int y) {
    SelfAlloc;
    self->x = x;
    self->y = y;
    return self;
  }
  destructor(Self) {
    free(self);
  }
  void method move(Self, int x, int y) {
    self->x = x;
    self->y = y;
  }
end

int main() {
  Point p = new Point(10, 10);
  Point::move(p, 10, 10);
  delete Point(p);

  return 0;
}
