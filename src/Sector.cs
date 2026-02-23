using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace FCS
{
    public static class Sector
    {
        public static string Create(
            string SightType,
            double Speed,
            double ZoomIn,
            double ZoomOut,
            double Sensitivity,
            double lineSizeMult,
            double InnerDiameter,
            double PointThickness,
            double[] RangerFinderPos,
            double[] DetectAllyPos,
            double[] DistancePos,
            bool DrawDisnaceCorrections,
            string rangefinderProgressBarColor1,
            string rangefinderProgressBarColor2,
            string crosshairLightColor,
            double Length,
            double Width,
            string Milliradian,
            double TargetSpeed,
            string BallisticDataFromFile,
            double FontSizeMult,
            double DistanceFactor
            )
        {
            Length = 20;
            Width = 10;
            TargetSpeed /= 3.6;
            double Size = 7.1429 * ZoomIn;
            double SizeMax = 10 * ZoomOut;
            double Scaling = 43 * Math.Pow(ZoomOut, -1.02);
            InnerDiameter = InnerDiameter * Size / 150;
            double Radius = 1000000;
            double ScrollStep = 2.8 * (SightConfig.VerticalResolution / 1080.0) * Math.Pow(Sensitivity, 2);
            double ScrollAngle = 0.05;
            double ScrollSpeed = ScrollAngle * Radius / ScrollStep * (Math.PI / 180.0);
            double TargetDistance = 0;
            double Angle = 0;
            int DistanceStep = 100;
            int MaxDistance = 4000;
            double MinAvgSpeed = 0;
            double RealMaxAngle = 0;
            string line = null;
            double[,] BallisticData = new double[3, MaxDistance / DistanceStep];
            int MaxLength = 364;
            if (SightType != "Rocket")
            {
                BallisticData = new double[3, BallisticDataFromFile.Count(x => x == '\n')];
                StringReader reader1 = new StringReader(BallisticDataFromFile);
                for (int i = 0; (line = reader1.ReadLine()) != null && i < BallisticData.GetLength(1); i++)
                {
                    BallisticData[0, i] = Convert.ToDouble(line.Split('\t')[0]);
                    BallisticData[1, i] = Convert.ToDouble(line.Split('\t')[1]);
                    if (BallisticData[0, i] < MaxDistance)
                    {
                        MinAvgSpeed = BallisticData[0, i] / BallisticData[1, i];
                        RealMaxAngle = i * ScrollStep;
                    }
                }
            }
            if (SightType == "Rocket")
            {
                BallisticData = new double[3, MaxDistance / DistanceStep];
                for (int i = 0; i < BallisticData.GetLength(1); i++)
                {
                    BallisticData[0, i] = DistanceStep * i;
                }
            }
            int FixedDistance = 200;
            double CurrentStep = 0;
            double[] FixedDistances = new double[MaxDistance / FixedDistance];
            for (int i = 0; i < FixedDistances.GetLength(0); i++)
            {
                double FirstDistance = 0;
                TargetDistance = FixedDistance * (i + 1);
                for (int j = 0; j < BallisticData.GetLength(1) && FixedDistances[i] == 0; j++)
                {
                    CurrentStep = ScrollStep * (j - 1);
                    double SecondDistance = BallisticData[0, j];
                    if (TargetDistance >= FirstDistance && TargetDistance < SecondDistance)
                    {
                        FixedDistances[i] = CurrentStep + (TargetDistance - FirstDistance) / (SecondDistance - FirstDistance) * ScrollStep;
                    }
                    FirstDistance = BallisticData[0, j];
                }
            }
            double FontSize = 5 * FontSizeMult * Size / 150 * 0.7;
            FontSize /= 2;
            FontSize *= DistanceFactor;
            for (int i = 0; Convert.ToInt16(Math.Pow(2, (i + 1))) < FixedDistances.GetLength(0) && FixedDistances[Convert.ToInt16(Math.Pow(2, (i + 1)))] - FixedDistances[Convert.ToInt16(Math.Pow(2, i))] < FontSize; i++)
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
            if (Angle > SizeMax)
            {
                Angle = SizeMax;
            }
            double CenterThousand = Math.Asin(TargetSpeed / (Speed)) * 1000;
            double PreemptiveSpeed = (Math.Asin(TargetSpeed / MinAvgSpeed) * 1000 - CenterThousand) / RealMaxAngle;
            CenterThousand *= 0.98;
            if (SightType == "Rocket")
            {
                CenterThousand = Math.Round(10 * Size / 150, 3);
            }
            double CenterThousand2 = CenterThousand;
            double CenterHalfThousand = CenterThousand / 2;
            if (CenterThousand < InnerDiameter / 2) { CenterThousand = InnerDiameter / 2; }
            if (CenterHalfThousand < InnerDiameter / 2) { CenterHalfThousand = InnerDiameter / 2; }

            string fcs_data = @"thousandth:t=" + '\u0022' + Milliradian + '\u0022' + @"
rangefinderProgressBarColor1:c=" + rangefinderProgressBarColor1 + @"
rangefinderProgressBarColor2:c=" + rangefinderProgressBarColor2 + @"
rangefinderTextScale:r=1
rangefinderVerticalOffset:r=" + RangerFinderPos[1] + @"
rangefinderHorizontalOffset:r=" + RangerFinderPos[0] + @"
rangefinderUseThousandth:b=no
detectAllyTextScale:r=1
detectAllyOffset:p2=" + DetectAllyPos[0] + @"," + DetectAllyPos[1] + @"
fontSizeMult:r=" + Convert.ToString(Math.Round(FontSizeMult * Scaling * Size / 150, 5)) + @"
lineSizeMult:r=" + Convert.ToString(lineSizeMult) + @"
drawCentralLineVert:b=no
drawCentralLineHorz:b=no
crosshairLightColor:c=" + crosshairLightColor + @"
drawAdditionalLines:b = no
distancePos:p2 = 0,0
move:b = yes
crosshairDistHorSizeMain:p2=0.0, 0.004
distanceCorrectionPos:p2=" + DistancePos[0] + @"," + DistancePos[1] + @"
drawDistanceCorrection:b=";
            if (DrawDisnaceCorrections == true)
            {
                fcs_data += "yes";
            }
            else
            {
                fcs_data += "no";
            }

            fcs_data += @"
crosshair_distances {";
            if (SightType != "Rocket")
            {
                double Position = -0.01;
                double SizeText;
                double SizeText1 = -0.0083;
                double SizeText2 = 0.024;
                for (int i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0); i += Convert.ToInt16(FixedDistance / 200))
                {
                    double Distance = ((i + 1) * 200);
                    if (Distance >= FixedDistance)
                    {
                        if (Distance >= 1000)
                        {
                            SizeText1 = -0.0085;
                            SizeText2 = 0.027;
                        }
                        Position *= -1;
                        if (Position >= 0)
                        {
                            SizeText = SizeText1;
                        }
                        else { SizeText = SizeText2; }
                        fcs_data += @"
distance { distance:p3=" + Distance + @"," + Distance / 100 + @"," + Position + @"; textPos:p2=" + SizeText + @",0; }";
                    }
                }
            }
            fcs_data += @"
}
crosshair_hor_ranges{ }";
            fcs_data += @"
drawCircles{
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(InnerDiameter, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(2 * PointThickness * Size / 150, 2)) + @";thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(Size / 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";move:b=yes;thousandth:b=yes}";
            if (SightType != "Rocket")
            {
                fcs_data += @"
circle{segment:p2=95.0,145.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=35.0,85.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=335.0,385.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=275.0,325.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=215.0,265.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=155.0,205.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 40, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20, 5)) + @";moveRadial:b=yes;thousandth:b=yes}

circle{segment:p2=95.0,145.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=35.0,85.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=335.0,385.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=275.0,325.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=215.0,265.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=155.0,205.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 80, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 40, 5)) + @";moveRadial:b=yes;thousandth:b=yes}

circle{segment:p2=95.0,145.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=35.0,85.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=335.0,385.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=275.0,325.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=215.0,265.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=155.0,205.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 120, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 60, 5)) + @";moveRadial:b=yes;thousandth:b=yes}

