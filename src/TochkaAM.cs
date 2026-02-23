using System;
using System.IO;
using System.Linq;

namespace FCS
{
    public static class TochkaSM2
    {
        public static string Create(
            string SightType,
            string Type,
            string Type2,
            double Speed,
            double Speed2,
            double ArmorPower,
            double ArmorPower2,
            double ZoomIn,
            double ZoomOut,
            double Sensitivity,
            string Language,
            bool DrawCrosshairDistShow,
            bool drawOuterLinesShow,
            bool BalisticInfoShow,
            bool SightNameShow,
            bool TankSizesShow,
            bool TargetLockShow,
            bool RangerFinderShow,
            bool DistanceShow,
            double lineSizeMult,
            double InnerDiameter,
            double PointThickness,
            double[] RangerFinderPos,
            double[] DistancePos,
            double[] DetectAllyPos,
            string rangefinderProgressBarColor1,
            string rangefinderProgressBarColor2,
            string crosshairLightColor,
            double Length,
            double Height,
            double Width,
            string Milliradian,
            double TargetSpeed,
            string LangName,
            string LangName2,
            string BallisticDataFromFile,
            string BallisticDataFromFile2,
            string LangRocketName,
            double RocketSpeed,
            double RocketArmorPower,
            string LangData,
            double FontSizeMult,
            double DistanceFactor
            )
        {
            TargetSpeed /= 3.6;
            double Size = 7.1429 * ZoomIn;
            double SizeMax = 5 * ZoomOut;
            double Scaling = 43 * Math.Pow(ZoomOut, -1.02);
            InnerDiameter = InnerDiameter * Size / 150;
            double Radius = 1000000;
            double ScrollStep = 2.8 * (SightConfig.VerticalResolution / 1080.0) * Math.Pow(Sensitivity, 2);
            double ScrollAngle = 0.05;
            double ScrollingBug = 0.3471 * ScrollAngle;
            double ScrollSpeed = ScrollAngle * Radius / ScrollStep * (Math.PI / 180.0);
            double TargetDistance = 0;
            double MaxAngle = 0.09;
            double Angle = 0;
            int DistanceStep = 100;
            int MaxDistance = 4000;
            double MinAvgSpeed = 0;
            double RealMaxAngle = 0;
            string line = null;
            double[,] BallisticData = new double[3, MaxDistance / DistanceStep];
            if (SightType != "Rocket")
            {
                BallisticData = new double[3, BallisticDataFromFile.Count(x => x == '\n')];
                StringReader reader1 = new StringReader(BallisticDataFromFile);
                for (int i = 0; (line = reader1.ReadLine()) != null && i < BallisticData.GetLength(1); i++)
                {
                    BallisticData[0, i] = Convert.ToDouble(line.Split('\t')[0]);
                    BallisticData[1, i] = Convert.ToDouble(line.Split('\t')[1]);
                    BallisticData[2, i] = Convert.ToDouble(line.Split('\t')[2]);
                    if (BallisticData[0, i] < MaxDistance)
                    {
                        MinAvgSpeed = BallisticData[0, i] / BallisticData[1, i];
                        RealMaxAngle = i * ScrollStep;
                    }
                }
            }
            int MaxLength = 364;
            int MaxLines = 2500;
            if (SightType == "Double")
            {
                MaxLength /= 2;
            }
            if (SightType == "Rocket")
            {
                BallisticData = new double[3, MaxDistance / DistanceStep];
                for (int i = 0; i < BallisticData.GetLength(1); i++)
                {
                    BallisticData[0, i] = DistanceStep * i;
                    BallisticData[1, i] = Math.Round(DistanceStep * i / Speed, 1);
                    BallisticData[2, i] = 0;
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

            double Angle2 = Angle;
            double[,] BallisticData2 = new double[3, MaxDistance / DistanceStep];
            double[] FixedDistances2 = new double[MaxDistance / FixedDistance];
            int FixedDistance2 = 200;
            double FontSize2 = 5 * FontSizeMult * Size / 150 * 0.7;
            FontSize2 /= 2;
            FontSize2 *= DistanceFactor;
            if (SightType == "Double")
            {
                Angle2 = BallisticDataFromFile2.Count(x => x == '\n') * ScrollStep;
                if (Angle2 > MaxAngle)
                {
                    Angle2 = MaxAngle;
                }
                StringReader reader2 = new StringReader(BallisticDataFromFile2);
                BallisticData2 = new double[3, BallisticDataFromFile2.Count(x => x == '\n')];
                for (int i = 0; (line = reader2.ReadLine()) != null && i < BallisticData2.GetLength(1); i++)
                {
                    BallisticData2[0, i] = Convert.ToDouble(line.Split('\t')[0]);
                    BallisticData2[1, i] = Convert.ToDouble(line.Split('\t')[1]);
                    BallisticData2[2, i] = Convert.ToDouble(line.Split('\t')[2]);
                }
                for (int i = 0; i < FixedDistances2.GetLength(0); i++)
                {
                    double FirstDistance = 0;
                    TargetDistance = FixedDistance2 * (i + 1);
                    for (int j = 0; j < BallisticData2.GetLength(1) && FixedDistances2[i] == 0; j++)
                    {
                        CurrentStep = ScrollStep * (j - 1);
                        double SecondDistance = BallisticData2[0, j];
                        if (TargetDistance >= FirstDistance && TargetDistance < SecondDistance)
                        {
                            FixedDistances2[i] = CurrentStep + (TargetDistance - FirstDistance) / (SecondDistance - FirstDistance) * ScrollStep;
                        }
                        FirstDistance = BallisticData2[0, j];
                    }
                }
                for (int i = 0; Convert.ToInt16(Math.Pow(2, (i + 1))) < FixedDistances2.GetLength(0) && FixedDistances2[Convert.ToInt16(Math.Pow(2, (i + 1)))] - FixedDistances2[Convert.ToInt16(Math.Pow(2, i))] < FontSize2; i++)
                {
                    FixedDistance2 *= 2;
                }
                for (int i = 0; i < BallisticData2.GetLength(1); i++)
                {
                    if (BallisticData2[0, i] < (MaxDistance))
                    {
                        Angle2 = ScrollStep * (BallisticData2.GetLength(1));
                    }
                }
                if (Angle2 > SizeMax)
                {
                    Angle2 = SizeMax;
                }
            }
            double RadiusX = 100000 * Length;
            double RadiusY = 100000 * Height;
            double ScrollSpeedLaser = 0;
            if (SightType == "Laser")
            {
                ScrollSpeedLaser = Math.Sqrt(Height * Height + Length * Length) / FixedDistances[11];
            }
            double CenterThousand = Math.Asin(TargetSpeed / (Speed)) * 1000;
            double PreemptiveSpeed = (Math.Asin(TargetSpeed / MinAvgSpeed) * 1000 - CenterThousand) / RealMaxAngle;
            double CenterThousandDouble = 0;
            if (SightType == "Double")
            {
                CenterThousandDouble = Math.Asin(TargetSpeed / (Speed2)) * 1000;
            }
            CenterThousand *= 0.98;
            if (SightType == "Rocket")
            {
                CenterThousand = Math.Round(10 * Size / 150, 3);
            }
            double CenterThousand2 = CenterThousand;
            double CenterHalfThousand = CenterThousand / 2;
            if (CenterThousand < InnerDiameter / 2) { CenterThousand = InnerDiameter / 2; }
            if (CenterHalfThousand < InnerDiameter / 2) { CenterHalfThousand = InnerDiameter / 2; }
            string Rangefinder = "Rangefinder";
            string Capture = "Target lock";
            string ON = "ON";
            string SightName = "Tochka-SM2";
            if (SightType == "Double")
            {
                SightName = "Tochka-SMD2";
            }
            if (SightType == "Laser")
            {
                SightName = "Tochka-SML2";
            }
            if (SightType == "Rocket")
            {
                SightName = "Tochka-SMR2";
            }
            if (SightType == "Howitzer")
            {
                SightName = "Tochka-SMH2";
            }
            string Elevation = "Distance";
            string Meters = "Meters";
            string Seconds = "Seconds";
            string Millimetrs = "Millimeters";
            string LengthString = "Length";
            string WidthString = "Width";
            string HeightString = "Height";
            int Pos = 1;
            if (Language == "English") { Pos = 1; }
            if (Language == "French") { Pos = 2; }
            if (Language == "Italian") { Pos = 3; }
            if (Language == "German") { Pos = 4; }
            if (Language == "Spanish") { Pos = 5; }
            if (Language == "Russian") { Pos = 6; }
            if (Language == "Polish") { Pos = 7; }
            if (Language == "Czech") { Pos = 8; }
            if (Language == "Turkish") { Pos = 9; }
            if (Language == "Chinese") { Pos = 10; }
            if (Language == "Japanese") { Pos = 11; }
            if (Language == "Portuguese") { Pos = 12; }
            if (Language == "Ukrainian") { Pos = 13; }
            if (Language == "Serbian") { Pos = 14; }
            if (Language == "Hungarian") { Pos = 15; }
            if (Language == "Korean") { Pos = 16; }
            if (Language == "Belarusian") { Pos = 17; }
            if (Language == "Romanian") { Pos = 18; }
            if (Language == "TChinese") { Pos = 19; }
            if (Language == "HChinese") { Pos = 20; }
            if (Language == "Russian" || Language == "Ukrainian" || Language == "Belarusian") { SightName = "Точка-AM2"; }
            if (SightType == "Double" && (Language == "Russian" || Language == "Ukrainian" || Language == "Belarusian"))
            {
                SightName = "Точка-АМД2";
            }
            if (SightType == "Laser" && (Language == "Russian" || Language == "Ukrainian" || Language == "Belarusian"))
            {
                SightName = "Точка-АМЛ2";
            }
            if (SightType == "Rocket" && (Language == "Russian" || Language == "Ukrainian" || Language == "Belarusian"))
            {
                SightName = "Точка-АМР2";
            }
            if (SightType == "Howitzer" && (Language == "Russian" || Language == "Ukrainian" || Language == "Belarusian"))
            {
                SightName = "Точка-АМГ2";
            }
            StringReader reader = new StringReader(LangData);
            string line1 = String.Empty;
            while ((line1 = reader.ReadLine()) != null)
            {
                if (line1.Split(';')[0].Replace("\"", "") == Rangefinder)
                {
                    Rangefinder = line1.Split(';')[Pos];
                    Rangefinder = Rangefinder.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == Capture)
                {
                    Capture = line1.Split(';')[Pos];
                    Capture = Capture.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == ON)
                {
                    ON = line1.Split(';')[Pos];
                    ON = ON.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == Elevation)
                {
                    Elevation = line1.Split(';')[Pos];
                    Elevation = Elevation.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == Meters)
                {
                    Meters = line1.Split(';')[Pos];
                    Meters = Meters.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == Seconds)
                {
                    Seconds = line1.Split(';')[Pos];
                    Seconds = Seconds.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == Millimetrs)
                {
                    Millimetrs = line1.Split(';')[Pos];
                    Millimetrs = Millimetrs.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == LengthString)
                {
                    LengthString = line1.Split(';')[Pos];
                    LengthString = LengthString.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == WidthString)
                {
                    WidthString = line1.Split(';')[Pos];
                    WidthString = WidthString.Replace("\"", "");
                }
                if (line1.Split(';')[0].Replace("\"", "") == HeightString)
                {
                    HeightString = line1.Split(';')[Pos];
                    HeightString = HeightString.Replace("\"", "");
                }
            }

            string fcs_data = @"drawAdditionalLines:b=no
thousandth:t=" + '\u0022' + Milliradian + '\u0022' + @"
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
drawDistanceCorrection:b=no
crosshair_distances{}
crosshair_hor_ranges{}

drawCircles{
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(InnerDiameter, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(2 * PointThickness * Size / 150, 2)) + @";thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(Size / 200, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";move:b=yes;thousandth:b=yes}";
            if (drawOuterLinesShow == true)
            {
                fcs_data += @"
circle{segment:p2=60.0,120.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(Size * 2, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";thousandth:b=yes}
circle{segment:p2=240.0,300.0;pos:p2=0.0,0.0;diameter:r=" + Convert.ToString(Math.Round(Size * 2, 2)) + @";size:r=" + Convert.ToString(lineSizeMult) + @";thousandth:b=yes}";
            }
            double CurrentAngle = 0.0;
            if (DrawCrosshairDistShow == true && SightType != "Rocket")
            {
                double LastDistance = 0;
                double PastDistance = 0;
                for (int i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0) && FixedDistances[i] < Angle; i += Convert.ToInt16(FixedDistance / 200))
                {
                    double Distance = ((i + 1) * 200);
                    double AdditionalDistance = (Distance + PastDistance) / 2;
                    if (AdditionalDistance >= FixedDistance)
                    {
                        fcs_data += @"
circle{segment:p2=0.0,360.0;pos:p2=" + Convert.ToString(Math.Round(Math.Atan(Length / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances[i] + LastDistance) / 2, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=" + Convert.ToString(Math.Round(Math.Atan(Width / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances[i] + LastDistance) / 2, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=-" + Convert.ToString(Math.Round(Math.Atan(Length / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances[i] + LastDistance) / 2, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=-" + Convert.ToString(Math.Round(Math.Atan(Width / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances[i] + LastDistance) / 2, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                    LastDistance = FixedDistances[i];
                    PastDistance = Distance;
                }
            }
            if (DrawCrosshairDistShow == true && SightType == "Double")
            {
                double LastDistance = 0;
                double PastDistance = 0;
                for (int i = Convert.ToInt16(FixedDistance2 / 200) - 1; i < FixedDistances2.GetLength(0) && FixedDistances2[i] < Angle2; i += Convert.ToInt16(FixedDistance2 / 200))
                {
                    double Distance = ((i + 1) * 200);
                    double AdditionalDistance = (Distance + PastDistance) / 2;
                    if (AdditionalDistance >= FixedDistance2)
                    {
                        fcs_data += @"
circle{segment:p2=0.0,360.0;pos:p2=" + Convert.ToString(Math.Round(Math.Atan(Length / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances2[i] + LastDistance) / 2 - ScrollAngle, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=" + Convert.ToString(Math.Round(Math.Atan(Width / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances2[i] + LastDistance) / 2 - ScrollAngle, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=-" + Convert.ToString(Math.Round(Math.Atan(Length / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances2[i] + LastDistance) / 2 - ScrollAngle, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
circle{segment:p2=0.0,360.0;pos:p2=-" + Convert.ToString(Math.Round(Math.Atan(Width / AdditionalDistance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round((FixedDistances2[i] + LastDistance) / 2 - ScrollAngle, 2)) + @";diameter:r=0.1;size:r=" + Convert.ToString(Math.Round(1.5 * lineSizeMult * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                    LastDistance = FixedDistances2[i];
                    PastDistance = Distance;
                }
            }
            fcs_data += @"
}
drawTexts{";
            if (RangerFinderShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Rangefinder + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * 70.5 * RangerFinderPos[0] / 250 / 150, 2)) + @"," + Convert.ToString(Math.Round(-Size * 67 * RangerFinderPos[1] / 0.2 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (TargetLockShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Capture + ": " + ON + '\u0022' + @";pos:p2=-" + Convert.ToString(Math.Round(Size * 96.5 / 150, 2)) + @",-" + Convert.ToString(Math.Round(Size * 67 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (SightNameShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + SightName + '\u0022' + @";align:i=0;pos:p2=0,-" + Convert.ToString(Math.Round(Size * 83 / 150, 2)) + @";thousandth:b=yes;size:r=0.7}";
            }
            if (BalisticInfoShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + LangName + '\u0022' + @";align:i=0;pos:p2=-" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 59 / 150, 2)) + @";thousandth:b=yes;size:r=1}
text{text:t=" + '\u0022' + @"-----------------------------" + '\u0022' + @";align:i=0;pos:p2=-" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 62 / 150, 2)) + @";thousandth:b=yes;size:r=1;highlight:b=yes}";
            }
            if (DistanceShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Elevation + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * DistancePos[0] / 150, 2)) + @"," + Convert.ToString(Math.Round(-Size * (DistancePos[1] + 6) / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (TankSizesShow == true)
            {
                fcs_data += @"
text{text:t=" + '\u0022' + LengthString + @"=" + Convert.ToString(Length) + Meters + @"   " + WidthString + @"=" + Convert.ToString(Width) + Meters + @"   " + HeightString + @"=" + Convert.ToString(Height) + Meters + '\u0022' + @";align:i=0;pos:p2=0,-" + Convert.ToString(Math.Round(Size * 77 / 150, 2)) + @";thousandth:b=yes;size:r=0.7}";
            }
            if ((Type == "heat" || Type == "hesh" || Type == "he" || Type == "atgm" || Type == "aam" || Type == "sam") && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + ArmorPower + " " + Millimetrs + '\u0022' + @";pos:p2=-" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (LangRocketName != null && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + LangRocketName + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 59 / 150, 2)) + @";thousandth:b=yes;size:r=1}
text{text:t=" + '\u0022' + @"-----------------------------" + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 62 / 150, 2)) + @";thousandth:b=yes;size:r=1;highlight:b=yes}";
            }
            if (SightType == "Double" && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + LangName2 + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 59 / 150, 2)) + @";thousandth:b=yes;size:r=1}
text{text:t=" + '\u0022' + @"-----------------------------" + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Size * 75 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 62 / 150, 2)) + @";thousandth:b=yes;size:r=1;highlight:b=yes}";
            }
            if (SightType == "Double" && (Type2 == "heat" || Type2 == "hesh" || Type2 == "he") && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + ArmorPower2 + " " + Millimetrs + '\u0022' + @";align:i=1;pos:p2=" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (RocketArmorPower != 0 && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + RocketArmorPower + " " + Millimetrs + '\u0022' + @";align:i=1;pos:p2=" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            CurrentAngle = 0.0;
            if (DrawCrosshairDistShow == true && SightType != "Rocket")
            {
                int i;
                double Position = 0;
                double Align = -1;
                double PositionChange = -1;
                for (i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0) && FixedDistances[i] < Angle; i += Convert.ToInt16(FixedDistance / 200))
                {
                    if (FixedDistances[i] > FontSize / 4 && (i + 1) * 200 >= FixedDistance)
                    {
                        double Distance = (i + 1) * 2;
                        if (Distance >= 4)
                        {
                            Position = Math.Atan(Length / Distance / 100) * 1000 / 2 + 1 * Size / 150;
                            Position *= PositionChange;
                            PositionChange *= -1;
                            Align *= -1;
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(Distance) + '\u0022' + @";align:i=" + Convert.ToString(Align) + @";pos:p2=" + Convert.ToString(Math.Round(Position, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=0.7;highlight:b=yes}";
                        }
                    }
                }
            }
            if (SightType == "Double" && DrawCrosshairDistShow == true)
            {
                int i;
                double Position = 0;
                double Align = -1;
                double PositionChange = -1;
                for (i = Convert.ToInt16(FixedDistance2 / 200) - 1; i < FixedDistances2.GetLength(0) && FixedDistances2[i] < Angle2; i += Convert.ToInt16(FixedDistance2 / 200))
                {
                    if (FixedDistances2[i] > FontSize2 / 4 && (i + 1) * 200 >= FixedDistance2)
                    {
                        double Distance = (i + 1) * 2;
                        if (Distance >= 4)
                        {
                            Position = Math.Atan(Length / Distance / 100) * 1000 / 2 + 1 * Size / 150;
                            Position *= PositionChange;
                            PositionChange *= -1;
                            Align *= -1;
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(Distance) + '\u0022' + @";align:i=" + Convert.ToString(Align) + @";pos:p2=" + Convert.ToString(Math.Round(Position, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances2[i] - ScrollAngle, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=0.7;highlight:b=yes}";
                        }
                    }
                }
            }
            if (SightType != "Rocket")
            {
                int a = -5;
                while (a <= 5)
                {
                    if (a != 0)
                    {
                        double Position = CenterThousand2 * a;
                        fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(Math.Round(TargetSpeed * 3.6 * Math.Abs(Convert.ToDouble(a)) / 10)) + '\u0022' + @";align:i=0;pos:p2=" + Convert.ToString(Math.Round(Position, 2)) + @",-" + Convert.ToString(Math.Round(2 * Size / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=0.5;highlight:b=yes}";
                    }
                    a++;
                }
            }
            if (BallisticData[2, 0] != 0 && (BalisticInfoShow == true) && SightType != "Laser")
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData[2, 0]) + " " + Millimetrs + '\u0022' + @";pos:p2=-" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
            }
            if (BallisticData[2, 0] != 0 && (BalisticInfoShow == true) && SightType == "Laser")
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData[2, 0]) + " " + Millimetrs + '\u0022' + @";pos:p2=-" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";thousandth:b=yes;size:r=1}";
            }
            if (SightType == "Double" && BallisticData2[2, 0] != 0 && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData2[2, 0]) + " " + Millimetrs + '\u0022' + @";align:i=1;pos:p2=" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
            }
            if (BalisticInfoShow == true && SightType != "Laser")
            {
                fcs_data += @"
text{text:t=" + '\u0022' + "-----" + '\u0022' + @";align:i=1;pos:p2=-" + Convert.ToString(Math.Round(Size * 53 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
            }
            if ((SightType == "Double" || LangRocketName != null) && (BalisticInfoShow == true))
            {
                fcs_data += @"
text{text:t=" + '\u0022' + "-----" + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * 53 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * 65 / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
            }
            CurrentAngle = ScrollAngle;
            if (SightType != "Laser")
            {
                for (int i = 1; i < MaxLength; i++)
                {
                    double CurrentDistance = 0;
                    double CurrentDistance2 = 0;
                    if (i < BallisticData.GetLength(1) && BallisticData[0, i] < MaxDistance)
                    {
                        CurrentDistance = BallisticData[0, i];
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
                        if (BalisticInfoShow == true)
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData[1, i]) + Seconds + '\u0022' + @";align:i=1;pos:p2=-" + Convert.ToString(Math.Round(Size * 53 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * (65 - 53 * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
                        }
                        if (LangRocketName != null && (BalisticInfoShow == true))
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(Math.Round(CurrentDistance / RocketSpeed, 1)) + Seconds + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * 53 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * (65 + 53 * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
                        }
                        if (BallisticData[2, i] != 0 && (BalisticInfoShow == true))
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData[2, i]) + " " + Millimetrs + '\u0022' + @";pos:p2=-" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * (65 - 97 * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
                        }
                    }
                    if (i < BallisticData2.GetLength(1) && BallisticData2[0, i] < MaxDistance)
                    {
                        CurrentDistance2 = BallisticData2[0, i];
                        if (CurrentDistance2 < 100)
                        {
                            CurrentDistance2 = Math.Floor(CurrentDistance2 / 5) * 5;
                        }
                        if ((CurrentDistance2 >= 100) && (CurrentDistance2 < 400))
                        {
                            CurrentDistance2 = Math.Floor(CurrentDistance2 / 10) * 10;
                        }
                        if (CurrentDistance2 >= 400)
                        {
                            CurrentDistance2 = Math.Floor(CurrentDistance2 / 50) * 50;
                        }
                        if (SightType == "Double" && (BalisticInfoShow == true))
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData2[1, i]) + Seconds + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * 53 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * (65 + 53 * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
                        }
                        if (SightType == "Double" && BallisticData2[2, i] != 0 && (BalisticInfoShow == true))
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(BallisticData2[2, i]) + " " + Millimetrs + '\u0022' + @";align:i=1;pos:p2=" + Convert.ToString(Math.Round(Size * 97 / 150, 2)) + @"," + Convert.ToString(Math.Round(Size * (65 + 97 * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1}";
                        }
                    }
                    if (SightType == "Double")
                    {
                        if (CurrentDistance == 0 && CurrentDistance2 != 0)
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + ">" + MaxDistance + @" / " + Convert.ToString(CurrentDistance2) + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * DistancePos[0] / 150, 2)) + @",-" + Convert.ToString(Math.Round(Size * (DistancePos[1] - DistancePos[0] * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1;highlight:b=yes}";
                        }
                        else if (CurrentDistance != 0 && CurrentDistance2 == 0)
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(CurrentDistance) + " / >" + MaxDistance + '\u0022' + @"; pos:p2=" + Convert.ToString(Math.Round(Size * DistancePos[0] / 150, 2)) + @",-" + Convert.ToString(Math.Round(Size * (DistancePos[1] - DistancePos[0] * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1;highlight:b=yes}";
                        }
                        else if (CurrentDistance != 0 && CurrentDistance2 != 0)
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(CurrentDistance) + " / " + Convert.ToString(CurrentDistance2) + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * DistancePos[0] / 150, 2)) + @",-" + Convert.ToString(Math.Round(Size * (DistancePos[1] - DistancePos[0] * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1;highlight:b=yes}";
                        }
                    }
                    else
                    {
                        if (CurrentDistance != 0)
                        {
                            fcs_data += @"
text{text:t=" + '\u0022' + Convert.ToString(CurrentDistance) + '\u0022' + @";pos:p2=" + Convert.ToString(Math.Round(Size * DistancePos[0] / 150, 2)) + @",-" + Convert.ToString(Math.Round(Size * (DistancePos[1] - DistancePos[0] * CurrentAngle * ScrollingBug) / 150, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes;size:r=1;highlight:b=yes}";
                        }
                    }
                    CurrentAngle += ScrollAngle;
                }
            }
            if (SightType != "Rocket")
            {
                fcs_data += @"
}
drawLines{
line{line:p4=0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0,-" + Convert.ToString(Math.Round(CenterHalfThousand * 4, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterHalfThousand * 3, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0," + Convert.ToString(Math.Round(CenterHalfThousand * 4, 2)) + @",0," + Convert.ToString(Math.Round(CenterHalfThousand * 3, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(PreemptiveSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand2 * 2, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand2 * 3, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 2, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand2 * 2, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand2 * 3, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(PreemptiveSpeed * 2, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand2 * 2, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand2 * 3, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 3, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand2 * 2, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand2 * 3, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(PreemptiveSpeed * 3, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand2 * 4, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand2 * 5, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 4, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand2 * 4, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand2 * 5, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(PreemptiveSpeed * 4, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand2 * 4, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand2 * 5, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(PreemptiveSpeed * 5, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand2 * 4, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand2 * 5, 2)) + @",0;radialAngle:r=0;radialCenter:p2=0," + Convert.ToString(Radius) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(PreemptiveSpeed * 5, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
            }
            else
            {
                fcs_data += @"
}
drawLines{
line{line:p4=0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0,-" + Convert.ToString(Math.Round(CenterHalfThousand * 4, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterHalfThousand * 3, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=0," + Convert.ToString(Math.Round(CenterHalfThousand * 4, 2)) + @",0," + Convert.ToString(Math.Round(CenterHalfThousand * 3, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand * 2, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand * 3, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand * 2, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand * 3, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand * 4, 2)) + @",0," + Convert.ToString(Math.Round(CenterThousand * 5, 2)) + @",0;move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand * 4, 2)) + @",0,-" + Convert.ToString(Math.Round(CenterThousand * 5, 2)) + @",0;move:b=false;thousandth:b=yes}";
            }
            CurrentAngle = ScrollAngle;
            if (SightType == "Double")
            {
                fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(CenterThousandDouble, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @"," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousandDouble, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @",-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousandDouble * 2, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @"," + Convert.ToString(Math.Round(CenterThousandDouble * 3, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousandDouble * 2, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @",-" + Convert.ToString(Math.Round(CenterThousandDouble * 3, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(CenterThousandDouble * 4, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @"," + Convert.ToString(Math.Round(CenterThousandDouble * 5, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousandDouble * 4, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @",-" + Convert.ToString(Math.Round(CenterThousandDouble * 5, 2)) + @",-" + Convert.ToString(Math.Round(ScrollStep, 3)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
            }
            if (drawOuterLinesShow == true)
            {
                fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(Size / 15, 2)) + @"," + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @"," + Convert.ToString(Math.Round(Size / 3, 2)) + @"," + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Size / 15, 2)) + @"," + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @",-" + Convert.ToString(Math.Round(Size / 3, 2)) + @"," + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @";move:b=false;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Size / 3, 2)) + @",-" + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @"," + Convert.ToString(Math.Round(Size / 3, 2)) + @",-" + Convert.ToString(Math.Round(Size / 150 * 80, 2)) + @";move:b=false;thousandth:b=yes}";
            }
            CurrentAngle = 0.0;
            if (DrawCrosshairDistShow == true && SightType != "Rocket")
            {
                for (int i = Convert.ToInt16(FixedDistance / 200) - 1; i < FixedDistances.GetLength(0) && FixedDistances[i] < Angle; i += Convert.ToInt16(FixedDistance / 200))
                {
                    double Distance = ((i + 1) * 200);
                    if (Distance >= FixedDistance)
                    {
                        fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(Math.Atan(Length / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @"," + Convert.ToString(Math.Round(Math.Atan(Width / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Math.Atan(Length / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @",-" + Convert.ToString(Math.Round(Math.Atan(Width / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances[i], 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                }
            }
            if (DrawCrosshairDistShow == true && SightType == "Double")
            {
                for (int i = Convert.ToInt16(FixedDistance2 / 200) - 1; i < FixedDistances2.GetLength(0) && FixedDistances2[i] < Angle2; i += Convert.ToInt16(FixedDistance2 / 200))
                {
                    double Distance = ((i + 1) * 200);
                    if (Distance >= FixedDistance2)
                    {
                        fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(Math.Atan(Length / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances2[i] - ScrollAngle, 2)) + @"," + Convert.ToString(Math.Round(Math.Atan(Width / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances2[i] - ScrollAngle, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(Math.Atan(Length / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances2[i] - ScrollAngle, 2)) + @",-" + Convert.ToString(Math.Round(Math.Atan(Width / Distance) * 1000 / 2, 2)) + @"," + Convert.ToString(Math.Round(FixedDistances2[i] - ScrollAngle, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(ScrollAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                }
            }
            fcs_data += @"
line{line:p4=" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0," + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(CenterThousand, 2)) + @",0,-" + Convert.ToString(Math.Round(InnerDiameter / 2, 2)) + @",0;move:b=yes;thousandth:b=yes}";
            int NumLines = 0;
            if (SightType == "Double")
            {
                CurrentAngle = 2 * ScrollAngle;
                for (int i = 2; i < MaxLength && NumLines <= MaxLines; i++)
                {
                    double CurrentDistance = 0;
                    double CurrentDistance2 = 0;
                    double FrameHeight = 0;
                    double FrameWidth = 0;
                    if (i < BallisticData.GetLength(1) && BallisticData[0, i] < MaxDistance)
                    {
                        CurrentDistance = BallisticData[0, i];
                        double FrameLength = Math.Atan(Length / CurrentDistance) * 1000 / 2;
                        FrameHeight = Math.Atan(Height / CurrentDistance) * 1000 / 2;
                        FrameWidth = Math.Atan(Width / CurrentDistance) * 1000 / 2;
                        NumLines += 8;
                        fcs_data += @"
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @",-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round((FrameHeight + FrameLength * CurrentAngle * ScrollingBug) / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(-FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(-FrameHeight / 2 + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight / 2 - FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round((FrameHeight + FrameLength * CurrentAngle * ScrollingBug) / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                    if (i < BallisticData2.GetLength(1) && BallisticData2[0, i] < MaxDistance)
                    {
                        CurrentDistance2 = BallisticData2[0, i];
                        double FrameHeight2 = Math.Atan(Height / CurrentDistance2) * 1000 / 2;
                        double FrameLength2 = Math.Atan(Length / CurrentDistance2) * 1000 / 2;
                        double FrameWidth2 = Math.Atan(Width / CurrentDistance2) * 1000 / 2;
                        if (FrameHeight == 0 && FrameWidth == 0)
                        {
                            FrameHeight = FrameHeight2;
                            FrameWidth = FrameWidth2;
                        }
                        if (CurrentDistance2 >= CurrentDistance)
                        {
                            FrameWidth /= 2;
                            FrameHeight /= 2;
                        }
                        NumLines += 4;
                        fcs_data += @"
line{line:p4=-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight2, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight2, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength2, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @",-" + Convert.ToString(Math.Round(FrameLength2, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength2, 2)) + @"," + Convert.ToString(Math.Round(-FrameHeight / 2, 2)) + @"," + Convert.ToString(Math.Round(FrameLength2, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    }
                    CurrentAngle += ScrollAngle;
                }
            }
            else if (SightType == "Base" || SightType == "Rocket" || SightType == "Howitzer")
            {
                CurrentAngle = ScrollAngle;
                for (int i = 1; i < BallisticData.GetLength(1) && i < MaxLength && NumLines <= MaxLines && BallisticData[0, i] < MaxDistance; i++)
                {
                    double CurrentDistance = BallisticData[0, i];
                    double FrameHeight = Math.Atan(Height / CurrentDistance) * 1000 / 2;
                    double FrameLength = Math.Atan(Length / CurrentDistance) * 1000 / 2;
                    double FrameWidth = Math.Atan(Width / CurrentDistance) * 1000 / 2;
                    NumLines += 8;
                    fcs_data += @"
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @",-" + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameWidth, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameLength, 2)) + @",-" + Convert.ToString(Math.Round((FrameHeight + FrameLength * CurrentAngle * ScrollingBug) / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(-FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(-FrameHeight / 2 + FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight - FrameLength * CurrentAngle * ScrollingBug, 2)) + @",-" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight / 2 - FrameLength * CurrentAngle * ScrollingBug, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}
line{line:p4=" + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round(FrameHeight + FrameLength * CurrentAngle * ScrollingBug, 2)) + @"," + Convert.ToString(Math.Round(FrameLength, 2)) + @"," + Convert.ToString(Math.Round((FrameHeight + FrameLength * CurrentAngle * ScrollingBug) / 2, 2)) + @";radialAngle:r=" + Convert.ToString(Math.Round(CurrentAngle, 3)) + @";radialCenter:p2=" + Convert.ToString(Radius) + @",0;radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeed, 5)) + @";moveRadial:b=yes;thousandth:b=yes}";
                    CurrentAngle += ScrollAngle;
                }
            }
            else if (SightType == "Laser")
            {
                fcs_data += @"
line{line:p4=-" + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @",-" + Convert.ToString(Math.Round(Width, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @",-" + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=" + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @"," + Convert.ToString(Math.Round(Width, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @"," + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=-" + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @",-" + Convert.ToString(Math.Round(Width, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @"," + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=" + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @"," + Convert.ToString(Math.Round(Width, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @",-" + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=-" + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @",-" + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height / 2, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @",-" + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=" + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height, 2)) + @"," + Convert.ToString(Math.Round(Length, 2)) + @",-" + Convert.ToString(Math.Round(Height / 2, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @"," + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=-" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=-" + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @",-" + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height / 2, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @"," + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}
line{line:p4=" + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height, 2)) + @"," + Convert.ToString(Math.Round(Length, 2)) + @"," + Convert.ToString(Math.Round(Height / 2, 2)) + @";radialAngle:r=0;radialCenter:p2=" + Convert.ToString(RadiusY) + @",-" + Convert.ToString(RadiusX) + @";radialMoveSpeed:r=" + Convert.ToString(Math.Round(ScrollSpeedLaser, 5)) + @";moveRadial:b=yes;thousandth:b=yes;}";
            }
            fcs_data += @"
}";
            return fcs_data;
        }
    }
}
