
using Android.Content;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;

namespace ChessArrowDrawer;

public class ArrowOverlayView : View
{
    readonly Paint paint = new()
    {
        Color = Color.Rgb(255, 45, 55),
        StrokeWidth = 9f,
        StrokeCap = Paint.Cap.Round,
        StrokeJoin = Paint.Join.Round,
        AntiAlias = true
    };

    readonly List<(PointF Start, PointF End)> arrows = new();

    PointF? drawingStart;
    PointF? drawingEnd;

    public ArrowOverlayView(Context context) : base(context)
    {
        SetBackgroundColor(Color.Transparent);
    }

    protected override void OnDraw(Canvas? canvas)
    {
        base.OnDraw(canvas);

        if (canvas == null)
            return;

        foreach (var arrow in arrows)
            DrawArrow(canvas, arrow.Start, arrow.End);

        if (drawingStart != null && drawingEnd != null)
        {
            DrawArrow(canvas, drawingStart, drawingEnd);
        }
    }

    void DrawArrow(Canvas canvas, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;

        float length = MathF.Sqrt(dx * dx + dy * dy);

        if (length < 15)
            return;

        float angle = MathF.Atan2(dy, dx);

        float headLength = 28f;

        float leftAngle = angle + MathF.PI * 0.82f;
        float rightAngle = angle - MathF.PI * 0.82f;

        canvas.DrawLine(
            start.X, start.Y,
            end.X, end.Y,
            paint
        );

        var left = new PointF(
            end.X + headLength * MathF.Cos(leftAngle),
            end.Y + headLength * MathF.Sin(leftAngle)
        );

        var right = new PointF(
            end.X + headLength * MathF.Cos(rightAngle),
            end.Y + headLength * MathF.Sin(rightAngle)
        );

        canvas.DrawLine(
            end.X, end.Y,
            left.X, left.Y,
            paint
        );

        canvas.DrawLine(
            end.X, end.Y,
            right.X, right.Y,
            paint
        );
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e == null)
            return false;

        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:

                drawingStart = new PointF(e.GetX(), e.GetY());
                drawingEnd = drawingStart;

                Invalidate();
                return true;

            case MotionEventActions.Move:

                if (drawingStart != null)
                {
                    drawingEnd = new PointF(
                        e.GetX(),
                        e.GetY()
                    );

                    Invalidate();
                }

                return true;

            case MotionEventActions.Up:

                if (drawingStart != null && drawingEnd != null)
                {
                    arrows.Add((
                        drawingStart,
                        drawingEnd
                    ));
                }

                drawingStart = null;
                drawingEnd = null;

                Invalidate();
                return true;

            case MotionEventActions.Cancel:

                drawingStart = null;
                drawingEnd = null;

                Invalidate();
                return true;
        }

        return true;
    }

    public void ClearArrows()
    {
        arrows.Clear();
        Invalidate();
    }
}
