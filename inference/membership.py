from __future__ import annotations


class Membership:
    @staticmethod
    def linear_up(x: float, a: float, b: float) -> float:
        if x <= a:
            return 0.0
        if x >= b:
            return 1.0
        return (x - a) / (b - a)

    @staticmethod
    def linear_down(x: float, a: float, b: float) -> float:
        if x <= a:
            return 1.0
        if x >= b:
            return 0.0
        return (b - x) / (b - a)

    @staticmethod
    def triangle(x: float, a: float, b: float, c: float) -> float:
        if x <= a or x >= c:
            return 0.0
        if x == b:
            return 1.0
        if x < b:
            return (x - a) / (b - a)
        return (c - x) / (c - b)

    @staticmethod
    def trapezoid(x: float, a: float, b: float, c: float, d: float) -> float:
        if x <= a or x >= d:
            return 0.0
        if b <= x <= c:
            return 1.0
        if x < b:
            return (x - a) / (b - a)
        return (d - x) / (d - c)
