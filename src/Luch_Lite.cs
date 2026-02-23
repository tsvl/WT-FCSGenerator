using System;
using System.IO;
using System.Linq;

namespace FCS
{
    public static class Luch_Lite
    {
        public static string Create(
                double Speed,
                double ZoomIn,
                double ZoomOut,
                double Sensitivity,
                bool DrawCrosshairDist,
                double CrosshairDistSize,
                double CrosshairDistAdditonalSize,
                bool DrawCrosshairDistText,
                string rangefinderProgressBarColor1,
                string rangefinderProgressBarColor2,
                string crosshairLightColor,
                double lineSizeMult,
                double Length,
                double Height,
                double PointThickness,
                string Milliradian,
                double TargetSpeed,
                string BallisticDataFromFile
                )
        {
            TargetSpeed /= 3.6;
            double Size = 7.1429 * ZoomIn;
            double InnerDiameter = 4 * Size / 150;
            double Scaling = 43 * Math.Pow(ZoomOut, -1.02);
            double Radius = 5000000;
            double ScrollStep = 2.8 * (SightConfig.VerticalResolution / 1080.0) * Math.Pow(Sensitivity, 2);
            double ScrollAngle = 0.005;
            double ScrollSpeed = ScrollAngle * Radius / ScrollStep * (Math.PI / 180.0);
            double TargetDistance = 0;
            double MaxAngle = 0.09;
            double Angle = BallisticDataFromFile.Count(x => x == '\n') * ScrollStep;
            if (Angle > MaxAngle)
            {
                Angle = MaxAngle;
            }
            StringReader reader1 = new StringReader(BallisticDataFromFile);
            double[,] BallisticData = new double[3, BallisticDataFromFile.Count(x => x == '\n')];
            int MaxLength = 243;
            string line = null;
            for (int i = 0; (line = reader1.ReadLine()) != null && i < BallisticData.GetLength(1); i++)
            {
                BallisticData[0, i] = Convert.ToDouble(line.Split('\t')[0]);
                BallisticData[1, i] = Convert.ToDouble(line.Split('\t')[1]);
                BallisticData[2, i] = Convert.ToDouble(line.Split('\t')[2]);
            }
            double MaxDistance = 3400;
            double FixedDistance = 200;
            double CurrentStep = 0;
            double[] FixedDistances = new double[Convert.ToInt16(MaxDistance / FixedDistance)];
            for (int i = 0; i < FixedDistances.GetLength(0); i++)
            {
                double FirstDistance = 0;
                TargetDistance = FixedDistance * (i + 1);
                for (int j = 0; j < BallisticData.GetLength(1) && FixedDistances[i] == 0; j++)
                {
                    CurrentStep = ScrollStep * (j);
                    double SecondDistance = BallisticData[0, j];
                    if (TargetDistance >= FirstDistance && TargetDistance < SecondDistance)
                    {
                        FixedDistances[i] = CurrentStep + (TargetDistance - FirstDistance) / (SecondDistance - FirstDistance) * ScrollStep;
                    }
                    FirstDistance = BallisticData[0, j];
                }
            }
            double FontSize = 5 * 0.75 * Scaling * Size / 150 * 0.5;
            FontSize /= 2;
            for (int i = 0; FixedDistances[Convert.ToInt16(Math.Pow(2, i))] < FontSize; i++)
            {
                FixedDistance *= 2;
            }
            for (int i = 0; i < BallisticData.GetLength(1); i++)
            {
                if (BallisticData[0, i] < (MaxDistance))
                {
                    Angle = ScrollStep * (BallisticData.GetLength(1));
                }
            }
            if (Angle > Size / 150 * 55)
            {
                Angle = Size / 150 * 55;
            }
            double CenterThousand = Math.Atan(TargetSpeed / (Speed * 0.9)) * 1000;
            string fcs_data = @"drawAdditionalLines:b = no
thousandth:t = " + '\u0022' + Milliradian + '\u0022' + @"

rangefinderProgressBarColor1:c = " + rangefinderProgressBarColor1 + @"
rangefinderProgressBarColor2:c = " + rangefinderProgressBarColor2 + @"
rangefinderTextScale:r = 0.7
rangefinderVerticalOffset:r = 0.01
rangefinderHorizontalOffset:r = 120
rangefinderUseThousandth:b = no

detectAllyTextScale:r = 0.7
detectAllyOffset:p2 = 120, -0.01

fontSizeMult:r = " + Convert.ToString(Math.Round(0.75 * Scaling * Size / 150, 5)) + @"
lineSizeMult:r = " + Convert.ToString(lineSizeMult) + @"

drawCentralLineVert:b = no
drawCentralLineHorz:b = no
drawSightMask:b = no

crosshairLightColor:c = " + crosshairLightColor + @"

drawDistanceCorrection:b = yes
distanceCorrectionPos:p2 = -0.035, 0.12

crosshair_distances { }

crosshair_hor_ranges{ }

drawCircles {
    // Barrel lift point
    circle {
        segment:p2 = 0.0, 360.0;
        pos:p2 = 0.0, 0.0;
        diameter:r = 0.1;
        size:r = " + Convert.ToString(Math.Round(PointThickness * Size / 150, 2)) + @";
        move:b = yes;
        thousandth:b = yes;
    }
}

drawTexts {";
            double CurrentAngle = 0.0;
            if ((DrawCrosshairDist == true) && (DrawCrosshairDistText == true))
            {
                fcs_data += @"
    // Marks on the distance lines";
                int i;
                double Position = 3;
                double align = -1;
                for (i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0); i += Convert.ToInt16(FixedDistance / 200))
                {
                    if (FixedDistances[i] > InnerDiameter)
                    {
                        double Distance = (i + 1) * 2;
                        Position = -Position;
                        align = -align;
                        fcs_data += @"
    text {
        text:t = " + '\u0022' + Convert.ToString(Distance) + '\u0022' + @"
        align:i = " + Convert.ToString(align) + @"
        pos:p2 = " + Convert.ToString(Math.Round(Position * Size / 150, 2)) + @", " + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @"
        radialAngle:r = " + Convert.ToString(CurrentAngle) + @"
        radialCenter:p2 = " + Convert.ToString(Radius) + @", 0
        radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"
        moveRadial:b = yes
        thousandth:b = yes
        size:r = 0.5
        highlight:b = yes
    }";
                    }
                }
                fcs_data += @"

    // Marks on lines of preemptive";
                i = -5;
                while (i <= 5)
                {
                    if (i != 0)
                    {
                        Position = CenterThousand * i;
                        fcs_data += @"
    text {
        text:t = " + '\u0022' + Convert.ToString(Math.Round(TargetSpeed * 3.6 * Math.Abs(Convert.ToDouble(i)))) + '\u0022' + @"
        align:i = 0
        pos:p2 = " + Convert.ToString(Math.Round(Position, 2)) + @", -" + Convert.ToString(Math.Round(2 * Size / 150, 2)) + @"
        radialAngle:r = " + Convert.ToString(CurrentAngle) + @"
        radialCenter:p2 = " + Convert.ToString(Radius) + @", 0
        radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"
        moveRadial:b = yes
        thousandth:b = yes
        size:r = 0.5
        highlight:b = yes
    }";

                    }
                    i++;
                }
            }
            fcs_data += @"
}

drawLines {
    // Interface
    // Central triangle
    line { line:p4 = 0, 0, " + Convert.ToString(Math.Round(2.5 * Size / 150, 2)) + @", " + Convert.ToString(Math.Round(InnerDiameter, 2)) + @"; thousandth:b = yes;}
    line { line:p4 = 0, 0, -" + Convert.ToString(Math.Round(2.5 * Size / 150, 2)) + @", " + Convert.ToString(Math.Round(InnerDiameter, 2)) + @"; thousandth:b = yes;}";
            fcs_data += @"

    // Lines of preemptive";
            for (int i = 1; i <= 5; i++)
            {
                fcs_data += @"
    // " + Convert.ToString(i * 10 - 5) + @" km/h
    line { line:p4 = " + Convert.ToString(Math.Round(CenterThousand * i - CenterThousand / 2, 2)) + @", 0, " + Convert.ToString(Math.Round(CenterThousand * i - CenterThousand / 2, 2)) + @", " + Convert.ToString(Math.Round(0.75 * Size / 150, 2)) + @"; thousandth:b = yes;}
    line { line:p4 = -" + Convert.ToString(Math.Round(CenterThousand * i - CenterThousand / 2, 2)) + @", 0, -" + Convert.ToString(Math.Round(CenterThousand * i - CenterThousand / 2, 2)) + @", " + Convert.ToString(Math.Round(0.75 * Size / 150, 2)) + @"; thousandth:b = yes;}

    // " + Convert.ToString(i * 10) + @" km/h
    line { line:p4 = " + Convert.ToString(Math.Round(CenterThousand * i, 2)) + @", 0, " + Convert.ToString(Math.Round(CenterThousand * i, 2)) + @", " + Convert.ToString(Math.Round(1.5 * Size / 150, 2)) + @"; thousandth:b = yes;}
    line { line:p4 = -" + Convert.ToString(Math.Round(CenterThousand * i, 2)) + @", 0, -" + Convert.ToString(Math.Round(CenterThousand * i, 2)) + @", " + Convert.ToString(Math.Round(1.5 * Size / 150, 2)) + @"; thousandth:b = yes;}";
            }
            fcs_data += @"

    // Barrel lift line
    line { line:p4 = 0, -" + Convert.ToString(Math.Round(InnerDiameter, 2)) + @", 0, -" + Convert.ToString(Math.Round(3 * Size / 150, 2)) + @"; move:b = yes; thousandth:b = yes;}";

            CurrentAngle = 0.0;
            if (DrawCrosshairDist == true)
            {
                fcs_data += @"

    // Range correction lines
    // Central line
    line { line:p4 = 0, " + Convert.ToString(Math.Round(InnerDiameter, 2)) + @", 0, " + Convert.ToString(Math.Round(Angle + (Size / 150 * Angle / 20), 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}

    // Distance lines";
                double LastDistance = 0;
                for (int i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0) && FixedDistances[i] < Angle; i += Convert.ToInt16(FixedDistance / 200))
                {
                    if ((FixedDistances[i] - LastDistance) / 2 + LastDistance > InnerDiameter)
                    {
                        fcs_data += @"
    line { line:p4 = -" + Convert.ToString(Math.Round(CrosshairDistAdditonalSize * Size / 150 / 2, 2)) + @", " + Convert.ToString(Math.Round((FixedDistances[i] - LastDistance) / 2 + LastDistance, 2)) + @", " + Convert.ToString(Math.Round(CrosshairDistAdditonalSize * Size / 150 / 2, 2)) + @", " + Convert.ToString(Math.Round((FixedDistances[i] - LastDistance) / 2 + LastDistance, 2)) + @"; radialAngle:r = " + Convert.ToString(Math.Round(CurrentAngle, 3)) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}";
                    }
                    if (FixedDistances[i] > InnerDiameter)
                    {
                        fcs_data += @"
    line { line:p4 = -" + Convert.ToString(Math.Round(CrosshairDistSize * Size / 150 / 2, 2)) + @", " + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @", " + Convert.ToString(Math.Round(CrosshairDistSize * Size / 150 / 2, 2)) + @", " + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @"; radialAngle:r = " + Convert.ToString(Math.Round(CurrentAngle, 3)) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}";
                    }
                    LastDistance = FixedDistances[i];
                }
            }
            CurrentAngle = ScrollAngle;
            for (int i = 0; i < BallisticData.GetLength(1) && i < MaxLength && BallisticData[0, i] < MaxDistance; i++)
            {
                double CurrentDistance = BallisticData[0, i];
                double FrameHeight = Math.Atan(Height / CurrentDistance) * 1000 / 2;
                double FrameLength = Math.Atan(Length / CurrentDistance) * 1000 / 2;
                if (CurrentDistance < 100)
                {
                    CurrentDistance = Math.Floor(CurrentDistance / 5) * 5;
                }
                if ((CurrentDistance >= 100) && (CurrentDistance < 400))
                {
                    CurrentDistance = Math.Floor(CurrentDistance / 10) * 10;
                }
                if (CurrentDistance >= 400)
                {
                    CurrentDistance = Math.Floor(CurrentDistance / 50) * 50;
                }
                fcs_data += @"

    // " + Convert.ToString(CurrentDistance) + @"m
    // Top and bottom horizontal lines
    line { line:p4 = -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @", -" + Convert.ToString(Math.Round(FrameLength / 2, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = " + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @", " + Convert.ToString(Math.Round(FrameLength / 2, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @", -" + Convert.ToString(Math.Round(FrameLength / 2, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = " + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @", " + Convert.ToString(Math.Round(FrameLength / 2, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}

    // Right and left vertical lines
    line { line:p4 = -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @", -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = " + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight, 2)) + @", " + Convert.ToString(Math.Round(FrameLength, 2)) + @", -" + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @", -" + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}
    line { line:p4 = " + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight, 2)) + @", " + Convert.ToString(Math.Round(FrameLength, 2)) + @", " + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @"; radialAngle:r = " + Convert.ToString(CurrentAngle) + @"; radialCenter:p2 = " + Convert.ToString(Radius) + @", 0; radialMoveSpeed:r = " + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @"; moveRadial:b = yes; thousandth:b = yes;}";
                CurrentAngle += ScrollAngle;
            }
            fcs_data += @"
}";
            return fcs_data;
        }
    }
}
