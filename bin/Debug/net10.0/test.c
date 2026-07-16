
typedef struct Point {
  int x;
  int y;
} *Point;
  void Point__move(Point self, int x, int y) {
    self->x = x;
    self->y = y;
  }


int main() {

  Point p;
  Point__move(p, 10, 10);

  return 0;
}