circle{segment:p2=95.0,145.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=35.0,85.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=335.0,385.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=275.0,325.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=215.0,265.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=155.0,205.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 160, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 80, 5)) + @";moveRadial:b=yes;thousandth:b=yes}

circle{segment:p2=95.0,145.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=35.0,85.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @"," + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=335.0,385.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=275.0,325.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=-" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=215.0,265.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",-" + Convert.ToString(Math.Cos(Math.PI * 30 / 180) * Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=155.0,205.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(CenterThousand * 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 100, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
";

            }
            if (SightType != "Rocket")
            {
                double CurrentAngle = ScrollAngle;
                for (int i = 1; i < BallisticData.GetLength(1) && i < MaxLength && BallisticData[0, i] < MaxDistance; i++)
                {
                    double CurrentDistance = BallisticData[0, i];
                    double FrameLength = Math.Atan(Length / CurrentDistance) * 1000;
                    double FrameWidth = Math.Atan(Width / CurrentDistance) * 1000;
                    fcs_data += @"
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(FrameLength, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(FrameWidth, 2)) + @";size:r=" + Convert.ToString(lineSizeMult * 2) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    CurrentAngle += ScrollAngle;
                }
            }
            fcs_data += @"
}

drawTexts {";
            if (SightType != "Rocket")
            {
                double Position = 0;
                int a = -5;
                while (a <= 5)
                {
                    if (a != 0)
                    {
                        Position = CenterThousand * 20 * a;
                        fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(Math.Round(TargetSpeed * 3.6 * Math.Abs(Convert.ToDouble(a)) * 20)) + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Position, 2)) + @",-" + Convert.ToString(Math.Round(5 * Size / 150, 2)) + @";radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 20 * a, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1.5;highlight:b=yes}";
                    }
                    a++;
                }
            }
            fcs_data += @"
}
drawLines {";
            if (SightType != "Rocket")
            {
                fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0," + Convert.ToString(Math.Round(SizeMax, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0,-" + Convert.ToString(Math.Round(SizeMax, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @"," + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @"," + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * SizeMax, 2)) + @"," + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * SizeMax, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @"," + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * SizeMax, 2)) + @"," + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * SizeMax, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @"," + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * SizeMax, 2)) + @",-" + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * SizeMax, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(Math.Cos(Math.PI * 60 / 180) * SizeMax, 2)) + @",-" + Convert.ToString(Math.Round(Math.Sin(Math.PI * 60 / 180) * SizeMax, 2)) + @";move:b=false;thousandth:b=yes}
}";
            }
            return fcs_data;
        }
    }
}
