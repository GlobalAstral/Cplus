
class Point
  int x;
  int y;
impl
  void method move(Self, int x, int y) {
    self->x = x;
    self->y = y;
  }
end

int main() {

  Point p;
  Point::move(p, 10, 10);

  return 0;
}
