using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Umacalc.Tests;

public static class AssetGenerator
{
    public static void GenerateAll(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        // 1. SPEED
        GenerateIcon(Path.Combine(outputDir, "speed.png"), "#1D4ED8", "#3B82F6", dc =>
        {
            // Running shoe / winged shoe
            var shoe = new StreamGeometry();
            using (var ctx = shoe.Open())
            {
                ctx.BeginFigure(new Point(28, 74), true, true);
                ctx.LineTo(new Point(34, 52), true, false); // heel back
                ctx.BezierTo(new Point(42, 48), new Point(54, 56), new Point(62, 58), true, false); // collar
                ctx.LineTo(new Point(84, 64), true, false); // tongue/top
                ctx.BezierTo(new Point(98, 68), new Point(106, 76), new Point(102, 84), true, false); // toe
                ctx.LineTo(new Point(28, 84), true, false); // sole line
                ctx.LineTo(new Point(26, 78), true, false);
            }
            shoe.Freeze();
            dc.DrawGeometry(Brushes.White, null, shoe);

            // Sole (White/Silver tread)
            var sole = new StreamGeometry();
            using (var ctx = sole.Open())
            {
                ctx.BeginFigure(new Point(24, 84), true, true);
                ctx.LineTo(new Point(104, 84), true, false);
                ctx.LineTo(new Point(102, 92), true, false);
                ctx.LineTo(new Point(76, 92), true, false);
                ctx.LineTo(new Point(72, 88), true, false);
                ctx.LineTo(new Point(48, 88), true, false);
                ctx.LineTo(new Point(44, 92), true, false);
                ctx.LineTo(new Point(24, 92), true, false);
            }
            sole.Freeze();
            dc.DrawGeometry(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#93C5FD")), null, sole);

            // Speed motion streaks behind the shoe
            var streakPen = new Pen(Brushes.White, 3.5)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            dc.DrawLine(streakPen, new Point(14, 50), new Point(30, 50));
            dc.DrawLine(streakPen, new Point(10, 64), new Point(24, 64));
            dc.DrawLine(streakPen, new Point(16, 78), new Point(22, 78));

            // Wing / dynamic stripe on shoe
            var stripePen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D4ED8")), 3);
            dc.DrawLine(stripePen, new Point(48, 62), new Point(78, 72));
            dc.DrawLine(stripePen, new Point(54, 70), new Point(82, 78));
        });

        // 2. STAMINA
        GenerateIcon(Path.Combine(outputDir, "stamina.png"), "#C2410C", "#EA580C", dc =>
        {
            // Clean heart
            var heart = new StreamGeometry();
            using (var ctx = heart.Open())
            {
                ctx.BeginFigure(new Point(64, 98), true, true);
                ctx.BezierTo(new Point(18, 70), new Point(16, 34), new Point(44, 28), true, false);
                ctx.BezierTo(new Point(54, 25), new Point(62, 34), new Point(64, 42), true, false);
                ctx.BezierTo(new Point(66, 34), new Point(74, 25), new Point(84, 28), true, false);
                ctx.BezierTo(new Point(112, 34), new Point(110, 70), new Point(64, 98), true, false);
            }
            heart.Freeze();
            dc.DrawGeometry(Brushes.White, null, heart);

            // EKG heartbeat wave inside
            var ekgPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EA580C")), 3.5)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            var ekg = new StreamGeometry();
            using (var ctx = ekg.Open())
            {
                ctx.BeginFigure(new Point(28, 56), false, false);
                ctx.LineTo(new Point(46, 56), true, false);
                ctx.LineTo(new Point(52, 42), true, false);
                ctx.LineTo(new Point(60, 72), true, false);
                ctx.LineTo(new Point(68, 48), true, false);
                ctx.LineTo(new Point(74, 60), true, false);
                ctx.LineTo(new Point(80, 56), true, false);
                ctx.LineTo(new Point(100, 56), true, false);
            }
            ekg.Freeze();
            dc.DrawGeometry(null, ekgPen, ekg);
        });

        // 3. POWER: The iconic Umamusume flexed bicep arm with clenched fist!
        GenerateIcon(Path.Combine(outputDir, "power.png"), "#B45309", "#F59E0B", dc =>
        {
            // Bold, muscular flexed arm silhouette
            var arm = new StreamGeometry();
            using (var ctx = arm.Open())
            {
                // Start at bottom shoulder
                ctx.BeginFigure(new Point(86, 92), true, true);
                // Across upper arm base to elbow
                ctx.LineTo(new Point(54, 92), true, false);
                ctx.BezierTo(new Point(40, 92), new Point(32, 84), new Point(32, 70), true, false); // rounded elbow
                // Forearm rising up on the left
                ctx.BezierTo(new Point(32, 58), new Point(36, 46), new Point(42, 38), true, false);
                // Top of clenched fist / knuckles
                ctx.BezierTo(new Point(48, 30), new Point(62, 30), new Point(70, 36), true, false);
                // Right side of fist and thumb
                ctx.BezierTo(new Point(74, 40), new Point(76, 48), new Point(70, 54), true, false);
                // Big peaked bicep curve bulging outward
                ctx.BezierTo(new Point(84, 52), new Point(96, 62), new Point(94, 76), true, false);
                // Shoulder transition back down
                ctx.BezierTo(new Point(94, 84), new Point(90, 88), new Point(86, 92), true, false);
            }
            arm.Freeze();
            dc.DrawGeometry(Brushes.White, null, arm);

            // Bicep inner muscle definition line
            var musclePen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706")), 3)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            var muscleLine = new StreamGeometry();
            using (var ctx = muscleLine.Open())
            {
                ctx.BeginFigure(new Point(52, 60), false, false);
                ctx.BezierTo(new Point(62, 62), new Point(72, 68), new Point(74, 78), true, false);
            }
            muscleLine.Freeze();
            dc.DrawGeometry(null, musclePen, muscleLine);

            // Fist knuckle details
            var fistLine = new StreamGeometry();
            using (var ctx = fistLine.Open())
            {
                ctx.BeginFigure(new Point(52, 44), false, false);
                ctx.LineTo(new Point(62, 44), true, false);
            }
            fistLine.Freeze();
            dc.DrawGeometry(null, musclePen, fistLine);
        });

        // 4. GUTS: Burning Fighting Spirit Flame
        GenerateIcon(Path.Combine(outputDir, "guts.png"), "#BE185D", "#EC4899", dc =>
        {
            var flame = new StreamGeometry();
            using (var ctx = flame.Open())
            {
                ctx.BeginFigure(new Point(64, 18), true, true);
                ctx.BezierTo(new Point(74, 34), new Point(94, 46), new Point(94, 70), true, false);
                ctx.BezierTo(new Point(94, 88), new Point(80, 102), new Point(64, 102), true, false);
                ctx.BezierTo(new Point(48, 102), new Point(34, 88), new Point(34, 70), true, false);
                ctx.BezierTo(new Point(34, 52), new Point(46, 38), new Point(54, 48), true, false);
                ctx.BezierTo(new Point(58, 40), new Point(60, 28), new Point(64, 18), true, false);
            }
            flame.Freeze();
            dc.DrawGeometry(Brushes.White, null, flame);

            // Inner flame core in pink
            var innerFlame = new StreamGeometry();
            using (var ctx = innerFlame.Open())
            {
                ctx.BeginFigure(new Point(64, 50), true, true);
                ctx.BezierTo(new Point(72, 60), new Point(80, 72), new Point(78, 86), true, false);
                ctx.BezierTo(new Point(76, 94), new Point(68, 98), new Point(64, 98), true, false);
                ctx.BezierTo(new Point(60, 98), new Point(52, 94), new Point(50, 86), true, false);
                ctx.BezierTo(new Point(48, 76), new Point(56, 62), new Point(64, 50), true, false);
            }
            innerFlame.Freeze();
            dc.DrawGeometry(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DB2777")), null, innerFlame);
        });

        // 5. WISDOM: The official Umamusume Mortarboard Graduation Cap!
        GenerateIcon(Path.Combine(outputDir, "wisdom.png"), "#047857", "#10B981", dc =>
        {
            // A. Skull cap base (head cap underneath the mortarboard diamond)
            var capBase = new StreamGeometry();
            using (var ctx = capBase.Open())
            {
                ctx.BeginFigure(new Point(42, 62), true, true);
                ctx.BezierTo(new Point(40, 76), new Point(48, 84), new Point(64, 84), true, false);
                ctx.BezierTo(new Point(80, 84), new Point(88, 76), new Point(86, 62), true, false);
                ctx.LineTo(new Point(42, 62), true, false);
            }
            capBase.Freeze();
            dc.DrawGeometry(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5")), null, capBase);

            // B. Top Diamond (Mortarboard Board)
            var board = new StreamGeometry();
            using (var ctx = board.Open())
            {
                ctx.BeginFigure(new Point(64, 28), true, true); // Top point
                ctx.LineTo(new Point(106, 48), true, false);   // Right point
                ctx.LineTo(new Point(64, 68), true, false);    // Bottom point
                ctx.LineTo(new Point(22, 48), true, false);    // Left point
            }
            board.Freeze();
            dc.DrawGeometry(Brushes.White, null, board);

            // Board thickness edge
            var boardEdge = new StreamGeometry();
            using (var ctx = boardEdge.Open())
            {
                ctx.BeginFigure(new Point(22, 48), true, true);
                ctx.LineTo(new Point(64, 68), true, false);
                ctx.LineTo(new Point(106, 48), true, false);
                ctx.LineTo(new Point(106, 52), true, false);
                ctx.LineTo(new Point(64, 72), true, false);
                ctx.LineTo(new Point(22, 52), true, false);
            }
            boardEdge.Freeze();
            dc.DrawGeometry(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A7F3D0")), null, boardEdge);

            // C. Center button on top of board
            dc.DrawEllipse(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#047857")), null, new Point(64, 48), 4, 3);

            // D. Tassel cord hanging down the side
            var tasselPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF08A")), 2.5) // Golden yellow tassel
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            var tassel = new StreamGeometry();
            using (var ctx = tassel.Open())
            {
                ctx.BeginFigure(new Point(64, 48), false, false);
                ctx.BezierTo(new Point(46, 50), new Point(34, 56), new Point(30, 68), true, false);
                ctx.LineTo(new Point(30, 84), true, false);
            }
            tassel.Freeze();
            dc.DrawGeometry(null, tasselPen, tassel);

            // Tassel brush / fringe at bottom
            var fringe = new StreamGeometry();
            using (var ctx = fringe.Open())
            {
                ctx.BeginFigure(new Point(28, 84), true, true);
                ctx.LineTo(new Point(32, 84), true, false);
                ctx.LineTo(new Point(35, 96), true, false);
                ctx.LineTo(new Point(25, 96), true, false);
            }
            fringe.Freeze();
            dc.DrawGeometry(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FACC15")), null, fringe);
        });

        // 6. HORSE LOGO
        GenerateIcon(Path.Combine(outputDir, "horse_logo.png"), "#1E40AF", "#3B82F6", dc =>
        {
            var horseshoe = new StreamGeometry();
            using (var ctx = horseshoe.Open())
            {
                ctx.BeginFigure(new Point(36, 92), false, false);
                ctx.BezierTo(new Point(20, 68), new Point(20, 36), new Point(64, 24), true, false);
                ctx.BezierTo(new Point(108, 36), new Point(108, 68), new Point(92, 92), true, false);
            }
            horseshoe.Freeze();
            var hsPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")), 9)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            dc.DrawGeometry(null, hsPen, horseshoe);

            var horse = new StreamGeometry();
            using (var ctx = horse.Open())
            {
                ctx.BeginFigure(new Point(50, 40), true, true);
                ctx.LineTo(new Point(60, 28), true, false);
                ctx.LineTo(new Point(64, 38), true, false);
                ctx.LineTo(new Point(78, 44), true, false);
                ctx.LineTo(new Point(86, 62), true, false);
                ctx.LineTo(new Point(76, 68), true, false);
                ctx.LineTo(new Point(68, 60), true, false);
                ctx.LineTo(new Point(62, 80), true, false);
                ctx.LineTo(new Point(44, 76), true, false);
                ctx.LineTo(new Point(52, 54), true, false);
            }
            horse.Freeze();
            dc.DrawGeometry(Brushes.White, null, horse);
        });
    }

    private static void GenerateIcon(string filePath, string colorStart, string colorEnd, Action<DrawingContext> drawContent)
    {
        int size = 128;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var grad = new RadialGradientBrush(
                (Color)ColorConverter.ConvertFromString(colorEnd),
                (Color)ColorConverter.ConvertFromString(colorStart)
            );
            dc.DrawEllipse(grad, null, new Point(64, 64), 60, 60);

            var ringPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)), 2);
            dc.DrawEllipse(null, ringPen, new Point(64, 64), 59, 59);

            drawContent(dc);
        }

        var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using var stream = File.Create(filePath);
        encoder.Save(stream);
    }
}
