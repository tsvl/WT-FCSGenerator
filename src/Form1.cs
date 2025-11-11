using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FCS
{

    public partial class Form1 : Form
    {

        DateTime StartTime = DateTime.Now;
        bool IsRuning = false;
        int SpeedNumbers = 0;
        double[] ProgressSpeed = new double[20];
        public Form1()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            checkedListBox2.ItemCheck += ItemCheck;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (IsRuning == true)
            {
                TimeSpan ts = DateTime.Now.Subtract(StartTime);
                string elapsedTime = String.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
                label18.Text = "Time: " + elapsedTime;
                double CurrentSpeed = (Convert.ToDouble(progressBar1.Value) / Convert.ToDouble(progressBar1.Maximum) / ts.TotalSeconds);
                if (SpeedNumbers < 20)
                {
                    SpeedNumbers += 1;
                }
                else { SpeedNumbers = 20; }
                double Temp2 = ProgressSpeed[0];
                for (int i = 0; i + 1 < ProgressSpeed.GetLength(0); i++)
                {
                    double Temp = ProgressSpeed[i + 1];
                    ProgressSpeed[i + 1] = Temp2;
                    Temp2 = Temp;
                }
                ProgressSpeed[0] = CurrentSpeed;
                double SumSpeed = 0;
                for (int i = 0; i < ProgressSpeed.GetLength(0); i++)
                {
                    SumSpeed += ProgressSpeed[i];
                }
                double remainingTime = (1 - Convert.ToDouble(progressBar1.Value) / Convert.ToDouble(progressBar1.Maximum)) / SumSpeed * SpeedNumbers;
                label19.Text = "Remaining Time: " + String.Format("{0:00}:{1:00}", Math.Floor(remainingTime / 60), Math.Floor(remainingTime % 60));
            }
        }
        void ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox lb = sender as CheckedListBox;
            if (e.Index == 0)
            {
                bool flag = e.NewValue == CheckState.Checked;
                for (int i = 1; i < lb.Items.Count; i++)
                {
                    lb.SetItemChecked(i, flag);
                }
            }
        }
        private bool CanUseDoubleShell(
            string BulletName1,
            string BulletName2,
            string Type1,
            string Type2,
            double BallisticCaliber1,
            double BallisticCaliber2
            )
        {
            if (BulletName1 != BulletName2 && Type1 != Type2)
            {
                int Level1 = 0;
                int Level2 = 0;
                if (Type1 == "apds_fs" || Type1 == "hesh" || Type1 == "heat_fs" || Type1 == "apds" || (Type1 == "he_frag_fs" && BallisticCaliber1 >= 0.12))
                {
                    Level1 = 1;
                }
                else if (Type1 == "aphe" || Type1 == "aphebc" || Type1 == "heat_grenade" || (Type1 == "he" && BallisticCaliber1 >= 0.12))
                {
                    Level1 = 2;
                }
                else if (Type1 == "ap" || Type1 == "apc" || Type1 == "apcbc" || Type1 == "apbc")
                {
                    Level1 = 3;
                }
                else if (Type1 == "apcr" || Type1 == "heat")
                {
                    Level1 = 4;
                }
                else
                {
                    Level1 = 5;
                }

                if (Type2 == "apds_fs" || Type2 == "hesh" || Type2 == "heat_fs" || Type2 == "apds" || (Type2 == "he_frag_fs" && BallisticCaliber2 >= 0.12))
                {
                    Level2 = 1;
                }
                else if (Type2 == "aphe" || Type2 == "aphebc" || Type2 == "heat_grenade" || (Type2 == "he" && BallisticCaliber2 >= 0.12))
                {
                    Level2 = 2;
                }
                else if (Type2 == "ap" || Type2 == "apc" || Type2 == "apcbc" || Type2 == "apbc")
                {
                    Level2 = 3;
                }
                else if (Type2 == "apcr" || Type2 == "heat")
                {
                    Level2 = 4;
                }
                else
                {
                    Level2 = 5;
                }
                if (Level1 <= Level2)
                {
                    if (
                        (Type1 == "aphe" && Type2 == "aphebc") ||
                        (Type1 == "ap" && Type2 == "apc") ||
                        (Type1 == "ap" && Type2 == "apcbc") ||
                        (Type1 == "ap" && Type2 == "apbc") ||
                        (Type1 == "apc" && Type2 == "apcbc") ||
                        (Type1 == "apc" && Type2 == "apbc") ||
                        (Type1 == "apcbc" && Type2 == "apbc") ||

                        (Type1 == "hesh" && Type2 == "he" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat_fs" && Type2 == "he" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat_grenade" && Type2 == "he" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat" && Type2 == "he" && BallisticCaliber2 < 0.12) ||

                        (Type1 == "hesh" && Type2 == "he_frag_fs" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat_fs" && Type2 == "he_frag_fs" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat_grenade" && Type2 == "he_frag_fs" && BallisticCaliber2 < 0.12) ||
                        (Type1 == "heat" && Type2 == "he_frag_fs" && BallisticCaliber2 < 0.12) ||

                        (Type2 == "aphe" && Type1 == "aphebc") ||
                        (Type2 == "ap" && Type1 == "apc") ||
                        (Type2 == "ap" && Type1 == "apcbc") ||
                        (Type2 == "ap" && Type1 == "apbc") ||
                        (Type2 == "apc" && Type1 == "apcbc") ||
                        (Type2 == "apc" && Type1 == "apbc") ||
                        (Type2 == "apcbc" && Type1 == "apbc") ||

                        (Type2 == "hesh" && Type1 == "he" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat_fs" && Type1 == "he" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat_grenade" && Type1 == "he" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat" && Type1 == "he" && BallisticCaliber1 < 0.12) ||

                        (Type2 == "hesh" && Type1 == "he_frag_fs" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat_fs" && Type1 == "he_frag_fs" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat_grenade" && Type1 == "he_frag_fs" && BallisticCaliber1 < 0.12) ||
                        (Type2 == "heat" && Type1 == "he_frag_fs" && BallisticCaliber1 < 0.12)
                        )
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private double HePenetration(double ExplosiveMass, string ExplosiveType)
        {
            double HePenetration = 0;
            string[,] ExplosiveTypes = {
                    {"comp_a", "1.44"},
                    {"octol", "1.59"},
                    {"ocfol", "1.59"},
                    {"clx_663", "1.59"},
                    {"hmx", "1.59"},
                    {"lx14", "1.41"},
                    {"comp_h6", "1.35"},
                    {"oshiyaku", "1.0"},
                    {"rdx_petn", "1.3"},
                    {"rdx_tnt", "1.28"},
                    {"rdx_pwx", "1.28"},
                    {"rdx_aluminium", "1.28"},
                    {"smoke_composition", "1.0"},
                    {"s_r_379", "0.002"},
                    {"sw_39a", "1.28"},
                    {"tp_97", "1.28"},
                    {"tp_91", "1.05"},
                    {"tp_88", "1.3"},
                    {"rdx", "1.6"},
                    {"h761", "1.6"},
                    {"ha_41", "1.6"},
                    {"tetryl_bpp", "1.45"},
                    {"h5_fp02", "1.7"},
                    {"tnt_a_ix_2", "1.54"},
                    {"ph_salz_h10", "1.43"},
                    {"fp10_np10", "1.28"},
                    {"fp02_np10", "1.28"},
                    {"lyddit", "1.1"},
                    {"shimose", "1.1"},
                    {"melinite", "1.1"},
                    {"torpex", "1.6"},
                    {"trialen_105", "1.6"},
                    {"hbx", "1.6"},
                    {"hta", "1.2"},
                    {"hta_petn", "1.2"},
                    {"napalm", "0.002"},
                    {"oktogel", "0.002"},
                    {"sks", "0.002"},
                    {"fp_02", "0.002"},
                    {"octogel", "0.002"},
                    {"aerea_64057", "0.002"},
                    {"napalm_gel", "0.002"},
                    {"pentolite", "1.21"},
                    {"comp_b", "1.31"},
                    {"amatol_tnt", "1.0"},
                    {"amatol", "1.0"},
                    {"minol", "1.15"},
                    {"exp_d_tetryl", "0.98"},
                    {"exp_d", "0.98"},
                    {"fp60_40", "1.0"},
                    {"fp02", "1.0"},
                    {"fp15", "1.0"},
                    {"fp02_tetraxhlornaphtalin", "1.0"},
                    {"tetryl", "1.45"},
                    {"ph_salz", "1.43"},
                    {"a_ix_2", "1.54"},
                    {"a_ix_1", "1.25"},
                    {"h10", "1.7"},
                    {"h5", "1.7"},
                    {"np_10", "1.7"},
                    {"np_15", "1.7"},
                    {"petn", "1.7"},
                    {"tnt", "1.0"},
                    {"explosive_d", "0.98"},
                    {"picric_acid_tnt", "0.98"},
                    {"hexal", "1.7"},
                    {"picric_acid", "1.1"},
                    {"shellite", "0.94"},
                    {"mn_f_dn", "0.9"},
                    {"dd", "0.94"},
                    {"mmn", "0.94"},
                    {"tga_16", "1.5"},
                    {"tgaf_5", "1.6"},
                    {"tritonal", "1.18"},
                    {"liEx_S13df", "1.008"},
                    {"pbxn_3", "1.28"},
                    {"pbxn_4", "1.28"},
                    {"pbxn_5", "1.28"},
                    {"pbxn_102", "1.28"},
                    {"pbxn_110", "1.28"},
                    {"nitrolit", "1.01"},
                    {"tg_40", "1.28"},
                    {"hlp_n", "10.23"},
                    {"okfol_20", "1.325"}
                };
            double RealExplosiveMass = 0;
            for (int i = 0; i < ExplosiveTypes.GetLength(0); i++)
            {
                if (ExplosiveType == ExplosiveTypes[i, 0])
                {
                    RealExplosiveMass = ExplosiveMass * Convert.ToDouble(ExplosiveTypes[i, 1]);
                }
            }
            double[,] MassToPenetration = {
                    {0.005, 2.0},
                    {0.1, 4.0},
                    {0.2, 5.0},
                    {2.0, 25.0},
                    {3.0, 35.0},
                    {5.0, 40.0},
                    {6.0, 50.0},
                    {8.0, 60.0},
                    {9.0, 61.0},
                    {10.0, 62.0},
                    {11.0, 63.0},
                    {25.0, 65.0},
                    {40.0, 70.0},
                    {120.0, 82.0},
                    {300.0, 96.0},
                    {500.0, 111.0},
                    {700.0, 127.0},
                    {1500.0, 191.0},
                    {3500.0, 320.0},
                    {5000.0, 350.0},
                    {6000.0, 365.0}
                };
            for (int i = 0; i + 1 < MassToPenetration.GetLength(0) && HePenetration == 0; i++)
            {
                if (RealExplosiveMass < MassToPenetration[0, 0])
                {
                    HePenetration = 2;
                }
                if (RealExplosiveMass >= MassToPenetration[i, 0] && RealExplosiveMass < MassToPenetration[i + 1, 0])
                {
                    HePenetration = Math.Round(MassToPenetration[i, 1] + ((MassToPenetration[i + 1, 1] - MassToPenetration[i, 1]) / (MassToPenetration[i + 1, 0] - MassToPenetration[i, 0]) * (RealExplosiveMass - MassToPenetration[i, 0])), MidpointRounding.AwayFromZero);
                }
                if (RealExplosiveMass >= MassToPenetration[MassToPenetration.GetLength(0) - 1, 0])
                {
                    HePenetration = 365;
                }
            }
            return HePenetration;
        }
        private string Ballistic(
            double Sensivity,
            string Type,
            double BulletMass,
            double Speed,
            double BallisticCaliber,
            double Cx,
            double ExplosiveMass,
            double DamageMass,
            double DamageCaliber,
            double demarrePenetrationK,
            double demarreSpeedPow,
            double demarreMassPow,
            double demarreCaliberPow,
            double[,] ArmorPowerArray
            )
        {
            if (demarrePenetrationK == 0)
            {
                demarrePenetrationK = 0.9;
            }
            if (demarreSpeedPow == 0)
            {
                demarreSpeedPow = 1.43;
            }
            if (demarreMassPow == 0)
            {
                demarreMassPow = 0.71;
            }
            if (demarreCaliberPow == 0)
            {
                demarreCaliberPow = 1.07;
            }
            double ScrollStep = 2.8 * Math.Pow(Sensivity, 2);
            double[,] BallisticData = new double[3, Convert.ToInt32(Math.Floor((Math.PI / 180) * 60 * 1000 / ScrollStep))];
            for (int i = 0; i < BallisticData.GetLength(1); i++)
            {
                BallisticData[0, i] = 0.00001;
            }
            double LastDistance = 0;
            for (int i = 0; i < BallisticData.GetLength(1) && LastDistance < 4500; i++)
            {
                double Angle = ScrollStep * (i) / 1000;
                double g = 9.80665, dt = 0.01, p = 101325, T = 15;
                double x = 0, y = 0, t = 0;
                double x0 = 0, y0 = 0;
                double vx = Speed * Math.Cos(Angle), vy = Speed * Math.Sin(Angle);
                while (y >= 0)
                {
                    double ro = p * 0.0289652 / 8.31446 / (T + 273.15) * Math.Pow((1 - 0.0065 * y / 288.15), (g * 0.0289652 / 8.31446 / 0.0065 - 1));
                    double F = Cx * ro * (Math.Pow(vx, 2) + Math.Pow(vy, 2)) / 2 * Math.Pow(BallisticCaliber, 2) / 4 * Math.PI;
                    vx += (-F / BulletMass * Math.Cos(Math.Atan(vy / vx))) * dt;
                    vy += (-g - F / BulletMass * Math.Sin(Math.Atan(vy / vx))) * dt;
                    t += dt; x0 = x; y0 = y;
                    x += vx * dt; y += vy * dt;
                }
                BallisticData[0, i] = x0 + (x - x0) / (y - y0) * -y0;
                LastDistance = BallisticData[0, i];
                BallisticData[1, i] = Math.Round(t, 1, MidpointRounding.AwayFromZero);
                double Penetration = 0;
                if (Type == "i" || Type == "t" || Type == "ac" || Type == "aphe" || Type == "aphebc" || Type == "ap" || Type == "sap" || Type == "sapi" || Type == "apc" || Type == "apbc" || Type == "apcbc" || Type == "sapcbc")
                {
                    Penetration = demarrePenetrationK * Math.Pow(Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2)) / 1900, demarreSpeedPow) * Math.Pow(BulletMass, demarreMassPow) / Math.Pow(BallisticCaliber * 10, demarreCaliberPow) * 100;
                    if (Type == "aphe" || Type == "aphebc" || Type == "ac" || Type == "sapcbc" || Type == "sap" || Type == "sapi")
                    {
                        double k = ExplosiveMass / BulletMass;
                        double[,] PenByExpl = {
                                                {0.0065, 1.0},
                                                {0.016, 0.93},
                                                {0.02, 0.9},
                                                {0.03, 0.85},
                                                {0.04, 0.75}
                                            };
                        double PenK = 0;
                        for (int a = 0; a + 1 < PenByExpl.GetLength(0) && PenK == 0; a++)
                        {
                            if (k < PenByExpl[0, 0])
                            {
                                PenK = 1.0;
                            }
                            if (k >= PenByExpl[a, 0] && k < PenByExpl[a + 1, 0])
                            {
                                PenK = PenByExpl[a, 1] + ((PenByExpl[a + 1, 1] - PenByExpl[a, 1]) / (PenByExpl[a + 1, 0] - PenByExpl[a, 0]) * (k - PenByExpl[a, 0]));
                            }
                            if (k >= PenByExpl[PenByExpl.GetLength(0) - 1, 0])
                            {
                                PenK = 0.75;
                            }
                        }
                        Penetration *= PenK;
                    }
                    Penetration = Math.Round(Penetration, MidpointRounding.AwayFromZero);
                }
                if (Type == "apcr" || Type == "apds")
                {
                    double k = DamageMass / BulletMass;
                    double[,] PenBySubcaliber = {
                                            {0.0, 0.25},
                                            {0.15, 0.4},
                                            {0.3, 0.5},
                                            {0.4, 0.75}
                                        };
                    double SubcaliberK = 0;
                    for (int a = 0; a + 1 < PenBySubcaliber.GetLength(0) && SubcaliberK == 0; a++)
                    {
                        if (k < PenBySubcaliber[0, 0])
                        {
                            SubcaliberK = 0.25;
                        }
                        if (k >= PenBySubcaliber[a, 0] && k < PenBySubcaliber[a + 1, 0])
                        {
                            SubcaliberK = PenBySubcaliber[a, 1] + ((PenBySubcaliber[a + 1, 1] - PenBySubcaliber[a, 1]) / (PenBySubcaliber[a + 1, 0] - PenBySubcaliber[a, 0]) * (k - PenBySubcaliber[a, 0]));
                        }
                        if (k >= PenBySubcaliber[PenBySubcaliber.GetLength(0) - 1, 0])
                        {
                            SubcaliberK = 0.75;
                        }
                    }
                    Penetration = Math.Round(demarrePenetrationK * Math.Pow(Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2)) / 1900, demarreSpeedPow) * Math.Pow((BulletMass - DamageMass) * SubcaliberK + DamageMass, demarreMassPow) / Math.Pow(DamageCaliber * 10, demarreCaliberPow) * 100, MidpointRounding.AwayFromZero);
                }
                if (Type == "apds_fs")
                {
                    for (int a = 0; a + 1 < ArmorPowerArray.GetLength(1); a++)
                    {
                        if (BallisticData[0, i] >= ArmorPowerArray[0, a] && BallisticData[0, i] < ArmorPowerArray[0, a + 1])
                        {
                            Penetration = Math.Round(ArmorPowerArray[1, a] + (BallisticData[0, i] - ArmorPowerArray[0, a]) / (ArmorPowerArray[0, a + 1] - ArmorPowerArray[0, a]) * (ArmorPowerArray[1, a + 1] - ArmorPowerArray[1, a]), MidpointRounding.AwayFromZero);
                        }
                    }
                }
                BallisticData[2, i] = Penetration;
                Application.DoEvents();
            }
            string BallisticStrings = null;
            for (int i = 0; BallisticData[0, i + 1] >= BallisticData[0, i] && BallisticData[0, i + 1] != 0.00001; i++)
            {
                BallisticStrings += String.Format("{0:f3}", BallisticData[0, i]) + "\t" + BallisticData[1, i] + "\t" + BallisticData[2, i] + "\n";
            }
            return BallisticStrings;
        }

        private void TextBox1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void TextBox3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void TextBox4_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void TextBox2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string Dataminepath = textBox1.Text;
            string[] file_list = Directory.GetFiles(Dataminepath + "\\aces.vromfs.bin_u\\gamedata\\units\\tankmodels", "*.blkx");
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = file_list.Length;
            progressBar1.Step = 1;
            StartTime = DateTime.Now;
            IsRuning = true;
            foreach (string file in file_list)
            {
                label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                label1.Refresh();
                progressBar1.PerformStep();
                string TankData = null;
                string WeaponPath = null;
                string RocketPath = null;
                string RocketPath2 = null;
                string ZoomOut = null;
                string ZoomIn = null;
                string ZoomOut2 = null;
                string ZoomIn2 = null;
                bool HasLaser = false;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                {
                    TankData = sr.ReadToEnd();
                }
                StringReader reader = new StringReader(TankData);
                string line = String.Empty;
                string TankName = Path.GetFileNameWithoutExtension(file);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("groundModels_weapons") && (WeaponPath == null))
                    {
                        WeaponPath = line.Split('\"')[3];
                        WeaponPath += "x";
                    }
                    if (line.Contains("\"triggerGroup\": \"special\"") && (RocketPath == null))
                    {
                        line = reader.ReadLine();
                        RocketPath = line.Split('\"')[3];
                        RocketPath += "x";
                    }
                    if (line.Contains("\"triggerGroup\": \"special\"") && (RocketPath != null) && (RocketPath2 == null))
                    {
                        line = reader.ReadLine();
                        RocketPath2 = line.Split('\"')[3];
                        RocketPath2 += "x";
                        if (RocketPath == RocketPath2)
                        {
                            RocketPath2 = null;
                        }
                    }
                    if (line.Contains("\"cockpit\":"))
                    {
                        while ((ZoomOut == null) || (ZoomIn == null))
                        {
                            line = reader.ReadLine();
                            if (line.Contains("zoomOutFov"))
                            {
                                string zoomData = line.Split(':')[1];
                                if (zoomData.Contains("["))
                                {
                                    // Handle array case - get first value
                                    line = reader.ReadLine();
                                    while (line != null && !line.Trim().Any(char.IsDigit))
                                    {
                                        line = reader.ReadLine();
                                    }
                                    if (line != null)
                                    {
                                        ZoomOut = line.Trim().TrimEnd(',');
                                    }
                                }
                                else
                                {
                                    // Handle single value case
                                    ZoomOut = zoomData;
                                    ZoomOut = ZoomOut.Replace(" ", "");
                                    ZoomOut = ZoomOut.Replace(",", "");
                                }
                            }
                            if (line.Contains("zoomInFov"))
                            {
                                string zoomData = line.Split(':')[1];
                                if (zoomData.Contains("["))
                                {
                                    // Handle array case - get first value
                                    line = reader.ReadLine();
                                    while (line != null && !line.Trim().Any(char.IsDigit))
                                    {
                                        line = reader.ReadLine();
                                    }
                                    if (line != null)
                                    {
                                        ZoomIn = line.Trim().TrimEnd(',');
                                    }
                                }
                                else
                                {
                                    // Handle single value case
                                    ZoomIn = zoomData;
                                    ZoomIn = ZoomIn.Replace(" ", "");
                                    ZoomIn = ZoomIn.Replace(",", "");
                                }
                            }
                        }
                    }
                    if (line.Contains("\"cockpit\":"))
                    {
                        while ((ZoomOut2 == null) || (ZoomIn2 == null))
                        {
                            line = reader.ReadLine();
                            if (line.Contains("zoomOutFov"))
                            {
                                string zoomData = line.Split(':')[1];
                                if (zoomData.Contains("["))
                                {
                                    // Handle array case - get first value
                                    line = reader.ReadLine();
                                    while (line != null && !line.Trim().Any(char.IsDigit))
                                    {
                                        line = reader.ReadLine();
                                    }
                                    if (line != null)
                                    {
                                        ZoomOut2 = line.Trim().TrimEnd(',');
                                    }
                                }
                                else
                                {
                                    // Handle single value case
                                    ZoomOut2 = zoomData;
                                    ZoomOut2 = ZoomOut2.Replace(" ", "");
                                    ZoomOut2 = ZoomOut2.Replace(",", "");
                                }
                            }
                            if (line.Contains("zoomInFov"))
                            {
                                string zoomData = line.Split(':')[1];
                                if (zoomData.Contains("["))
                                {
                                    // Handle array case - get first value
                                    line = reader.ReadLine();
                                    while (line != null && !line.Trim().Any(char.IsDigit))
                                    {
                                        line = reader.ReadLine();
                                    }
                                    if (line != null)
                                    {
                                        ZoomIn2 = line.Trim().TrimEnd(',');
                                    }
                                }
                                else
                                {
                                    // Handle single value case
                                    ZoomIn2 = zoomData;
                                    ZoomIn2 = ZoomIn2.Replace(" ", "");
                                    ZoomIn2 = ZoomIn2.Replace(",", "");
                                }
                            }
                        }
                    }
                    if (line.Contains("laser"))
                    {
                        HasLaser = true;
                    }
                }
                string BulletInfo = null;
                string BulletName = null;
                if (WeaponPath != null)
                {
                    string[] lines = File.ReadAllLines(Dataminepath + "\\aces.vromfs.bin_u\\" + WeaponPath);
                    string data = null;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        data = lines[i];
                        BulletName = null;
                        string Mass = null;
                        string Caliber = null;
                        string Speed = null;
                        string DamageMass = null;
                        string DamageCaliber = null;
                        string ExplosiveMass = null;
                        string BulletType = null;
                        string Cx = null;
                        string BulletTypeName = null;
                        string BulletTypeShort = null;
                        string ExplosiveType = null;
                        string ArmorPower = null;
                        string ArmorPower0m = null;
                        string ArmorPower100m = null;
                        string ArmorPower500m = null;
                        string ArmorPower1000m = null;
                        string ArmorPower1500m = null;
                        string ArmorPower2000m = null;
                        string ArmorPower2500m = null;
                        string ArmorPower3000m = null;
                        string ArmorPower3500m = null;
                        string ArmorPower4000m = null;
                        string ArmorPower4500m = null;
                        string ArmorPower10000m = null;
                        string demarrePenetrationK = null;
                        string demarreSpeedPow = null;
                        string demarreMassPow = null;
                        string demarreCaliberPow = null;
                        if ((data.Contains("\"bullet\":")) || (data.Contains("\"rocket\":")))
                        {
                            bool HasModule = true;
                            if (i > 0 && lines[i - 1].Contains("{"))
                            {
                                HasModule = false;
                                // Search upwards for a line with a quote (and colon)
                                int nameLine = i - 1;
                                while (nameLine >= 0 && (!lines[nameLine].Contains("\"") || !lines[nameLine].Contains(":")))
                                {
                                    nameLine--;
                                }

                                if (nameLine >= 0)
                                {
                                    BulletName = lines[nameLine].Split('\"')[1];
                                    if (TankData.Contains(BulletName))
                                    {
                                        HasModule = true;
                                    }
                                    BulletName = BulletName.Replace("_cn_", "_");
                                    BulletName = BulletName.Replace("_fr_", "_");
                                    BulletName = BulletName.Replace("_germ_", "_");
                                    BulletName = BulletName.Replace("_il_", "_");
                                    BulletName = BulletName.Replace("_it_", "_");
                                    BulletName = BulletName.Replace("_jp_", "_");
                                    BulletName = BulletName.Replace("_sw_", "_");
                                    BulletName = BulletName.Replace("_uk_", "_");
                                    BulletName = BulletName.Replace("_us_", "_");
                                    BulletName = BulletName.Replace("_ussr_", "_");
                                    if (TankData.Contains(BulletName))
                                    {
                                        HasModule = true;
                                    }
                                    BulletName = null;
                                }
                            }
                            if (HasModule == true)
                            {
                                int Bracket = 1;
                                while (Bracket > 0 && i + 1 < lines.Length)
                                {
                                    i++;
                                    data = lines[i];
                                    if (data.Contains("\"sabot\":"))
                                    {
                                        int Bracket2 = 1;
                                        while (Bracket2 > 0 && i + 1 < lines.Length)
                                        {
                                            i++;
                                            data = lines[i];
                                            if (data.Contains("{"))
                                            {
                                                Bracket2++;
                                            }
                                            if (data.Contains("}"))
                                            {
                                                Bracket2--;
                                            }
                                            if (data.Contains("["))
                                            {
                                                Bracket2++;
                                            }
                                            if (data.Contains("]"))
                                            {
                                                Bracket2--;
                                            }
                                        }
                                        i++;
                                        data = lines[i];
                                    }
                                    if (data.Contains("{") && !lines[i - 1].Contains("    \"bullet\":"))
                                    {
                                        Bracket++;
                                    }
                                    if (data.Contains("}"))
                                    {
                                        Bracket--;
                                    }
                                    if (data.Contains("["))
                                    {
                                        Bracket++;
                                    }
                                    if (data.Contains("]"))
                                    {
                                        Bracket--;
                                    }
                                    if (data.Contains("\"bulletName\""))
                                    {
                                        if (data.Contains("["))
                                        {
                                            while (true)
                                            {
                                                i++;
                                                if (i >= lines.Length)
                                                {
                                                    break;
                                                }

                                                var nextLine = lines[i].Trim();
                                                if (nextLine.StartsWith("\""))
                                                {
                                                    BulletName = nextLine.Split('\"')[1].TrimEnd(',');
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            BulletName = data.Split('\"')[3];
                                        }
                                    }
                                    if (data.Contains("\"mass\""))
                                    {
                                        Mass = data.Split('\"')[2];
                                        Mass = Mass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"explosiveMass\""))
                                    {
                                        ExplosiveMass = data.Split('\"')[2];
                                        ExplosiveMass = ExplosiveMass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"caliber\": 0."))
                                    {
                                        Caliber = data.Split('\"')[2];
                                        Caliber = Caliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"speed\""))
                                    {
                                        Speed = data.Split('\"')[2];
                                        Speed = Speed.Split(' ')[1];
                                    }
                                    if (data.Contains("\"damageMass\""))
                                    {
                                        DamageMass = data.Split('\"')[2];
                                        DamageMass = DamageMass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"damageCaliber\": 0."))
                                    {
                                        DamageCaliber = data.Split('\"')[2];
                                        DamageCaliber = DamageCaliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"ballisticCaliber\""))
                                    {
                                        Caliber = data.Split('\"')[2];
                                        Caliber = Caliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"Cx\""))
                                    {
                                        string cxData = data.Split('\"')[2];
                                        if (cxData.Contains("["))
                                        {
                                            // Handle array case - collect all values and average them
                                            List<double> cxValues = new List<double>();
                                            string arrayLine = cxData;
                                            // Keep reading lines until we find the closing bracket
                                            while (!arrayLine.Contains("]") && i + 1 < lines.Length)
                                            {
                                                // Extract numbers from current line
                                                string[] parts = arrayLine.Split(new char[] { ',', '[', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                                foreach (string part in parts)
                                                {
                                                    if (double.TryParse(part, out double value))
                                                    {
                                                        cxValues.Add(value);
                                                    }
                                                }
                                                i++;
                                                arrayLine = lines[i];
                                            }
                                            // Process the final line with closing bracket
                                            string[] finalParts = arrayLine.Split(new char[] { ',', ']', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                            foreach (string part in finalParts)
                                            {
                                                if (double.TryParse(part, out double value))
                                                {
                                                    cxValues.Add(value);
                                                }
                                            }        // Calculate average
                                            if (cxValues.Count > 0)
                                            {
                                                double average = cxValues.Average();
                                                Cx = Math.Round(average, 4).ToString();
                                            }
                                            else
                                            {
                                                Cx = "0.38"; // Default fallback
                                            }
                                        }
                                        else
                                        {
                                            // Handle single value case as before
                                            Cx = cxData.Split(' ')[1];
                                        }
                                    }
                                    if (data.Contains("\"bulletType\""))
                                    {
                                        BulletType = data.Split('\"')[3];
                                        BulletTypeName = data.Split('\"')[3] + "/name/short";
                                        BulletTypeShort = BulletType.Split('_')[0];
                                    }
                                    if (data.Contains("\"endSpeed\""))
                                    {
                                        Speed = data.Split('\"')[2];
                                        Speed = Speed.Split(' ')[1];
                                    }
                                    if (data.Contains("\"explosiveType\""))
                                    {
                                        ExplosiveType = data.Split('\"')[3];
                                    }
                                    if (data.Contains("\"armorPower\""))
                                    {
                                        ArmorPower = data.Split('\"')[2];
                                        ArmorPower = ArmorPower.Split(' ')[1];
                                    }
                                    if (data.Contains("\"ArmorPower0m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower0m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower100m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower100m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower500m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower500m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower1000m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower1000m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower1500m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower1500m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower2000m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower2000m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower2500m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower2500m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower3000m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower3000m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower3500m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower3500m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower4000m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower4000m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower4500m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower4500m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"ArmorPower10000m\"") && BulletTypeShort == "apds")
                                    {
                                        data = lines[i + 1];
                                        ArmorPower10000m = data.Replace(" ", "");
                                    }
                                    if (data.Contains("\"demarrePenetrationK\""))
                                    {
                                        demarrePenetrationK = data.Split('\"')[2];
                                        demarrePenetrationK = demarrePenetrationK.Split(' ')[1];
                                    }
                                    if (data.Contains("\"demarreSpeedPow\""))
                                    {
                                        demarreSpeedPow = data.Split('\"')[2];
                                        demarreSpeedPow = demarreSpeedPow.Split(' ')[1];
                                    }
                                    if (data.Contains("\"demarreMassPow\""))
                                    {
                                        demarreMassPow = data.Split('\"')[2];
                                        demarreMassPow = demarreMassPow.Split(' ')[1];
                                    }
                                    if (data.Contains("\"demarreCaliberPow\""))
                                    {
                                        demarreCaliberPow = data.Split('\"')[2];
                                        demarreCaliberPow = demarreCaliberPow.Split(' ')[1];
                                    }
                                }
                                if (Cx == null)
                                {
                                    Cx = "0.38";
                                }
                                if (BulletName == null)
                                {
                                    BulletName = BulletTypeName;
                                }
                                BulletInfo += "\n\nName:" + BulletName + "\nType:" + BulletType + "\nBulletMass:" + Mass + "\nBallisticCaliber:" + Caliber + "\nSpeed:" + Speed + "\nCx:" + Cx;
                                if (ExplosiveMass != null)
                                {
                                    BulletInfo += "\nExplosiveMass:" + ExplosiveMass;
                                }
                                if (ExplosiveType != null)
                                {
                                    BulletInfo += "\nExplosiveType:" + ExplosiveType;
                                }
                                if (DamageMass != null)
                                {
                                    BulletInfo += "\nDamageMass:" + DamageMass;
                                }
                                if (DamageCaliber != null)
                                {
                                    BulletInfo += "\nDamageCaliber:" + DamageCaliber;
                                }
                                if (demarrePenetrationK != null)
                                {
                                    BulletInfo += "\ndemarrePenetrationK:" + demarrePenetrationK;
                                }
                                if (demarreSpeedPow != null)
                                {
                                    BulletInfo += "\ndemarreSpeedPow:" + demarreSpeedPow;
                                }
                                if (demarreMassPow != null)
                                {
                                    BulletInfo += "\ndemarreMassPow:" + demarreMassPow;
                                }
                                if (demarreCaliberPow != null)
                                {
                                    BulletInfo += "\ndemarreCaliberPow:" + demarreCaliberPow;
                                }
                                if (ArmorPower != null)
                                {
                                    BulletInfo += "\nArmorPower:" + ArmorPower;
                                }
                                if (ArmorPower0m != null)
                                {
                                    BulletInfo += "\nAPDS0:" + ArmorPower0m;
                                }
                                if (ArmorPower100m != null)
                                {
                                    BulletInfo += "\nAPDS100:" + ArmorPower100m;
                                }
                                if (ArmorPower500m != null)
                                {
                                    BulletInfo += "\nAPDS500:" + ArmorPower500m;
                                }
                                if (ArmorPower1000m != null)
                                {
                                    BulletInfo += "\nAPDS1000:" + ArmorPower1000m;
                                }
                                if (ArmorPower1500m != null)
                                {
                                    BulletInfo += "\nAPDS1500:" + ArmorPower1500m;
                                }
                                if (ArmorPower2000m != null)
                                {
                                    BulletInfo += "\nAPDS2000:" + ArmorPower2000m;
                                }
                                if (ArmorPower2500m != null)
                                {
                                    BulletInfo += "\nAPDS2500:" + ArmorPower2500m;
                                }
                                if (ArmorPower3000m != null)
                                {
                                    BulletInfo += "\nAPDS3000:" + ArmorPower3000m;
                                }
                                if (ArmorPower3500m != null)
                                {
                                    BulletInfo += "\nAPDS3500:" + ArmorPower3500m;
                                }
                                if (ArmorPower4000m != null)
                                {
                                    BulletInfo += "\nAPDS4000:" + ArmorPower4000m;
                                }
                                if (ArmorPower4500m != null)
                                {
                                    BulletInfo += "\nAPDS4500:" + ArmorPower4500m;
                                }
                                if (ArmorPower10000m != null)
                                {
                                    BulletInfo += "\nAPDS10000:" + ArmorPower10000m;
                                }
                                BulletInfo = BulletInfo.Replace(",", "");
                            }
                        }
                    }
                }
                if (RocketPath != null)
                {
                    string[] lines = File.ReadAllLines(Dataminepath + "\\aces.vromfs.bin_u\\" + RocketPath);
                    string data = null;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        data = lines[i];
                        BulletName = null;
                        string Mass = null;
                        string Caliber = null;
                        string Speed = null;
                        string DamageMass = null;
                        string DamageCaliber = null;
                        string ExplosiveMass = null;
                        string BulletType = null;
                        string Cx = null;
                        string BulletTypeName = null;
                        string ExplosiveType = null;
                        string ArmorPower = null;
                        if ((data.Contains("\"bullet\":")) || (data.Contains("\"rocket\":")))
                        {
                            bool HasModule = true;
                            if (lines[i - 1].Contains("{"))
                            {
                                HasModule = false;
                                BulletName = lines[i - 1].Split('\"')[1];
                                if (TankData.Contains(BulletName))
                                {
                                    HasModule = true;
                                }
                                BulletName = BulletName.Replace("_cn_", "_");
                                BulletName = BulletName.Replace("_fr_", "_");
                                BulletName = BulletName.Replace("_germ_", "_");
                                BulletName = BulletName.Replace("_il_", "_");
                                BulletName = BulletName.Replace("_it_", "_");
                                BulletName = BulletName.Replace("_jp_", "_");
                                BulletName = BulletName.Replace("_sw_", "_");
                                BulletName = BulletName.Replace("_uk_", "_");
                                BulletName = BulletName.Replace("_us_", "_");
                                BulletName = BulletName.Replace("_ussr_", "_");
                                if (TankData.Contains(BulletName))
                                {
                                    HasModule = true;
                                }
                                BulletName = null;
                            }
                            if (HasModule == true)
                            {
                                int Bracket = 1;
                                while (Bracket > 0)
                                {
                                    i++;
                                    data = lines[i];
                                    if (data.Contains("\"sabot\":"))
                                    {
                                        int Bracket2 = 1;
                                        while (Bracket2 > 0)
                                        {
                                            i++;
                                            data = lines[i];
                                            if (data.Contains("{"))
                                            {
                                                Bracket2++;
                                            }
                                            if (data.Contains("}"))
                                            {
                                                Bracket2--;
                                            }
                                        }
                                        i++;
                                        data = lines[i];
                                    }
                                    if (data.Contains("{") && !lines[i - 1].Contains("    \"bullet\":"))
                                    {
                                        Bracket++;
                                    }
                                    if (data.Contains("}"))
                                    {
                                        Bracket--;
                                    }
                                    if (data.Contains("["))
                                    {
                                        Bracket++;
                                    }
                                    if (data.Contains("]"))
                                    {
                                        Bracket--;
                                    }
                                    if (data.Contains("\"bulletName\""))
                                    {
                                        BulletName = data.Split('\"')[3];
                                    }
                                    if (data.Contains("\"mass\""))
                                    {
                                        Mass = data.Split('\"')[2];
                                        Mass = Mass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"explosiveMass\""))
                                    {
                                        ExplosiveMass = data.Split('\"')[2];
                                        ExplosiveMass = ExplosiveMass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"caliber\": 0."))
                                    {
                                        Caliber = data.Split('\"')[2];
                                        Caliber = Caliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"speed\""))
                                    {
                                        Speed = data.Split('\"')[2];
                                        Speed = Speed.Split(' ')[1];
                                    }
                                    if (data.Contains("\"damageMass\""))
                                    {
                                        DamageMass = data.Split('\"')[2];
                                        DamageMass = DamageMass.Split(' ')[1];
                                    }
                                    if (data.Contains("\"damageCaliber\": 0."))
                                    {
                                        DamageCaliber = data.Split('\"')[2];
                                        DamageCaliber = DamageCaliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"ballisticCaliber\""))
                                    {
                                        Caliber = data.Split('\"')[2];
                                        Caliber = Caliber.Split(' ')[1];
                                    }
                                    if (data.Contains("\"Cx\""))
                                    {
                                        string cxData = data.Split('\"')[2];
                                        if (cxData.Contains("["))
                                        {
                                            // Handle array case - collect all values and average them
                                            List<double> cxValues = new List<double>();
                                            string arrayLine = cxData;
                                            // Keep reading lines until we find the closing bracket
                                            while (!arrayLine.Contains("]") && i + 1 < lines.Length)
                                            {
                                                // Extract numbers from current line
                                                string[] parts = arrayLine.Split(new char[] { ',', '[', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                                foreach (string part in parts)
                                                {
                                                    if (double.TryParse(part, out double value))
                                                    {
                                                        cxValues.Add(value);
                                                    }
                                                }
                                                i++;
                                                arrayLine = lines[i];
                                            }
                                            // Process the final line with closing bracket
                                            string[] finalParts = arrayLine.Split(new char[] { ',', ']', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                            foreach (string part in finalParts)
                                            {
                                                if (double.TryParse(part, out double value))
                                                {
                                                    cxValues.Add(value);
                                                }
                                            }        // Calculate average
                                            if (cxValues.Count > 0)
                                            {
                                                double average = cxValues.Average();
                                                Cx = Math.Round(average, 4).ToString();
                                            }
                                            else
                                            {
                                                Cx = "0.38"; // Default fallback
                                            }
                                        }
                                        else
                                        {
                                            // Handle single value case as before
                                            Cx = cxData.Split(' ')[1];
                                        }
                                    }
                                    if (data.Contains("\"bulletType\""))
                                    {
                                        BulletType = data.Split('\"')[3];
                                        BulletTypeName = data.Split('\"')[3] + "/name/short";
                                    }
                                    if (data.Contains("\"endSpeed\""))
                                    {
                                        Speed = data.Split('\"')[2];
                                        Speed = Speed.Split(' ')[1];
                                    }
                                    if (data.Contains("\"explosiveType\""))
                                    {
                                        ExplosiveType = data.Split('\"')[3];
                                    }
                                    if (data.Contains("\"armorPower\""))
                                    {
                                        ArmorPower = data.Split('\"')[2];
                                        ArmorPower = ArmorPower.Split(' ')[1];
                                    }
                                }
                                if (Cx == null)
                                {
                                    Cx = "0.38";
                                }
                                if (BulletName == null)
                                {
                                    BulletName = BulletTypeName;
                                }
                                BulletInfo += "\n\nName:" + BulletName + "\nType:" + BulletType + "\nBulletMass:" + Mass + "\nBallisticCaliber:" + Caliber + "\nSpeed:" + Speed + "\nCx:" + Cx;
                                if (ExplosiveMass != null)
                                {
                                    BulletInfo += "\nExplosiveMass:" + ExplosiveMass;
                                }
                                if (ExplosiveType != null)
                                {
                                    BulletInfo += "\nExplosiveType:" + ExplosiveType;
                                }
                                if (DamageMass != null)
                                {
                                    BulletInfo += "\nDamageMass:" + DamageMass;
                                }
                                if (DamageCaliber != null)
                                {
                                    BulletInfo += "\nDamageCaliber:" + DamageCaliber;
                                }
                                if (ArmorPower != null)
                                {
                                    BulletInfo += "\nArmorPower:" + ArmorPower;
                                }
                                BulletInfo = BulletInfo.Replace(",", "");
                            }
                        }
                    }
                }

                if (RocketPath2 != null)
                {
                    string[] lines = File.ReadAllLines(Dataminepath + "\\aces.vromfs.bin_u\\" + RocketPath2);
                    string data = null;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        data = lines[i];
                        BulletName = null;
                        string Mass = null;
                        string Caliber = null;
                        string Speed = null;
                        string DamageMass = null;
                        string DamageCaliber = null;
                        string ExplosiveMass = null;
                        string BulletType = null;
                        string Cx = null;
                        string BulletTypeName = null;
                        string ExplosiveType = null;
                        string ArmorPower = null;
                        if ((data.Contains("\"bullet\":")) || (data.Contains("\"rocket\":")))
                        {
                            int Bracket = 1;
                            while (Bracket > 0)
                            {
                                i++;
                                data = lines[i];
                                if (data.Contains("\"sabot\":"))
                                {
                                    int Bracket2 = 1;
                                    while (Bracket2 > 0)
                                    {
                                        i++;
                                        data = lines[i];
                                        if (data.Contains("{"))
                                        {
                                            Bracket2++;
                                        }
                                        if (data.Contains("}"))
                                        {
                                            Bracket2--;
                                        }
                                    }
                                    i++;
                                    data = lines[i];
                                }
                                if (data.Contains("{") && !lines[i - 1].Contains("    \"bullet\":"))
                                {
                                    Bracket++;
                                }
                                if (data.Contains("}"))
                                {
                                    Bracket--;
                                }
                                if (data.Contains("["))
                                {
                                    Bracket++;
                                }
                                if (data.Contains("]"))
                                {
                                    Bracket--;
                                }
                                if (data.Contains("\"bulletName\""))
                                {
                                    BulletName = data.Split('\"')[3];
                                }
                                if (data.Contains("\"mass\""))
                                {
                                    Mass = data.Split('\"')[2];
                                    Mass = Mass.Split(' ')[1];
                                }
                                if (data.Contains("\"explosiveMass\""))
                                {
                                    ExplosiveMass = data.Split('\"')[2];
                                    ExplosiveMass = ExplosiveMass.Split(' ')[1];
                                }
                                if (data.Contains("\"caliber\": 0."))
                                {
                                    Caliber = data.Split('\"')[2];
                                    Caliber = Caliber.Split(' ')[1];
                                }
                                if (data.Contains("\"speed\""))
                                {
                                    Speed = data.Split('\"')[2];
                                    Speed = Speed.Split(' ')[1];
                                }
                                if (data.Contains("\"damageMass\""))
                                {
                                    DamageMass = data.Split('\"')[2];
                                    DamageMass = DamageMass.Split(' ')[1];
                                }
                                if (data.Contains("\"damageCaliber\": 0."))
                                {
                                    DamageCaliber = data.Split('\"')[2];
                                    DamageCaliber = DamageCaliber.Split(' ')[1];
                                }
                                if (data.Contains("\"ballisticCaliber\""))
                                {
                                    Caliber = data.Split('\"')[2];
                                    Caliber = Caliber.Split(' ')[1];
                                }
                                if (data.Contains("\"Cx\""))
                                {
                                    string cxData = data.Split('\"')[2];
                                    if (cxData.Contains("["))
                                    {
                                        // Handle array case - collect all values and average them
                                        List<double> cxValues = new List<double>();
                                        string arrayLine = cxData;
                                        // Keep reading lines until we find the closing bracket
                                        while (!arrayLine.Contains("]") && i + 1 < lines.Length)
                                        {
                                            // Extract numbers from current line
                                            string[] parts = arrayLine.Split(new char[] { ',', '[', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                            foreach (string part in parts)
                                            {
                                                if (double.TryParse(part, out double value))
                                                {
                                                    cxValues.Add(value);
                                                }
                                            }
                                            i++;
                                            arrayLine = lines[i];
                                        }
                                        // Process the final line with closing bracket
                                        string[] finalParts = arrayLine.Split(new char[] { ',', ']', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string part in finalParts)
                                        {
                                            if (double.TryParse(part, out double value))
                                            {
                                                cxValues.Add(value);
                                            }
                                        }        // Calculate average
                                        if (cxValues.Count > 0)
                                        {
                                            double average = cxValues.Average();
                                            Cx = Math.Round(average, 4).ToString();
                                        }
                                        else
                                        {
                                            Cx = "0.38"; // Default fallback
                                        }
                                    }
                                    else
                                    {
                                        // Handle single value case as before
                                        Cx = cxData.Split(' ')[1];
                                    }
                                }
                                if (data.Contains("\"bulletType\""))
                                {
                                    BulletType = data.Split('\"')[3];
                                    BulletTypeName = data.Split('\"')[3] + "/name/short";
                                }
                                if (data.Contains("\"endSpeed\""))
                                {
                                    Speed = data.Split('\"')[2];
                                    Speed = Speed.Split(' ')[1];
                                }
                                if (data.Contains("\"explosiveType\""))
                                {
                                    ExplosiveType = data.Split('\"')[3];
                                }
                                if (data.Contains("\"armorPower\""))
                                {
                                    ArmorPower = data.Split('\"')[2];
                                    ArmorPower = ArmorPower.Split(' ')[1];
                                }
                            }
                            if (Cx == null)
                            {
                                Cx = "0.38";
                            }
                            if (BulletName == null)
                            {
                                BulletName = BulletTypeName;
                            }
                            BulletInfo += "\n\nName:" + BulletName + "\nType:" + BulletType + "\nBulletMass:" + Mass + "\nBallisticCaliber:" + Caliber + "\nSpeed:" + Speed + "\nCx:" + Cx;
                            if (ExplosiveMass != null)
                            {
                                BulletInfo += "\nExplosiveMass:" + ExplosiveMass;
                            }
                            if (ExplosiveType != null)
                            {
                                BulletInfo += "\nExplosiveType:" + ExplosiveType;
                            }
                            if (DamageMass != null)
                            {
                                BulletInfo += "\nDamageMass:" + DamageMass;
                            }
                            if (DamageCaliber != null)
                            {
                                BulletInfo += "\nDamageCaliber:" + DamageCaliber;
                            }
                            if (ArmorPower != null)
                            {
                                BulletInfo += "\nArmorPower:" + ArmorPower;
                            }
                            BulletInfo = BulletInfo.Replace(",", "");
                        }
                    }
                }
                string text = "WeaponPath:" + WeaponPath;
                if (RocketPath != null)
                {
                    text += "\nRocketPath:" + RocketPath;
                }
                if (RocketPath2 != null)
                {
                    text += "\nRocketPath:" + RocketPath2;
                }
                text += "\nZoomIn:" + ZoomIn + "\nZoomOut:" + ZoomOut;
                if (HasLaser == true)
                {
                    text += "\nHasLaser";
                }
                text += BulletInfo;
                string LangData = null;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(Dataminepath + "\\lang.vromfs.bin_u\\lang\\units.csv"))
                {
                    LangData = sr.ReadToEnd();
                }
                StringReader reader2 = new StringReader(LangData);
                string line2 = String.Empty;
                string LangName2 = null;
                while ((line2 = reader2.ReadLine()) != null)
                {
                    if (line2.IndexOf(Path.GetFileNameWithoutExtension(file) + "_shop", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        LangName2 = line2.Split(';')[0];
                        LangName2 = LangName2.Replace("\"", "");
                        LangName2 = LangName2.Replace("_shop", "");
                    }
                }
                string TankPath = textBox3.Text + "//" + LangName2 + ".txt";
                if (WeaponPath != null)
                {
                    File.WriteAllText(TankPath, text);
                    if (ZoomIn2 != null)
                    {
                        string text2 = text.Replace(ZoomIn, ZoomIn2);
                        text2 = text2.Replace(ZoomOut, ZoomOut2);
                        File.WriteAllText(textBox3.Text + "//" + LangName2 + "_ModOptic.txt", text2);
                    }
                }
            }
            IsRuning = false;
            label1.Text = "File: ";
            label1.Refresh();
            progressBar1.Value = 0;
            SpeedNumbers = 0;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = file_list.Length;
            progressBar1.Step = 1;
            StartTime = DateTime.Now;
            IsRuning = true;
            foreach (string file in file_list)
            {
                label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                label1.Refresh();
                string TankPath = textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file);
                string BulletName = null;
                string Type = null;
                double BulletMass = 0;
                double BallisticCaliber = 0;
                double DamageMass = 0;
                double DamageCaliber = 0;
                double Speed = 0;
                double Cx = 0;
                double ExplosiveMass = 0;
                double demarrePenetrationK = 0;
                double demarreSpeedPow = 0;
                double demarreMassPow = 0;
                double demarreCaliberPow = 0;
                progressBar1.PerformStep();
                string TankData = null;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                {
                    TankData = sr.ReadToEnd();
                }
                StringReader reader = new StringReader(TankData);
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Name:"))
                    {
                        BulletName = line.Split(':')[1];
                        Type = null;
                        BulletMass = 0;
                        BallisticCaliber = 0;
                        DamageMass = 0;
                        DamageCaliber = 0;
                        Speed = 0;
                        Cx = 0;
                        ExplosiveMass = 0;
                        demarrePenetrationK = 0;
                        demarreSpeedPow = 0;
                        demarreMassPow = 0;
                        demarreCaliberPow = 0;
                        int b = 0;
                        double[,] ArmorPowerArray = new double[2, 12];
                        while (((line = reader.ReadLine()) != "") && (line != null))
                        {
                            if (line.StartsWith("Type:"))
                            {
                                Type = line.Split(':')[1];
                                if (Type.Contains("apds_fs"))
                                {
                                    Type = "apds_fs";
                                }
                                else
                                {
                                    Type = Type.Split('_')[0];
                                }
                            }
                            if (line.Contains("BulletMass:"))
                            {
                                BulletMass = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("BallisticCaliber:"))
                            {
                                BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("DamageMass:"))
                            {
                                DamageMass = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("DamageCaliber:"))
                            {
                                DamageCaliber = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Speed:"))
                            {
                                Speed = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Cx:"))
                            {
                                Cx = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ExplosiveMass:"))
                            {
                                ExplosiveMass = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("APDS"))
                            {
                                ArmorPowerArray[1, b] = Convert.ToDouble(line.Split(':')[1]);
                                line = line.Split(':')[0];
                                line = line.Substring(4);
                                ArmorPowerArray[0, b] = Convert.ToDouble(line);
                                b++;
                            }
                            if (line.Contains("demarrePenetrationK"))
                            {
                                demarrePenetrationK = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("demarreSpeedPow"))
                            {
                                demarreSpeedPow = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("demarreMassPow"))
                            {
                                demarreMassPow = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("demarreCaliberPow"))
                            {
                                demarreCaliberPow = Convert.ToDouble(line.Split(':')[1]);
                            }
                        }
                        if (Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam")
                        {
                            double Sensivity = Convert.ToDouble(trackBar1.Value) / 100;
                            string BallisticStrings = Ballistic(Sensivity, Type, BulletMass, Speed, BallisticCaliber, Cx, ExplosiveMass, DamageMass, DamageCaliber, demarrePenetrationK, demarreSpeedPow, demarreMassPow, demarreCaliberPow, ArmorPowerArray);
                            BulletName = BulletName.Split('/')[0];
                            if (BulletName.Contains("mm_"))
                            {
                                BulletName = BulletName.Remove(0, BulletName.IndexOf("mm_"));
                                BulletName = BulletName.Replace("mm_", "");
                            }
                            string BallisticPath = textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file);
                            if (Directory.Exists(BallisticPath) == false)
                            {
                                Directory.CreateDirectory(BallisticPath);
                            }
                            string FileName = BallisticPath + "//" + BulletName + ".txt";
                            File.WriteAllText(FileName, BallisticStrings);
                        }
                    }
                }
            }
            IsRuning = false;
            label1.Text = "File: ";
            label1.Refresh();
            progressBar1.Value = 0;
            SpeedNumbers = 0;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartTime = DateTime.Now;
            IsRuning = true;
            double Sensivity = Convert.ToDouble(trackBar1.Value) / 100;
            if (comboBox1.Text == "Tochka-SM2")
            {
                string Dataminepath = textBox1.Text;
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                string Language = "English";
                if (comboBox2.Text == "French") { Language = "French"; }
                if (comboBox2.Text == "Italian") { Language = "Italian"; }
                if (comboBox2.Text == "German") { Language = "German"; }
                if (comboBox2.Text == "Spanish") { Language = "Spanish"; }
                if (comboBox2.Text == "Russian") { Language = "Russian"; }
                if (comboBox2.Text == "Polish") { Language = "Polish"; }
                if (comboBox2.Text == "Czech") { Language = "Czech"; }
                if (comboBox2.Text == "Turkish") { Language = "Turkish"; }
                if (comboBox2.Text == "Chinese") { Language = "Chinese"; }
                if (comboBox2.Text == "Japanese") { Language = "Japanese"; }
                if (comboBox2.Text == "Portuguese") { Language = "Portuguese"; }
                if (comboBox2.Text == "Ukrainian") { Language = "Ukrainian"; }
                if (comboBox2.Text == "Serbian") { Language = "Serbian"; }
                if (comboBox2.Text == "Hungarian") { Language = "Hungarian"; }
                if (comboBox2.Text == "Korean") { Language = "Korean"; }
                if (comboBox2.Text == "Belarusian") { Language = "Belarusian"; }
                if (comboBox2.Text == "Romanian") { Language = "Romanian"; }
                if (comboBox2.Text == "TChinese") { Language = "TChinese"; }
                if (comboBox2.Text == "HChinese") { Language = "HChinese"; }
                bool BaseVersion = false;
                bool DoubleShells = false;
                bool LaserVersion = false;
                bool RocketVersion = false;
                bool HowitzerVersion = false;
                bool DrawCrosshairDistShow = false;
                bool drawOuterLinesShow = false;
                bool BalisticInfoShow = false;
                bool SightNameShow = false;
                bool TankSizesShow = false;
                bool TargetLockShow = false;
                bool RangerFinderShow = false;
                bool DistanceShow = false;
                string crosshairLightColor = Convert.ToString(pictureBox1.BackColor.R) + "," + Convert.ToString(pictureBox1.BackColor.G) + "," + Convert.ToString(pictureBox1.BackColor.B) + ",255";
                string rangefinderProgressBarColor1 = Convert.ToString(pictureBox2.BackColor.R) + "," + Convert.ToString(pictureBox2.BackColor.G) + "," + Convert.ToString(pictureBox2.BackColor.B) + ",64";
                string rangefinderProgressBarColor2 = Convert.ToString(pictureBox3.BackColor.R) + "," + Convert.ToString(pictureBox3.BackColor.G) + "," + Convert.ToString(pictureBox3.BackColor.B) + ",64";
                double lineSizeMult = Convert.ToDouble(trackBar2.Value) / 10;
                double InnerDiameter = Convert.ToDouble(trackBar4.Value) / 10;
                double PointThickness = Convert.ToDouble(trackBar3.Value) / 10;
                double[] RangerFinderPos = { Convert.ToDouble(textBox10.Text.Split(',')[0]), Convert.ToDouble(textBox10.Text.Split(',')[1]) };
                double[] DistancePos = { Convert.ToDouble(textBox9.Text.Split(',')[0]), Convert.ToDouble(textBox9.Text.Split(',')[1]) };
                double[] DetectAllyPos = { Convert.ToDouble(textBox8.Text.Split(',')[0]), Convert.ToDouble(textBox8.Text.Split(',')[1]) };
                double Length = Convert.ToDouble(textBox5.Text);
                double Height = Convert.ToDouble(textBox6.Text);
                double Width = Convert.ToDouble(textBox7.Text);
                double FontSize = Convert.ToDouble(trackBar5.Value) / 20;
                double DistanceFactor = Convert.ToDouble(trackBar6.Value) / 20;
                foreach (object itemChecked in checkedListBox1.CheckedItems)
                {
                    if (itemChecked.ToString().Contains("Tochka-SMD2 (Double Shells)").Equals(true))
                    {
                        DoubleShells = true;
                    }
                    if (itemChecked.ToString().Contains("Tochka-SM2 (Base Version)").Equals(true))
                    {
                        BaseVersion = true;
                    }
                    if (itemChecked.ToString().Contains("Tochka-SML2 (For Laser Rangefinders)").Equals(true))
                    {
                        LaserVersion = true;
                    }
                    if (itemChecked.ToString().Contains("Tochka-SMR2 (For SAM, AAM, ATGM)").Equals(true))
                    {
                        RocketVersion = true;
                    }
                    if (itemChecked.ToString().Contains("Tochka-SMH2 (For Howitzers)").Equals(true))
                    {
                        HowitzerVersion = true;
                    }
                }
                foreach (object itemChecked in checkedListBox3.CheckedItems)
                {
                    if (itemChecked.ToString().Contains("Range Correction Notches").Equals(true))
                    {
                        DrawCrosshairDistShow = true;
                    }
                    if (itemChecked.ToString().Contains("Outer Lines").Equals(true))
                    {
                        drawOuterLinesShow = true;
                    }
                    if (itemChecked.ToString().Contains("Ballistic Info").Equals(true))
                    {
                        BalisticInfoShow = true;
                    }
                    if (itemChecked.ToString().Contains("Sight Name").Equals(true))
                    {
                        SightNameShow = true;
                    }
                    if (itemChecked.ToString().Contains("Tank Sizes").Equals(true))
                    {
                        TankSizesShow = true;
                    }
                    if (itemChecked.ToString().Contains("\"Target Lock: ON\" Text").Equals(true))
                    {
                        TargetLockShow = true;
                    }
                    if (itemChecked.ToString().Contains("\"Rangefinder\" Text").Equals(true))
                    {
                        RangerFinderShow = true;
                    }
                    if (itemChecked.ToString().Contains("\"Distance\" Text").Equals(true))
                    {
                        DistanceShow = true;
                    }
                }
                foreach (string file in file_list)
                {
                    progressBar1.PerformStep();
                    string Country = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                    bool MakeSight = false;
                    foreach (object itemChecked in checkedListBox2.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("USA").Equals(true) && Country == "us")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Germany").Equals(true) && Country == "germ")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("USSR").Equals(true) && Country == "ussr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Britain").Equals(true) && Country == "uk")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Japan").Equals(true) && Country == "jp")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("China").Equals(true) && Country == "cn")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Italy").Equals(true) && Country == "it")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("France").Equals(true) && Country == "fr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Sweden").Equals(true) && Country == "sw")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Israel").Equals(true) && Country == "il")
                        {
                            MakeSight = true;
                        }
                    }
                    if (MakeSight == true && checkedListBox1.CheckedItems.Count > 0)
                    {
                        label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                        label1.Refresh();
                        string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string BulletName2 = null;
                        string Type = null;
                        string Type2 = null;
                        double BallisticCaliber = 0;
                        double BallisticCaliber2 = 0;
                        double Speed = 0;
                        double Speed2 = 0;
                        double ExplosiveMass = 0;
                        double ExplosiveMass2 = 0;
                        string ExplosiveType = null;
                        string ExplosiveType2 = null;
                        bool HasRocket = false;
                        bool HasLaser = false;
                        string RocketName = null;
                        string RocketType = null;
                        double RocketSpeed = 0;
                        double RocketArmorPower = 0;
                        double ArmorPower = 0;
                        double ArmorPower2 = 0;
                        string TankData = null;
                        double BulletMass = 0;
                        double DamageMass = 0;
                        double DamageCaliber = 0;
                        double Cx = 0;
                        double demarrePenetrationK = 0;
                        double demarreSpeedPow = 0;
                        double demarreMassPow = 0;
                        double demarreCaliberPow = 0;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }

                        string LangData = null;
                        string LangData2 = null;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader("Localization\\units_weaponry.csv"))
                        {
                            LangData = sr.ReadToEnd();
                        }
                        using (System.IO.StreamReader sr = new System.IO.StreamReader("Localization\\FCS.csv"))
                        {
                            LangData2 = sr.ReadToEnd();
                        }
                        StringReader readertest = new StringReader(TankData);
                        bool OnlyRocket = true;
                        string line = String.Empty;
                        while ((line = readertest.ReadLine()) != null)
                        {
                            if (line.StartsWith("Type:"))
                            {
                                Type = line.Split(':')[1];
                                Type = Type.Split('_')[0];
                                if (Type != "atgm" && Type != "sam" && Type != "aam")
                                {
                                    OnlyRocket = false;
                                }
                            }
                        }
                        StringReader reader = new StringReader(TankData);
                        line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("RocketPath:"))
                            {
                                HasRocket = true;
                            }
                            if (line.Contains("HasLaser"))
                            {
                                HasLaser = true;
                            }
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BallisticCaliber = 0;
                                Speed = 0;
                                Speed2 = 0;
                                ExplosiveMass = 0;
                                RocketName = null;
                                RocketType = null;
                                RocketArmorPower = 0;
                                RocketSpeed = 0;
                                ExplosiveType = null;
                                ArmorPower = 0;
                                BulletMass = 0;
                                DamageMass = 0;
                                DamageCaliber = 0;
                                Cx = 0;
                                demarrePenetrationK = 0;
                                demarreSpeedPow = 0;
                                demarreMassPow = 0;
                                demarreCaliberPow = 0;
                                double[,] ArmorPowerArray = new double[2, 12];
                                int b = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        if (Type.Contains("apds_fs"))
                                        {
                                            Type = "apds_fs";
                                        }
                                        if (Type.Contains("heat_fs"))
                                        {
                                            Type = "heat_fs";
                                        }
                                        if (Type.Contains("he_frag_fs"))
                                        {
                                            Type = "he_frag_fs";
                                        }
                                        if (Type.Contains("heat_grenade"))
                                        {
                                            Type = "heat_grenade";
                                        }
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("ExplosiveMass:"))
                                    {
                                        ExplosiveMass = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("ExplosiveType:"))
                                    {
                                        ExplosiveType = line.Split(':')[1];
                                    }
                                    if (line.Contains("ArmorPower:"))
                                    {
                                        ArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("BulletMass:"))
                                    {
                                        BulletMass = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("DamageMass:"))
                                    {
                                        DamageMass = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("DamageCaliber:"))
                                    {
                                        DamageCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Cx:"))
                                    {
                                        Cx = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("APDS"))
                                    {
                                        ArmorPowerArray[1, b] = Convert.ToDouble(line.Split(':')[1]);
                                        line = line.Split(':')[0];
                                        line = line.Substring(4);
                                        ArmorPowerArray[0, b] = Convert.ToDouble(line);
                                        b++;
                                    }
                                    if (line.Contains("demarrePenetrationK"))
                                    {
                                        demarrePenetrationK = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("demarreSpeedPow"))
                                    {
                                        demarreSpeedPow = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("demarreMassPow"))
                                    {
                                        demarreMassPow = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("demarreCaliberPow"))
                                    {
                                        demarreCaliberPow = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                string data = null;
                                string FileName = null;
                                string BallisticData = null;
                                string BallisticData2 = null;
                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                if (HasRocket == true && BaseVersion == true && OnlyRocket == false)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    StringReader reader2 = new StringReader(TankData);
                                    while ((line = reader2.ReadLine()) != null)
                                    {
                                        if (line.Contains("Name:"))
                                        {
                                            RocketName = line.Split(':')[1];
                                            RocketSpeed = 0;
                                            RocketType = null;
                                            while (((line = reader2.ReadLine()) != "") && (line != null))
                                            {
                                                if (line.StartsWith("Type:"))
                                                {
                                                    RocketType = line.Split(':')[1];
                                                    if (RocketType.Contains("atgm_vt"))
                                                    {
                                                        RocketType = "atgm_vt";
                                                    }
                                                    else
                                                    {
                                                        RocketType = RocketType.Split('_')[0];
                                                    }
                                                }
                                                if (line.Contains("Speed:"))
                                                {
                                                    RocketSpeed = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ArmorPower:"))
                                                {
                                                    RocketArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                            }
                                            if (Type == "atgm_vt")
                                            {
                                                RocketArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                            }
                                            if (RocketType == "atgm" || RocketType == "sam" || RocketType == "rocket" || RocketType == "aam")
                                            {
                                                if (BulletNameForBallistic.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                                }
                                                if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice" && Type != "napalm" && Type != "napalm_gel") || (Type == "he" && BallisticCaliber >= 0.12))
                                                {
                                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                                    {
                                                        BallisticData = sr.ReadToEnd();
                                                    }
                                                    string LangName = null;
                                                    string LangName2 = null;
                                                    string LangRocketName = null;
                                                    FileName = "Tochka_SM2_";
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
                                                    StringReader reader1 = new StringReader(LangData);
                                                    string line1 = String.Empty;
                                                    while ((line1 = reader1.ReadLine()) != null)
                                                    {
                                                        if (line1.Contains("\"" + BulletName + "\""))
                                                        {
                                                            var parts = line1.Split(';');
                                                            if (Pos < parts.Length)
                                                            {
                                                                LangName = parts[Pos].Replace("\"", "");
                                                            }
                                                            else
                                                            {
                                                                LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                            }
                                                        }
                                                    }
                                                    if (HasRocket == true)
                                                    {
                                                        StringReader reader3 = new StringReader(LangData);
                                                        line1 = String.Empty;
                                                        while ((line1 = reader3.ReadLine()) != null)
                                                        {
                                                            if (line1.Contains("\"" + RocketName + "\""))
                                                            {
                                                                var partsR = line1.Split(';');
                                                                if (Pos < partsR.Length)
                                                                {
                                                                    LangRocketName = partsR[Pos].Replace("\"", "");
                                                                }
                                                                else
                                                                {
                                                                    LangRocketName = (partsR.Length > 1 ? partsR[1].Replace("\"", "") : RocketName.Split('/')[0]);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    string SightType = "Base";
                                                    data = TochkaSM2.Create(
                                                        SightType,
                                                        Type,
                                                        Type2,
                                                        Speed,
                                                        Speed2,
                                                        ArmorPower,
                                                        ArmorPower2,
                                                        ZoomIn,
                                                        ZoomOut,
                                                        Sensivity,
                                                        Language,
                                                        DrawCrosshairDistShow,
                                                        drawOuterLinesShow,
                                                        BalisticInfoShow,
                                                        SightNameShow,
                                                        TankSizesShow,
                                                        TargetLockShow,
                                                        RangerFinderShow,
                                                        DistanceShow,
                                                        lineSizeMult,
                                                        InnerDiameter,
                                                        PointThickness,
                                                        RangerFinderPos,
                                                        DistancePos,
                                                        DetectAllyPos,
                                                        rangefinderProgressBarColor1,
                                                        rangefinderProgressBarColor2,
                                                        crosshairLightColor,
                                                        Length,
                                                        Height,
                                                        Width,
                                                        "real",
                                                        10,
                                                        LangName,
                                                        LangName2,
                                                        BallisticData,
                                                        BallisticData2,
                                                        LangRocketName,
                                                        RocketSpeed,
                                                        RocketArmorPower,
                                                        LangData2,
                                                        FontSize,
                                                        DistanceFactor
                                                        );
                                                    RocketName = RocketName.Split('/')[0];
                                                    if (RocketName.Contains("mm_"))
                                                    {
                                                        RocketName = RocketName.Remove(0, RocketName.IndexOf("mm_"));
                                                        RocketName = RocketName.Replace("mm_", "");
                                                    }
                                                    string TankPath = null;
                                                    if (file.Contains("_ModOptic"))
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                                        FileName += "ModOptic_";
                                                    }
                                                    else
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                                    }
                                                    FileName += BulletNameForBallistic;
                                                    if (HasRocket == true)
                                                    {
                                                        FileName += "_" + RocketName;
                                                    }
                                                    if (Directory.Exists(TankPath) == false)
                                                    {
                                                        Directory.CreateDirectory(TankPath);
                                                    }
                                                    FileName = TankPath + "//" + FileName + ".blk";
                                                    File.WriteAllText(FileName, data);
                                                    Application.DoEvents();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (HasRocket == false && BaseVersion == true && OnlyRocket == false)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    if (BulletNameForBallistic.Contains("mm_"))
                                    {
                                        BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                        BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                    }
                                    if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12))
                                    {
                                        using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                        {
                                            BallisticData = sr.ReadToEnd();
                                        }
                                        string LangName = null;
                                        string LangName2 = null;
                                        string LangRocketName = null;
                                        FileName = "Tochka_SM2_";
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
                                        StringReader reader1 = new StringReader(LangData);
                                        string line1 = String.Empty;
                                        while ((line1 = reader1.ReadLine()) != null)
                                        {
                                            if (line1.Contains("\"" + BulletName + "\""))
                                            {
                                                var parts = line1.Split(';');
                                                if (Pos < parts.Length)
                                                {
                                                    LangName = parts[Pos].Replace("\"", "");
                                                }
                                                else
                                                {
                                                    LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                }
                                            }
                                        }
                                        string SightType = "Base";
                                        data = TochkaSM2.Create(
                                            SightType,
                                            Type,
                                            Type2,
                                            Speed,
                                            Speed2,
                                            ArmorPower,
                                            ArmorPower2,
                                            ZoomIn,
                                            ZoomOut,
                                            Sensivity,
                                            Language,
                                            DrawCrosshairDistShow,
                                            drawOuterLinesShow,
                                            BalisticInfoShow,
                                            SightNameShow,
                                            TankSizesShow,
                                            TargetLockShow,
                                            RangerFinderShow,
                                            DistanceShow,
                                            lineSizeMult,
                                            InnerDiameter,
                                            PointThickness,
                                            RangerFinderPos,
                                            DistancePos,
                                            DetectAllyPos,
                                            rangefinderProgressBarColor1,
                                            rangefinderProgressBarColor2,
                                            crosshairLightColor,
                                            Length,
                                            Height,
                                            Width,
                                            "real",
                                            10,
                                            LangName,
                                            LangName2,
                                            BallisticData,
                                            BallisticData2,
                                            LangRocketName,
                                            RocketSpeed,
                                            RocketArmorPower,
                                            LangData2,
                                            FontSize,
                                            DistanceFactor
                                            );
                                        string TankPath = null;
                                        if (file.Contains("_ModOptic"))
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                            FileName += "ModOptic_";
                                        }
                                        else
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                        }
                                        FileName += BulletNameForBallistic;
                                        if (Directory.Exists(TankPath) == false)
                                        {
                                            Directory.CreateDirectory(TankPath);
                                        }
                                        FileName = TankPath + "//" + FileName + ".blk";
                                        File.WriteAllText(FileName, data);
                                        Application.DoEvents();
                                    }
                                }
                                if (RocketVersion == true && OnlyRocket == true)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    if (BulletNameForBallistic.Contains("mm_"))
                                    {
                                        BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                        BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                    }
                                    string LangName = null;
                                    string LangName2 = null;
                                    string LangRocketName = null;
                                    FileName = "Tochka_SMR2_";
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
                                    StringReader reader1 = new StringReader(LangData);
                                    string line1 = String.Empty;
                                    while ((line1 = reader1.ReadLine()) != null)
                                    {
                                        if (line1.Contains("\"" + BulletName + "\""))
                                        {
                                            var parts = line1.Split(';');
                                            if (Pos < parts.Length)
                                            {
                                                LangName = parts[Pos].Replace("\"", "");
                                            }
                                            else
                                            {
                                                LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                            }
                                        }
                                    }
                                    string SightType = "Rocket";
                                    data = TochkaSM2.Create(
                                        SightType,
                                        Type,
                                        Type2,
                                        Speed,
                                        Speed2,
                                        ArmorPower,
                                        ArmorPower2,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        Language,
                                        DrawCrosshairDistShow,
                                        drawOuterLinesShow,
                                        BalisticInfoShow,
                                        SightNameShow,
                                        TankSizesShow,
                                        TargetLockShow,
                                        RangerFinderShow,
                                        DistanceShow,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DistancePos,
                                        DetectAllyPos,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Height,
                                        Width,
                                        "real",
                                        10,
                                        LangName,
                                        LangName2,
                                        BallisticData,
                                        BallisticData2,
                                        LangRocketName,
                                        RocketSpeed,
                                        RocketArmorPower,
                                        LangData2,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                                if (DoubleShells == true && OnlyRocket == false)
                                {
                                    StringReader reader2 = new StringReader(TankData);
                                    while ((line = reader2.ReadLine()) != null)
                                    {
                                        if (line.Contains("Name:"))
                                        {
                                            BulletName2 = line.Split(':')[1];
                                            Type2 = null;
                                            BallisticCaliber2 = 0;
                                            ExplosiveMass2 = 0;
                                            ExplosiveType2 = null;
                                            ArmorPower2 = 0;
                                            while (((line = reader2.ReadLine()) != "") && (line != null))
                                            {
                                                if (line.StartsWith("Type:"))
                                                {
                                                    Type2 = line.Split(':')[1];
                                                    if (Type2.Contains("apds_fs"))
                                                    {
                                                        Type2 = "apds_fs";
                                                    }
                                                    if (Type2.Contains("heat_fs"))
                                                    {
                                                        Type2 = "heat_fs";
                                                    }
                                                    if (Type2.Contains("he_frag_fs"))
                                                    {
                                                        Type2 = "he_frag_fs";
                                                    }
                                                    if (Type2.Contains("heat_grenade"))
                                                    {
                                                        Type2 = "heat_grenade";
                                                    }
                                                    else
                                                    {
                                                        Type2 = Type2.Split('_')[0];
                                                    }
                                                }
                                                if (line.Contains("BallisticCaliber:"))
                                                {
                                                    BallisticCaliber2 = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("Speed:"))
                                                {
                                                    Speed2 = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ExplosiveMass:"))
                                                {
                                                    ExplosiveMass2 = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ExplosiveType:"))
                                                {
                                                    ExplosiveType2 = line.Split(':')[1];
                                                }
                                                if (line.Contains("ArmorPower:"))
                                                {
                                                    ArmorPower2 = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                            }
                                            bool DoubleShell = CanUseDoubleShell(BulletName, BulletName2, Type, Type2, BallisticCaliber, BallisticCaliber2);
                                            if (DoubleShell == true)
                                            {
                                                string BulletNameForBallistic2 = BulletName2.Split('/')[0];
                                                Type = Type.Split('_')[0];
                                                Type2 = Type2.Split('_')[0];
                                                if (Type == "he")
                                                {
                                                    ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                                }
                                                if (Type2 == "he")
                                                {
                                                    ArmorPower2 = HePenetration(ExplosiveMass2, ExplosiveType2);
                                                }
                                                if (BulletNameForBallistic.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                                }
                                                if (BulletNameForBallistic2.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic2 = BulletNameForBallistic2.Remove(0, BulletNameForBallistic2.IndexOf("mm_"));
                                                    BulletNameForBallistic2 = BulletNameForBallistic2.Replace("mm_", "");
                                                }
                                                if (((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice" && Type != "napalm" && Type != "napalm_gel") || (Type == "he" && BallisticCaliber >= 0.12)) &&
                                                    ((Type2 != "sam" && Type2 != "atgm" && Type2 != "rocket" && Type2 != "aam" && Type2 != "smoke" && Type2 != "shrapnel" && Type2 != "he" && Type2 != "practice" && Type2 != "napalm" && Type2 != "napalm_gel") || (Type2 == "he" && BallisticCaliber2 >= 0.037)))
                                                {
                                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                                    {
                                                        BallisticData = sr.ReadToEnd();
                                                    }
                                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic2 + ".txt"))
                                                    {
                                                        BallisticData2 = sr.ReadToEnd();
                                                    }
                                                    string LangName = null;
                                                    string LangName2 = null;
                                                    string LangRocketName = null;
                                                    FileName = "Tochka_SMD2_";
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
                                                    StringReader reader1 = new StringReader(LangData);
                                                    string line1 = String.Empty;
                                                    while ((line1 = reader1.ReadLine()) != null)
                                                    {
                                                        if (line1.Contains("\"" + BulletName + "\""))
                                                        {
                                                            var parts = line1.Split(';');
                                                            if (Pos < parts.Length)
                                                            {
                                                                LangName = parts[Pos].Replace("\"", "");
                                                            }
                                                            else
                                                            {
                                                                LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                            }
                                                        }
                                                        if (line1.Contains("\"" + BulletName2 + "\""))
                                                        {
                                                            var parts2 = line1.Split(';');
                                                            if (Pos < parts2.Length)
                                                            {
                                                                LangName2 = parts2[Pos].Replace("\"", "");
                                                            }
                                                            else
                                                            {
                                                                LangName2 = (parts2.Length > 1 ? parts2[1].Replace("\"", "") : BulletName2.Split('/')[0]);
                                                            }
                                                        }
                                                    }
                                                    string SightType = "Double";
                                                    data = TochkaSM2.Create(
                                                        SightType,
                                                        Type,
                                                        Type2,
                                                        Speed,
                                                        Speed2,
                                                        ArmorPower,
                                                        ArmorPower2,
                                                        ZoomIn,
                                                        ZoomOut,
                                                        Sensivity,
                                                        Language,
                                                        DrawCrosshairDistShow,
                                                        drawOuterLinesShow,
                                                        BalisticInfoShow,
                                                        SightNameShow,
                                                        TankSizesShow,
                                                        TargetLockShow,
                                                        RangerFinderShow,
                                                        DistanceShow,
                                                        lineSizeMult,
                                                        InnerDiameter,
                                                        PointThickness,
                                                        RangerFinderPos,
                                                        DistancePos,
                                                        DetectAllyPos,
                                                        rangefinderProgressBarColor1,
                                                        rangefinderProgressBarColor2,
                                                        crosshairLightColor,
                                                        Length,
                                                        Height,
                                                        Width,
                                                        "real",
                                                        10,
                                                        LangName,
                                                        LangName2,
                                                        BallisticData,
                                                        BallisticData2,
                                                        LangRocketName,
                                                        RocketSpeed,
                                                        RocketArmorPower,
                                                        LangData2,
                                                        FontSize,
                                                        DistanceFactor
                                                        );
                                                    string TankPath = null;
                                                    if (file.Contains("_ModOptic"))
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                                        FileName += "ModOptic_";
                                                    }
                                                    else
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                                    }
                                                    FileName += BulletNameForBallistic;
                                                    FileName += "_" + BulletNameForBallistic2;
                                                    if (Directory.Exists(TankPath) == false)
                                                    {
                                                        Directory.CreateDirectory(TankPath);
                                                    }
                                                    FileName = TankPath + "//" + FileName + ".blk";
                                                    File.WriteAllText(FileName, data);
                                                    Application.DoEvents();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (HasRocket == true && LaserVersion == true && HasLaser == true && OnlyRocket == false)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    StringReader reader2 = new StringReader(TankData);
                                    while ((line = reader2.ReadLine()) != null)
                                    {
                                        if (line.Contains("Name:"))
                                        {
                                            RocketName = line.Split(':')[1];
                                            RocketSpeed = 0;
                                            RocketType = null;
                                            while (((line = reader2.ReadLine()) != "") && (line != null))
                                            {
                                                if (line.StartsWith("Type:"))
                                                {
                                                    RocketType = line.Split(':')[1];
                                                    if (RocketType.Contains("atgm_vt"))
                                                    {
                                                        RocketType = "atgm_vt";
                                                    }
                                                    else
                                                    {
                                                        RocketType = RocketType.Split('_')[0];
                                                    }
                                                }
                                                if (line.Contains("Speed:"))
                                                {
                                                    RocketSpeed = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ArmorPower:"))
                                                {
                                                    RocketArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                            }
                                            if (Type == "atgm_vt")
                                            {
                                                RocketArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                            }
                                            if (RocketType == "atgm" || RocketType == "sam" || RocketType == "rocket" || RocketType == "aam")
                                            {
                                                if (BulletNameForBallistic.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                                }
                                                if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12))
                                                {
                                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                                    {
                                                        BallisticData = sr.ReadToEnd();
                                                    }
                                                    string LangName = null;
                                                    string LangName2 = null;
                                                    string LangRocketName = null;
                                                    FileName = "Tochka_SML2_";
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
                                                    StringReader reader1 = new StringReader(LangData);
                                                    string line1 = String.Empty;
                                                    while ((line1 = reader1.ReadLine()) != null)
                                                    {
                                                        if (line1.Contains("\"" + BulletName + "\""))
                                                        {
                                                            var parts = line1.Split(';');
                                                            if (Pos < parts.Length)
                                                            {
                                                                LangName = parts[Pos].Replace("\"", "");
                                                            }
                                                            else
                                                            {
                                                                LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                            }
                                                        }
                                                    }
                                                    if (HasRocket == true)
                                                    {
                                                        StringReader reader3 = new StringReader(LangData);
                                                        line1 = String.Empty;
                                                        while ((line1 = reader3.ReadLine()) != null)
                                                        {
                                                            if (line1.Contains("\"" + RocketName + "\""))
                                                            {
                                                                var partsR = line1.Split(';');
                                                                if (Pos < partsR.Length)
                                                                {
                                                                    LangRocketName = partsR[Pos].Replace("\"", "");
                                                                }
                                                                else
                                                                {
                                                                    LangRocketName = (partsR.Length > 1 ? partsR[1].Replace("\"", "") : RocketName.Split('/')[0]);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    string SightType = "Laser";
                                                    data = TochkaSM2.Create(
                                                        SightType,
                                                        Type,
                                                        Type2,
                                                        Speed,
                                                        Speed2,
                                                        ArmorPower,
                                                        ArmorPower2,
                                                        ZoomIn,
                                                        ZoomOut,
                                                        Sensivity,
                                                        Language,
                                                        DrawCrosshairDistShow,
                                                        drawOuterLinesShow,
                                                        BalisticInfoShow,
                                                        SightNameShow,
                                                        TankSizesShow,
                                                        TargetLockShow,
                                                        RangerFinderShow,
                                                        DistanceShow,
                                                        lineSizeMult,
                                                        InnerDiameter,
                                                        PointThickness,
                                                        RangerFinderPos,
                                                        DistancePos,
                                                        DetectAllyPos,
                                                        rangefinderProgressBarColor1,
                                                        rangefinderProgressBarColor2,
                                                        crosshairLightColor,
                                                        Length,
                                                        Height,
                                                        Width,
                                                        "real",
                                                        10,
                                                        LangName,
                                                        LangName2,
                                                        BallisticData,
                                                        BallisticData2,
                                                        LangRocketName,
                                                        RocketSpeed,
                                                        RocketArmorPower,
                                                        LangData2,
                                                        FontSize,
                                                        DistanceFactor
                                                        );
                                                    RocketName = RocketName.Split('/')[0];
                                                    if (RocketName.Contains("mm_"))
                                                    {
                                                        RocketName = RocketName.Remove(0, RocketName.IndexOf("mm_"));
                                                        RocketName = RocketName.Replace("mm_", "");
                                                    }
                                                    string TankPath = null;
                                                    if (file.Contains("_ModOptic"))
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                                        FileName += "ModOptic_";
                                                    }
                                                    else
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                                    }
                                                    FileName += BulletNameForBallistic;
                                                    if (HasRocket == true)
                                                    {
                                                        FileName += "_" + RocketName;
                                                    }
                                                    if (Directory.Exists(TankPath) == false)
                                                    {
                                                        Directory.CreateDirectory(TankPath);
                                                    }
                                                    FileName = TankPath + "//" + FileName + ".blk";
                                                    File.WriteAllText(FileName, data);
                                                    Application.DoEvents();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (HasRocket == false && LaserVersion == true && HasLaser == true && OnlyRocket == false)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    if (BulletNameForBallistic.Contains("mm_"))
                                    {
                                        BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                        BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                    }
                                    if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12))
                                    {
                                        using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                        {
                                            BallisticData = sr.ReadToEnd();
                                        }
                                        string LangName = null;
                                        string LangName2 = null;
                                        string LangRocketName = null;
                                        FileName = "Tochka_SML2_";
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
                                        StringReader reader1 = new StringReader(LangData);
                                        string line1 = String.Empty;
                                        while ((line1 = reader1.ReadLine()) != null)
                                        {
                                            if (line1.Contains("\"" + BulletName + "\""))
                                            {
                                                var parts = line1.Split(';');
                                                if (Pos < parts.Length)
                                                {
                                                    LangName = parts[Pos].Replace("\"", "");
                                                }
                                                else
                                                {
                                                    LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                }
                                            }
                                        }
                                        string SightType = "Laser";
                                        data = TochkaSM2.Create(
                                            SightType,
                                            Type,
                                            Type2,
                                            Speed,
                                            Speed2,
                                            ArmorPower,
                                            ArmorPower2,
                                            ZoomIn,
                                            ZoomOut,
                                            Sensivity,
                                            Language,
                                            DrawCrosshairDistShow,
                                            drawOuterLinesShow,
                                            BalisticInfoShow,
                                            SightNameShow,
                                            TankSizesShow,
                                            TargetLockShow,
                                            RangerFinderShow,
                                            DistanceShow,
                                            lineSizeMult,
                                            InnerDiameter,
                                            PointThickness,
                                            RangerFinderPos,
                                            DistancePos,
                                            DetectAllyPos,
                                            rangefinderProgressBarColor1,
                                            rangefinderProgressBarColor2,
                                            crosshairLightColor,
                                            Length,
                                            Height,
                                            Width,
                                            "real",
                                            10,
                                            LangName,
                                            LangName2,
                                            BallisticData,
                                            BallisticData2,
                                            LangRocketName,
                                            RocketSpeed,
                                            RocketArmorPower,
                                            LangData2,
                                            FontSize,
                                            DistanceFactor
                                            );
                                        string TankPath = null;
                                        if (file.Contains("_ModOptic"))
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                            FileName += "ModOptic_";
                                        }
                                        else
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                        }
                                        FileName += BulletNameForBallistic;
                                        if (Directory.Exists(TankPath) == false)
                                        {
                                            Directory.CreateDirectory(TankPath);
                                        }
                                        FileName = TankPath + "//" + FileName + ".blk";
                                        File.WriteAllText(FileName, data);
                                        Application.DoEvents();
                                    }
                                }
                                if (HasRocket == true && HowitzerVersion == true && OnlyRocket == false && Speed < 500)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    StringReader reader2 = new StringReader(TankData);
                                    while ((line = reader2.ReadLine()) != null)
                                    {
                                        if (line.Contains("Name:"))
                                        {
                                            RocketName = line.Split(':')[1];
                                            RocketSpeed = 0;
                                            RocketType = null;
                                            while (((line = reader2.ReadLine()) != "") && (line != null))
                                            {
                                                if (line.StartsWith("Type:"))
                                                {
                                                    RocketType = line.Split(':')[1];
                                                    if (RocketType.Contains("atgm_vt"))
                                                    {
                                                        RocketType = "atgm_vt";
                                                    }
                                                    else
                                                    {
                                                        RocketType = RocketType.Split('_')[0];
                                                    }
                                                }
                                                if (line.Contains("Speed:"))
                                                {
                                                    RocketSpeed = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ArmorPower:"))
                                                {
                                                    RocketArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                            }
                                            if (Type == "atgm_vt")
                                            {
                                                RocketArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                            }
                                            if (RocketType == "atgm" || RocketType == "sam" || RocketType == "rocket" || RocketType == "aam")
                                            {
                                                if (BulletNameForBallistic.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                                }
                                                if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12))
                                                {
                                                    BallisticData = Ballistic(1, Type, BulletMass, Speed, BallisticCaliber, Cx, ExplosiveMass, DamageMass, DamageCaliber, demarrePenetrationK, demarreSpeedPow, demarreMassPow, demarreCaliberPow, ArmorPowerArray);
                                                    string LangName = null;
                                                    string LangName2 = null;
                                                    string LangRocketName = null;
                                                    FileName = "Tochka_SMH2_";
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
                                                    StringReader reader1 = new StringReader(LangData);
                                                    string line1 = String.Empty;
                                                    while ((line1 = reader1.ReadLine()) != null)
                                                    {
                                                        if (line1.Contains("\"" + BulletName + "\""))
                                                        {
                                                            var parts = line1.Split(';');
                                                            if (Pos < parts.Length)
                                                            {
                                                                LangName = parts[Pos].Replace("\"", "");
                                                            }
                                                            else
                                                            {
                                                                LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                            }
                                                        }
                                                    }
                                                    if (HasRocket == true)
                                                    {
                                                        StringReader reader3 = new StringReader(LangData);
                                                        line1 = String.Empty;
                                                        while ((line1 = reader3.ReadLine()) != null)
                                                        {
                                                            if (line1.Contains("\"" + RocketName + "\""))
                                                            {
                                                                var partsR = line1.Split(';');
                                                                if (Pos < partsR.Length)
                                                                {
                                                                    LangRocketName = partsR[Pos].Replace("\"", "");
                                                                }
                                                                else
                                                                {
                                                                    LangRocketName = (partsR.Length > 1 ? partsR[1].Replace("\"", "") : RocketName.Split('/')[0]);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    string SightType = "Howitzer";
                                                    data = TochkaSM2.Create(
                                                        SightType,
                                                        Type,
                                                        Type2,
                                                        Speed,
                                                        Speed2,
                                                        ArmorPower,
                                                        ArmorPower2,
                                                        ZoomIn,
                                                        ZoomOut,
                                                        1,
                                                        Language,
                                                        DrawCrosshairDistShow,
                                                        drawOuterLinesShow,
                                                        BalisticInfoShow,
                                                        SightNameShow,
                                                        TankSizesShow,
                                                        TargetLockShow,
                                                        RangerFinderShow,
                                                        DistanceShow,
                                                        lineSizeMult,
                                                        InnerDiameter,
                                                        PointThickness,
                                                        RangerFinderPos,
                                                        DistancePos,
                                                        DetectAllyPos,
                                                        rangefinderProgressBarColor1,
                                                        rangefinderProgressBarColor2,
                                                        crosshairLightColor,
                                                        Length,
                                                        Height,
                                                        Width,
                                                        "real",
                                                        10,
                                                        LangName,
                                                        LangName2,
                                                        BallisticData,
                                                        BallisticData2,
                                                        LangRocketName,
                                                        RocketSpeed,
                                                        RocketArmorPower,
                                                        LangData2,
                                                        FontSize,
                                                        DistanceFactor
                                                        );
                                                    RocketName = RocketName.Split('/')[0];
                                                    if (RocketName.Contains("mm_"))
                                                    {
                                                        RocketName = RocketName.Remove(0, RocketName.IndexOf("mm_"));
                                                        RocketName = RocketName.Replace("mm_", "");
                                                    }
                                                    string TankPath = null;
                                                    if (file.Contains("_ModOptic"))
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                                        FileName += "ModOptic_";
                                                    }
                                                    else
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                                    }
                                                    FileName += BulletNameForBallistic;
                                                    if (HasRocket == true)
                                                    {
                                                        FileName += "_" + RocketName;
                                                    }
                                                    if (Directory.Exists(TankPath) == false)
                                                    {
                                                        Directory.CreateDirectory(TankPath);
                                                    }
                                                    FileName = TankPath + "//" + FileName + ".blk";
                                                    File.WriteAllText(FileName, data);
                                                    Application.DoEvents();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (HasRocket == false && HowitzerVersion == true && OnlyRocket == false && Speed < 500)
                                {
                                    Type = Type.Split('_')[0];
                                    if (Type == "he")
                                    {
                                        ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                    }
                                    if (BulletNameForBallistic.Contains("mm_"))
                                    {
                                        BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                        BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                    }
                                    if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12))
                                    {
                                        BallisticData = Ballistic(1, Type, BulletMass, Speed, BallisticCaliber, Cx, ExplosiveMass, DamageMass, DamageCaliber, demarrePenetrationK, demarreSpeedPow, demarreMassPow, demarreCaliberPow, ArmorPowerArray);
                                        string LangName = null;
                                        string LangName2 = null;
                                        string LangRocketName = null;
                                        FileName = "Tochka_SMH2_";
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
                                        StringReader reader1 = new StringReader(LangData);
                                        string line1 = String.Empty;
                                        while ((line1 = reader1.ReadLine()) != null)
                                        {
                                            if (line1.Contains("\"" + BulletName + "\""))
                                            {
                                                var parts = line1.Split(';');
                                                if (Pos < parts.Length)
                                                {
                                                    LangName = parts[Pos].Replace("\"", "");
                                                }
                                                else
                                                {
                                                    LangName = (parts.Length > 1 ? parts[1].Replace("\"", "") : BulletName.Split('/')[0]);
                                                }
                                            }
                                        }
                                        string SightType = "Howitzer";
                                        data = TochkaSM2.Create(
                                            SightType,
                                            Type,
                                            Type2,
                                            Speed,
                                            Speed2,
                                            ArmorPower,
                                            ArmorPower2,
                                            ZoomIn,
                                            ZoomOut,
                                            1,
                                            Language,
                                            DrawCrosshairDistShow,
                                            drawOuterLinesShow,
                                            BalisticInfoShow,
                                            SightNameShow,
                                            TankSizesShow,
                                            TargetLockShow,
                                            RangerFinderShow,
                                            DistanceShow,
                                            lineSizeMult,
                                            InnerDiameter,
                                            PointThickness,
                                            RangerFinderPos,
                                            DistancePos,
                                            DetectAllyPos,
                                            rangefinderProgressBarColor1,
                                            rangefinderProgressBarColor2,
                                            crosshairLightColor,
                                            Length,
                                            Height,
                                            Width,
                                            "real",
                                            10,
                                            LangName,
                                            LangName2,
                                            BallisticData,
                                            BallisticData2,
                                            LangRocketName,
                                            RocketSpeed,
                                            RocketArmorPower,
                                            LangData2,
                                            FontSize,
                                            DistanceFactor
                                            );
                                        string TankPath = null;
                                        if (file.Contains("_ModOptic"))
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                            FileName += "ModOptic_";
                                        }
                                        else
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                        }
                                        FileName += BulletNameForBallistic;
                                        if (Directory.Exists(TankPath) == false)
                                        {
                                            Directory.CreateDirectory(TankPath);
                                        }
                                        FileName = TankPath + "//" + FileName + ".blk";
                                        File.WriteAllText(FileName, data);
                                        Application.DoEvents();
                                    }
                                }
                            }
                        }
                    }
                }
                label1.Text = "File: ";
                label1.Refresh();
                progressBar1.Value = 0;
                IsRuning = false;
            }
            if (comboBox1.Text == "Luch")
            {
                string Dataminepath = textBox1.Text;
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                string Language = "English";
                if (comboBox2.Text == "Русский")
                {
                    Language = "Русский";
                }
                foreach (string file in file_list)
                {
                    label1.Text = Path.GetFileNameWithoutExtension(file);
                    label1.Refresh();
                    string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                    //if (Directory.Exists(TankPath2) == false)
                    {
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string Type = null;
                        double BallisticCaliber = 0;
                        double Speed = 0;
                        double ExplosiveMass = 0;
                        bool HasRocket = false;
                        string RocketName = null;
                        string RocketType = null;
                        double RocketSpeed = 0;
                        double RocketArmorPower = 0;
                        string ExplosiveType = null;
                        double ArmorPower = 0;
                        progressBar1.PerformStep();
                        string TankData = null;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }

                        string LangData = null;
                        StringReader reader = new StringReader(TankData);
                        string line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("RocketPath:"))
                            {
                                HasRocket = true;
                            }
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BallisticCaliber = 0;
                                Speed = 0;
                                ExplosiveMass = 0;
                                RocketName = null;
                                RocketType = null;
                                RocketArmorPower = 0;
                                RocketSpeed = 0;
                                ExplosiveType = null;
                                ArmorPower = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("ExplosiveMass:"))
                                    {
                                        ExplosiveMass = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("ExplosiveType:"))
                                    {
                                        ExplosiveType = line.Split(':')[1];
                                    }
                                    if (line.Contains("ArmorPower:"))
                                    {
                                        ArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                if (Type == "he")
                                {
                                    ArmorPower = HePenetration(ExplosiveMass, ExplosiveType);
                                }
                                if (HasRocket == true)
                                {
                                    StringReader reader2 = new StringReader(TankData);
                                    while ((line = reader2.ReadLine()) != null)
                                    {
                                        if (line.Contains("Name:"))
                                        {
                                            RocketName = line.Split(':')[1];
                                            RocketSpeed = 0;
                                            RocketType = null;
                                            while (((line = reader2.ReadLine()) != "") && (line != null))
                                            {
                                                if (line.StartsWith("Type:"))
                                                {
                                                    RocketType = line.Split(':')[1];
                                                    RocketType = RocketType.Split('_')[0];
                                                }
                                                if (line.Contains("Speed:"))
                                                {
                                                    RocketSpeed = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                                if (line.Contains("ArmorPower:"))
                                                {
                                                    RocketArmorPower = Convert.ToDouble(line.Split(':')[1]);
                                                }
                                            }
                                            if (RocketType == "atgm" || RocketType == "sam" || RocketType == "rocket" || RocketType == "aam")
                                            {
                                                string data = null;
                                                string FileName = null;
                                                string BallisticData = null;
                                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                                if (BulletNameForBallistic.Contains("mm_"))
                                                {
                                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                                }
                                                if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.1))
                                                {
                                                    if (textBox2.Text != "Ballistic path")
                                                    {
                                                        using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                                        {
                                                            BallisticData = sr.ReadToEnd();
                                                        }
                                                    }
                                                    string LangName = null;
                                                    string LangRocketName = null;
                                                    using (System.IO.StreamReader sr = new System.IO.StreamReader("Localization\\units_weaponry.csv"))
                                                    {
                                                        LangData = sr.ReadToEnd();
                                                    }
                                                    if (Language == "Русский")
                                                    {
                                                        FileName = "Luch_";
                                                        StringReader reader1 = new StringReader(LangData);
                                                        string line1 = String.Empty;
                                                        while ((line1 = reader1.ReadLine()) != null)
                                                        {
                                                            if (line1.Contains("\"" + BulletName + "\""))
                                                            {
                                                                LangName = line1.Split(';')[6];
                                                                LangName = LangName.Replace("\"", "");
                                                            }
                                                        }
                                                        if (HasRocket == true)
                                                        {
                                                            StringReader reader3 = new StringReader(LangData);
                                                            line1 = String.Empty;
                                                            while ((line1 = reader3.ReadLine()) != null)
                                                            {
                                                                if (line1.Contains("\"" + RocketName + "\""))
                                                                {
                                                                    LangRocketName = line1.Split(';')[6];
                                                                    LangRocketName = LangRocketName.Replace("\"", "");
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (Language == "English")
                                                    {
                                                        FileName = "Luch_";
                                                        StringReader reader1 = new StringReader(LangData);
                                                        string line1 = String.Empty;
                                                        while ((line1 = reader1.ReadLine()) != null)
                                                        {
                                                            if (line1.Contains("\"" + BulletName + "\""))
                                                            {
                                                                LangName = line1.Split(';')[1];
                                                                LangName = LangName.Replace("\"", "");
                                                            }
                                                        }
                                                        if (HasRocket == true)
                                                        {
                                                            StringReader reader3 = new StringReader(LangData);
                                                            line1 = String.Empty;
                                                            while ((line1 = reader3.ReadLine()) != null)
                                                            {
                                                                if (line1.Contains("\"" + RocketName + "\""))
                                                                {
                                                                    LangRocketName = line1.Split(';')[6];
                                                                    LangRocketName = LangRocketName.Replace("\"", "");
                                                                }
                                                            }
                                                        }
                                                    }
                                                    data = Luch.Create(Type, Speed, ArmorPower, ZoomIn, ZoomOut, Sensivity, Language, true, 4, 2, true, "0, 255, 0, 64", "255, 255, 255, 64", "255, 0, 0, 255", 1.5, 6.5, 2.4, 2, "real", 10, LangName, BallisticData, LangRocketName, RocketSpeed, RocketArmorPower);
                                                    BulletName = BulletName.Split('/')[0];
                                                    if (BulletName.Contains("mm_"))
                                                    {
                                                        BulletName = BulletName.Remove(0, BulletName.IndexOf("mm_"));
                                                        BulletName = BulletName.Replace("mm_", "");
                                                    }
                                                    RocketName = RocketName.Split('/')[0];
                                                    if (RocketName.Contains("mm_"))
                                                    {
                                                        RocketName = RocketName.Remove(0, RocketName.IndexOf("mm_"));
                                                        RocketName = RocketName.Replace("mm_", "");
                                                    }
                                                    string TankPath = null;
                                                    if (file.Contains("_ModOptic"))
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                                        FileName += "ModOptic_";
                                                    }
                                                    else
                                                    {
                                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                                    }
                                                    FileName += BulletName + "_" + RocketName;
                                                    if (Directory.Exists(TankPath) == false)
                                                    {
                                                        Directory.CreateDirectory(TankPath);
                                                    }
                                                    FileName = TankPath + "//" + "FCS_" + FileName + ".blk";
                                                    File.WriteAllText(FileName, data);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string data = null;
                                    string FileName = null;
                                    string BallisticData = null;
                                    string BulletNameForBallistic = BulletName.Split('/')[0];
                                    if (BulletNameForBallistic.Contains("mm_"))
                                    {
                                        BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                        BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                    }
                                    if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.1))
                                    {
                                        if (textBox2.Text != "Ballistic path")
                                        {
                                            using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                            {
                                                BallisticData = sr.ReadToEnd();
                                            }
                                        }
                                        string LangName = null;
                                        string LangRocketName = null;
                                        using (System.IO.StreamReader sr = new System.IO.StreamReader("Localization\\units_weaponry.csv"))
                                        {
                                            LangData = sr.ReadToEnd();
                                        }
                                        if (Language == "Русский")
                                        {
                                            FileName = "Luch_";
                                            StringReader reader1 = new StringReader(LangData);
                                            string line1 = String.Empty;
                                            while ((line1 = reader1.ReadLine()) != null)
                                            {
                                                if (line1.Contains("\"" + BulletName + "\""))
                                                {
                                                    LangName = line1.Split(';')[6];
                                                    LangName = LangName.Replace("\"", "");
                                                }
                                            }
                                        }
                                        if (Language == "English")
                                        {
                                            FileName = "Luch_";
                                            StringReader reader1 = new StringReader(LangData);
                                            string line1 = String.Empty;
                                            while ((line1 = reader1.ReadLine()) != null)
                                            {
                                                if (line1.Contains("\"" + BulletName + "\""))
                                                {
                                                    LangName = line1.Split(';')[1];
                                                    LangName = LangName.Replace("\"", "");
                                                }
                                            }
                                        }
                                        data = Luch.Create(Type, Speed, ArmorPower, ZoomIn, ZoomOut, Sensivity, Language, true, 4, 2, true, "0, 255, 0, 64", "255, 255, 255, 64", "255, 0, 0, 255", 1.5, 6.5, 2.4, 2, "real", 10, LangName, BallisticData, LangRocketName, RocketSpeed, RocketArmorPower);
                                        BulletName = BulletName.Split('/')[0];
                                        if (BulletName.Contains("mm_"))
                                        {
                                            BulletName = BulletName.Remove(0, BulletName.IndexOf("mm_"));
                                            BulletName = BulletName.Replace("mm_", "");
                                        }
                                        string TankPath = null;
                                        if (file.Contains("_ModOptic"))
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                            FileName += "ModOptic_";
                                        }
                                        else
                                        {
                                            TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                        }
                                        FileName += BulletName;
                                        if (Directory.Exists(TankPath) == false)
                                        {
                                            Directory.CreateDirectory(TankPath);
                                        }
                                        FileName = TankPath + "//" + "FCS_" + FileName + ".blk";
                                        File.WriteAllText(FileName, data);
                                    }
                                }
                            }
                        }
                    }
                    /*else
                    {
                        progressBar1.PerformStep();
                    }*/
                }
                label1.Text = "";
                label1.Refresh();
                progressBar1.Value = 0;
            }
            if (comboBox1.Text == "Luch Lite")
            {
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                foreach (string file in file_list)
                {
                    string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                    //if (Directory.Exists(TankPath2) == false)
                    {
                        label1.Text = Path.GetFileNameWithoutExtension(file);
                        label1.Refresh();
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string Type = null;
                        double BulletMass = 0;
                        double BallisticCaliber = 0;
                        double Speed = 0;
                        double Cx = 0;
                        progressBar1.PerformStep();
                        string TankData = null;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }
                        StringReader reader = new StringReader(TankData);
                        string line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BulletMass = 0;
                                BallisticCaliber = 0;
                                Speed = 0;
                                Cx = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BulletMass:"))
                                    {
                                        BulletMass = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Cx:"))
                                    {
                                        Cx = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                string data = null;
                                string FileName = null;
                                string BallisticData = null;
                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                if (BulletNameForBallistic.Contains("mm_"))
                                {
                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                }
                                // Exclude napalm-based rounds from sight generation (legacy crash fix for Luch_Lite)
                                if ((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice" && Type != "napalm" && Type != "napalm_gel") || (Type == "he" && BallisticCaliber >= 0.1))
                                {
                                    if (textBox2.Text != "Ballistic path")
                                    {
                                        using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                        {
                                            BallisticData = sr.ReadToEnd();
                                        }
                                    }
                                    FileName = "Luch_Lite_";
                                    data = Luch_Lite.Create(Speed, ZoomIn, ZoomOut, Sensivity, true, 3, 1.5, true, "0, 255, 0, 64", "255, 255, 255, 64", "255, 0, 0, 255", 1.5, 6.5, 2.4, 5, "real", 10, BallisticData);
                                    BulletName = BulletName.Split('/')[0];
                                    if (BulletName.Contains("mm_"))
                                    {
                                        BulletName = BulletName.Remove(0, BulletName.IndexOf("mm_"));
                                        BulletName = BulletName.Replace("mm_", "");
                                    }
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletName;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + "FCS_" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                }
                            }
                        }
                    }
                    /*else
                    {
                        progressBar1.PerformStep();
                    }*/
                }
                label1.Text = "";
                label1.Refresh();
                progressBar1.Value = 0;
            }
            if (comboBox1.Text == "Duga")
            {
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                bool BaseVersion = false;
                bool RocketVersion = false;
                bool DrawDisnaceCorrections = false;
                string crosshairLightColor = Convert.ToString(pictureBox1.BackColor.R) + "," + Convert.ToString(pictureBox1.BackColor.G) + "," + Convert.ToString(pictureBox1.BackColor.B) + ",255";
                string rangefinderProgressBarColor1 = Convert.ToString(pictureBox2.BackColor.R) + "," + Convert.ToString(pictureBox2.BackColor.G) + "," + Convert.ToString(pictureBox2.BackColor.B) + ",64";
                string rangefinderProgressBarColor2 = Convert.ToString(pictureBox3.BackColor.R) + "," + Convert.ToString(pictureBox3.BackColor.G) + "," + Convert.ToString(pictureBox3.BackColor.B) + ",64";
                double lineSizeMult = Convert.ToDouble(trackBar2.Value) / 10;
                double InnerDiameter = Convert.ToDouble(trackBar4.Value) / 10;
                double PointThickness = Convert.ToDouble(trackBar3.Value) / 10;
                double[] RangerFinderPos = { Convert.ToDouble(textBox10.Text.Split(',')[0]), Convert.ToDouble(textBox10.Text.Split(',')[1]) };
                double[] DetectAllyPos = { Convert.ToDouble(textBox8.Text.Split(',')[0]), Convert.ToDouble(textBox8.Text.Split(',')[1]) };
                double[] DistancePos = { Convert.ToDouble(textBox9.Text.Split(',')[0]), Convert.ToDouble(textBox9.Text.Split(',')[1]) };
                double Length = Convert.ToDouble(textBox5.Text);
                double Width = Convert.ToDouble(textBox7.Text);
                double FontSize = Convert.ToDouble(trackBar5.Value) / 20;
                double DistanceFactor = Convert.ToDouble(trackBar6.Value) / 20;
                foreach (object itemChecked in checkedListBox1.CheckedItems)
                {
                    if (itemChecked.ToString().Contains("Base Version").Equals(true))
                    {
                        BaseVersion = true;
                    }
                    if (itemChecked.ToString().Contains("For SAM, AAM, ATGM").Equals(true))
                    {
                        RocketVersion = true;
                    }
                }
                foreach (string file in file_list)
                {
                    progressBar1.PerformStep();
                    string Country = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                    bool MakeSight = false;
                    foreach (object itemChecked in checkedListBox2.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("USA").Equals(true) && Country == "us")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Germany").Equals(true) && Country == "germ")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("USSR").Equals(true) && Country == "ussr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Britain").Equals(true) && Country == "uk")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Japan").Equals(true) && Country == "jp")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("China").Equals(true) && Country == "cn")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Italy").Equals(true) && Country == "it")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("France").Equals(true) && Country == "fr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Sweden").Equals(true) && Country == "sw")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Israel").Equals(true) && Country == "il")
                        {
                            MakeSight = true;
                        }
                    }
                    foreach (object itemChecked in checkedListBox3.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("Draw Distance Corrections").Equals(true))
                        {
                            DrawDisnaceCorrections = true;
                        }
                    }
                    if (MakeSight == true && checkedListBox1.CheckedItems.Count > 0)
                    {
                        label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                        label1.Refresh();
                        string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string Type = null;
                        string TankData = null;
                        double BallisticCaliber = 0;
                        double Speed = 0;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }
                        StringReader readertest = new StringReader(TankData);
                        bool OnlyRocket = true;
                        string line = String.Empty;
                        while ((line = readertest.ReadLine()) != null)
                        {
                            if (line.StartsWith("Type:"))
                            {
                                Type = line.Split(':')[1];
                                Type = Type.Split('_')[0];
                                if (Type != "atgm" && Type != "sam" && Type != "aam")
                                {
                                    OnlyRocket = false;
                                }
                            }
                        }
                        StringReader reader = new StringReader(TankData);
                        line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BallisticCaliber = 0;
                                Speed = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                string data = null;
                                string FileName = null;
                                string BallisticData = null;
                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                if (BulletNameForBallistic.Contains("mm_"))
                                {
                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                }
                                if (((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12)) && BaseVersion == true && OnlyRocket == false)
                                {
                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                    {
                                        BallisticData = sr.ReadToEnd();
                                    }
                                    FileName = "Duga_";
                                    string SightType = "Base";
                                    data = Duga.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                                if ((Type == "sam" || Type == "atgm" || Type == "aam") && OnlyRocket == true && RocketVersion == true)
                                {
                                    FileName = "Duga_";
                                    string SightType = "Rocket";
                                    data = Duga.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                }
                label1.Text = "File: ";
                label1.Refresh();
                progressBar1.Value = 0;
                IsRuning = false;
            }
            if (comboBox1.Text == "Duga-2")
            {
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                bool BaseVersion = false;
                bool RocketVersion = false;
                bool DrawVerticalLines = false;
                bool DrawDisnaceCorrections = false;
                string crosshairLightColor = Convert.ToString(pictureBox1.BackColor.R) + "," + Convert.ToString(pictureBox1.BackColor.G) + "," + Convert.ToString(pictureBox1.BackColor.B) + ",255";
                string rangefinderProgressBarColor1 = Convert.ToString(pictureBox2.BackColor.R) + "," + Convert.ToString(pictureBox2.BackColor.G) + "," + Convert.ToString(pictureBox2.BackColor.B) + ",64";
                string rangefinderProgressBarColor2 = Convert.ToString(pictureBox3.BackColor.R) + "," + Convert.ToString(pictureBox3.BackColor.G) + "," + Convert.ToString(pictureBox3.BackColor.B) + ",64";
                double lineSizeMult = Convert.ToDouble(trackBar2.Value) / 10;
                double InnerDiameter = Convert.ToDouble(trackBar4.Value) / 10;
                double PointThickness = Convert.ToDouble(trackBar3.Value) / 10;
                double[] RangerFinderPos = { Convert.ToDouble(textBox10.Text.Split(',')[0]), Convert.ToDouble(textBox10.Text.Split(',')[1]) };
                double[] DetectAllyPos = { Convert.ToDouble(textBox8.Text.Split(',')[0]), Convert.ToDouble(textBox8.Text.Split(',')[1]) };
                double[] DistancePos = { Convert.ToDouble(textBox9.Text.Split(',')[0]), Convert.ToDouble(textBox9.Text.Split(',')[1]) };
                double Length = Convert.ToDouble(textBox5.Text);
                double Width = Convert.ToDouble(textBox7.Text);
                double FontSize = Convert.ToDouble(trackBar5.Value) / 20;
                double DistanceFactor = Convert.ToDouble(trackBar6.Value) / 20;
                foreach (object itemChecked in checkedListBox1.CheckedItems)
                {
                    if (itemChecked.ToString().Contains("Base Version").Equals(true))
                    {
                        BaseVersion = true;
                    }
                    if (itemChecked.ToString().Contains("For SAM, AAM, ATGM").Equals(true))
                    {
                        RocketVersion = true;
                    }
                }
                foreach (string file in file_list)
                {
                    progressBar1.PerformStep();
                    string Country = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                    bool MakeSight = false;
                    foreach (object itemChecked in checkedListBox2.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("USA").Equals(true) && Country == "us")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Germany").Equals(true) && Country == "germ")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("USSR").Equals(true) && Country == "ussr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Britain").Equals(true) && Country == "uk")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Japan").Equals(true) && Country == "jp")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("China").Equals(true) && Country == "cn")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Italy").Equals(true) && Country == "it")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("France").Equals(true) && Country == "fr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Sweden").Equals(true) && Country == "sw")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Israel").Equals(true) && Country == "il")
                        {
                            MakeSight = true;
                        }
                    }
                    foreach (object itemChecked in checkedListBox3.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("Draw Vertical Lines").Equals(true))
                        {
                            DrawVerticalLines = true;
                        }
                        if (itemChecked.ToString().Contains("Draw Distance Corrections").Equals(true))
                        {
                            DrawDisnaceCorrections = true;
                        }
                    }
                    if (MakeSight == true && checkedListBox1.CheckedItems.Count > 0)
                    {
                        label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                        label1.Refresh();
                        string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string Type = null;
                        string TankData = null;
                        double BallisticCaliber = 0;
                        double Speed = 0;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }
                        StringReader readertest = new StringReader(TankData);
                        bool OnlyRocket = true;
                        string line = String.Empty;
                        while ((line = readertest.ReadLine()) != null)
                        {
                            if (line.StartsWith("Type:"))
                            {
                                Type = line.Split(':')[1];
                                Type = Type.Split('_')[0];
                                if (Type != "atgm" && Type != "sam" && Type != "aam")
                                {
                                    OnlyRocket = false;
                                }
                            }
                        }
                        StringReader reader = new StringReader(TankData);
                        line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BallisticCaliber = 0;
                                Speed = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                string data = null;
                                string FileName = null;
                                string BallisticData = null;
                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                if (BulletNameForBallistic.Contains("mm_"))
                                {
                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                }
                                if (((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12)) && BaseVersion == true && OnlyRocket == false)
                                {
                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                    {
                                        BallisticData = sr.ReadToEnd();
                                    }
                                    FileName = "Duga2_";
                                    string SightType = "Base";
                                    data = Duga2.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawVerticalLines,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                                if ((Type == "sam" || Type == "atgm" || Type == "aam") && OnlyRocket == true && RocketVersion == true)
                                {
                                    FileName = "Duga2_";
                                    string SightType = "Rocket";
                                    data = Duga2.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawVerticalLines,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                }
                label1.Text = "File: ";
                label1.Refresh();
                progressBar1.Value = 0;
                IsRuning = false;
            }
            if (comboBox1.Text == "Sector")
            {
                string[] file_list = Directory.GetFiles(textBox3.Text, "*.txt");
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = file_list.Length;
                progressBar1.Step = 1;
                bool BaseVersion = false;
                bool RocketVersion = false;
                bool DrawDisnaceCorrections = false;
                string crosshairLightColor = Convert.ToString(pictureBox1.BackColor.R) + "," + Convert.ToString(pictureBox1.BackColor.G) + "," + Convert.ToString(pictureBox1.BackColor.B) + ",255";
                string rangefinderProgressBarColor1 = Convert.ToString(pictureBox2.BackColor.R) + "," + Convert.ToString(pictureBox2.BackColor.G) + "," + Convert.ToString(pictureBox2.BackColor.B) + ",64";
                string rangefinderProgressBarColor2 = Convert.ToString(pictureBox3.BackColor.R) + "," + Convert.ToString(pictureBox3.BackColor.G) + "," + Convert.ToString(pictureBox3.BackColor.B) + ",64";
                double lineSizeMult = Convert.ToDouble(trackBar2.Value) / 10;
                double InnerDiameter = Convert.ToDouble(trackBar4.Value) / 10;
                double PointThickness = Convert.ToDouble(trackBar3.Value) / 10;
                double[] RangerFinderPos = { Convert.ToDouble(textBox10.Text.Split(',')[0]), Convert.ToDouble(textBox10.Text.Split(',')[1]) };
                double[] DetectAllyPos = { Convert.ToDouble(textBox8.Text.Split(',')[0]), Convert.ToDouble(textBox8.Text.Split(',')[1]) };
                double[] DistancePos = { Convert.ToDouble(textBox9.Text.Split(',')[0]), Convert.ToDouble(textBox9.Text.Split(',')[1]) };
                double Length = Convert.ToDouble(textBox5.Text);
                double Width = Convert.ToDouble(textBox7.Text);
                double FontSize = Convert.ToDouble(trackBar5.Value) / 20;
                double DistanceFactor = Convert.ToDouble(trackBar6.Value) / 20;
                foreach (object itemChecked in checkedListBox1.CheckedItems)
                {
                    if (itemChecked.ToString().Contains("Base Version").Equals(true))
                    {
                        BaseVersion = true;
                    }
                    if (itemChecked.ToString().Contains("For SAM, AAM, ATGM").Equals(true))
                    {
                        RocketVersion = true;
                    }
                }
                foreach (string file in file_list)
                {
                    progressBar1.PerformStep();
                    string Country = Path.GetFileNameWithoutExtension(file).Split('_')[0];
                    bool MakeSight = false;
                    foreach (object itemChecked in checkedListBox2.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("USA").Equals(true) && Country == "us")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Germany").Equals(true) && Country == "germ")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("USSR").Equals(true) && Country == "ussr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Britain").Equals(true) && Country == "uk")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Japan").Equals(true) && Country == "jp")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("China").Equals(true) && Country == "cn")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Italy").Equals(true) && Country == "it")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("France").Equals(true) && Country == "fr")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Sweden").Equals(true) && Country == "sw")
                        {
                            MakeSight = true;
                        }
                        if (itemChecked.ToString().Contains("Israel").Equals(true) && Country == "il")
                        {
                            MakeSight = true;
                        }
                    }
                    foreach (object itemChecked in checkedListBox3.CheckedItems)
                    {
                        if (itemChecked.ToString().Contains("Draw Distance Corrections").Equals(true))
                        {
                            DrawDisnaceCorrections = true;
                        }
                    }
                    if (MakeSight == true && checkedListBox1.CheckedItems.Count > 0)
                    {
                        label1.Text = "File: " + Path.GetFileNameWithoutExtension(file);
                        label1.Refresh();
                        string TankPath2 = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                        double ZoomIn = 0;
                        double ZoomOut = 0;
                        string BulletName = null;
                        string Type = null;
                        bool ForAA = false;
                        string TankData = null;
                        double BallisticCaliber = 0;
                        double Speed = 0;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                        {
                            TankData = sr.ReadToEnd();
                        }
                        StringReader readertest = new StringReader(TankData);
                        bool OnlyRocket = true;
                        string line = String.Empty;
                        while ((line = readertest.ReadLine()) != null)
                        {
                            if (line.StartsWith("Type:"))
                            {
                                Type = line.Split(':')[1];
                                Type = Type.Split('_')[0];
                                if (Type != "atgm" && Type != "sam" && Type != "aam")
                                {
                                    OnlyRocket = false;
                                }
                                if (!line.Contains("tank"))
                                {
                                    ForAA = true;
                                }
                            }
                        }
                        StringReader reader = new StringReader(TankData);
                        line = String.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("ZoomIn:"))
                            {
                                ZoomIn = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("ZoomOut:"))
                            {
                                ZoomOut = Convert.ToDouble(line.Split(':')[1]);
                            }
                            if (line.Contains("Name:"))
                            {
                                BulletName = line.Split(':')[1];
                                Type = null;
                                BallisticCaliber = 0;
                                Speed = 0;
                                while (((line = reader.ReadLine()) != "") && (line != null))
                                {
                                    if (line.StartsWith("Type:"))
                                    {
                                        Type = line.Split(':')[1];
                                        Type = Type.Split('_')[0];
                                    }
                                    if (line.Contains("BallisticCaliber:"))
                                    {
                                        BallisticCaliber = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                    if (line.Contains("Speed:"))
                                    {
                                        Speed = Convert.ToDouble(line.Split(':')[1]);
                                    }
                                }
                                string data = null;
                                string FileName = null;
                                string BallisticData = null;
                                string BulletNameForBallistic = BulletName.Split('/')[0];
                                if (BulletNameForBallistic.Contains("mm_"))
                                {
                                    BulletNameForBallistic = BulletNameForBallistic.Remove(0, BulletNameForBallistic.IndexOf("mm_"));
                                    BulletNameForBallistic = BulletNameForBallistic.Replace("mm_", "");
                                }
                                if (((Type != "sam" && Type != "atgm" && Type != "rocket" && Type != "aam" && Type != "smoke" && Type != "shrapnel" && Type != "he" && Type != "practice") || (Type == "he" && BallisticCaliber >= 0.12)) && BaseVersion == true && OnlyRocket == false && ForAA == true)
                                {
                                    using (System.IO.StreamReader sr = new System.IO.StreamReader(textBox2.Text + "//" + Path.GetFileNameWithoutExtension(file) + "//" + BulletNameForBallistic + ".txt"))
                                    {
                                        BallisticData = sr.ReadToEnd();
                                    }
                                    FileName = "Sector_";
                                    string SightType = "Base";
                                    data = Sector.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                                if ((Type == "sam" || Type == "atgm" || Type == "aam") && OnlyRocket == true && RocketVersion == true)
                                {
                                    FileName = "Sector_";
                                    string SightType = "Rocket";
                                    data = Sector.Create(
                                        SightType,
                                        Speed,
                                        ZoomIn,
                                        ZoomOut,
                                        Sensivity,
                                        lineSizeMult,
                                        InnerDiameter,
                                        PointThickness,
                                        RangerFinderPos,
                                        DetectAllyPos,
                                        DistancePos,
                                        DrawDisnaceCorrections,
                                        rangefinderProgressBarColor1,
                                        rangefinderProgressBarColor2,
                                        crosshairLightColor,
                                        Length,
                                        Width,
                                        "real",
                                        10,
                                        BallisticData,
                                        FontSize,
                                        DistanceFactor
                                        );
                                    string TankPath = null;
                                    if (file.Contains("_ModOptic"))
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file).Replace("_ModOptic", "");
                                        FileName += "ModOptic_";
                                    }
                                    else
                                    {
                                        TankPath = textBox4.Text + "//" + Path.GetFileNameWithoutExtension(file);
                                    }
                                    FileName += BulletNameForBallistic;
                                    if (Directory.Exists(TankPath) == false)
                                    {
                                        Directory.CreateDirectory(TankPath);
                                    }
                                    FileName = TankPath + "//" + FileName + ".blk";
                                    File.WriteAllText(FileName, data);
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                }
                label1.Text = "File: ";
                label1.Refresh();
                progressBar1.Value = 0;
                IsRuning = false;
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Tochka-SM2")
            {
                checkedListBox1.Items.Clear();
                checkedListBox2.Items.Clear();
                checkedListBox3.Items.Clear();

                checkedListBox1.Items.Add("Tochka-SM2 (Base Version)", true);
                checkedListBox1.Items.Add("Tochka-SMD2 (Double Shells)", true);
                checkedListBox1.Items.Add("Tochka-SML2 (For Laser Rangefinders)", true);
                checkedListBox1.Items.Add("Tochka-SMR2 (For SAM, AAM, ATGM)", true);
                checkedListBox1.Items.Add("Tochka-SMH2 (For Howitzers)", true);

                checkedListBox2.Items.Add("Select all", true);
                checkedListBox2.Items.Add("USA", true);
                checkedListBox2.Items.Add("Germany", true);
                checkedListBox2.Items.Add("USSR", true);
                checkedListBox2.Items.Add("Britain", true);
                checkedListBox2.Items.Add("Japan", true);
                checkedListBox2.Items.Add("China", true);
                checkedListBox2.Items.Add("Italy", true);
                checkedListBox2.Items.Add("France", true);
                checkedListBox2.Items.Add("Sweden", true);
                checkedListBox2.Items.Add("Israel", true);

                checkedListBox3.Items.Add("Range Correction Notches", true);
                checkedListBox3.Items.Add("Outer Lines", true);
                checkedListBox3.Items.Add("Ballistic Info", true);
                checkedListBox3.Items.Add("Sight Name", true);
                checkedListBox3.Items.Add("Tank Sizes", true);
                checkedListBox3.Items.Add("\"Target Lock: ON\" Text", true);
                checkedListBox3.Items.Add("\"Rangefinder\" Text", true);
                checkedListBox3.Items.Add("\"Distance\" Text", true);

                textBox10.Text = "250, 0.2";
                textBox9.Text = "70.5, 47";
                textBox8.Text = "-345, 0.2";
                trackBar1.Value = 50;

                textBox6.Visible = true;
                label9.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                trackBar1.Visible = true;
            }
            if (comboBox1.Text == "Duga")
            {
                checkedListBox1.Items.Clear();
                checkedListBox2.Items.Clear();
                checkedListBox3.Items.Clear();

                checkedListBox1.Items.Add("Base Version", true);
                checkedListBox1.Items.Add("For SAM, AAM, ATGM", true);

                checkedListBox2.Items.Add("Select all", true);
                checkedListBox2.Items.Add("USA", true);
                checkedListBox2.Items.Add("Germany", true);
                checkedListBox2.Items.Add("USSR", true);
                checkedListBox2.Items.Add("Britain", true);
                checkedListBox2.Items.Add("Japan", true);
                checkedListBox2.Items.Add("China", true);
                checkedListBox2.Items.Add("Italy", true);
                checkedListBox2.Items.Add("France", true);
                checkedListBox2.Items.Add("Sweden", true);
                checkedListBox2.Items.Add("Israel", true);

                checkedListBox3.Items.Add("Draw Distance Corrections", true);

                textBox10.Text = "120, 0.01";
                textBox9.Text = "0.05, 0.05";
                textBox8.Text = "120, -0.01";
                trackBar1.Value = 50;

                textBox6.Visible = false;
                label9.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                trackBar1.Visible = false;
            }
            if (comboBox1.Text == "Duga-2")
            {
                checkedListBox1.Items.Clear();
                checkedListBox2.Items.Clear();
                checkedListBox3.Items.Clear();

                checkedListBox1.Items.Add("Base Version", true);
                checkedListBox1.Items.Add("For SAM, AAM, ATGM", true);

                checkedListBox2.Items.Add("Select all", true);
                checkedListBox2.Items.Add("USA", true);
                checkedListBox2.Items.Add("Germany", true);
                checkedListBox2.Items.Add("USSR", true);
                checkedListBox2.Items.Add("Britain", true);
                checkedListBox2.Items.Add("Japan", true);
                checkedListBox2.Items.Add("China", true);
                checkedListBox2.Items.Add("Italy", true);
                checkedListBox2.Items.Add("France", true);
                checkedListBox2.Items.Add("Sweden", true);
                checkedListBox2.Items.Add("Israel", true);

                checkedListBox3.Items.Add("Draw Vertical Lines", true);
                checkedListBox3.Items.Add("Draw Distance Corrections", true);

                textBox10.Text = "120, 0.01";
                textBox9.Text = "0.05, 0.05";
                textBox8.Text = "120, -0.01";
                trackBar1.Value = 50;

                textBox6.Visible = false;
                label9.Visible = false;
                label2.Visible = true;
                label3.Visible = true;
                trackBar1.Visible = true;
            }
            if (comboBox1.Text == "Sector")
            {
                checkedListBox1.Items.Clear();
                checkedListBox2.Items.Clear();
                checkedListBox3.Items.Clear();

                checkedListBox1.Items.Add("Base Version", true);
                checkedListBox1.Items.Add("For SAM, AAM, ATGM", true);

                checkedListBox2.Items.Add("Select all", true);
                checkedListBox2.Items.Add("USA", true);
                checkedListBox2.Items.Add("Germany", true);
                checkedListBox2.Items.Add("USSR", true);
                checkedListBox2.Items.Add("Britain", true);
                checkedListBox2.Items.Add("Japan", true);
                checkedListBox2.Items.Add("China", true);
                checkedListBox2.Items.Add("Italy", true);
                checkedListBox2.Items.Add("France", true);
                checkedListBox2.Items.Add("Sweden", true);
                checkedListBox2.Items.Add("Israel", true);

                checkedListBox3.Items.Add("Draw Distance Corrections", true);

                textBox10.Text = "120, 0.01";
                textBox9.Text = "0.05, 0.05";
                textBox8.Text = "120, -0.01";
                trackBar1.Value = 50;

                textBox6.Visible = false;
                label9.Visible = false;
                label2.Visible = true;
                label3.Visible = true;
                trackBar1.Visible = true;
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            label3.Text = trackBar1.Value.ToString() + "%";
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label6.Text = Convert.ToString(Convert.ToDouble(trackBar2.Value) / 10);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            label11.Text = Convert.ToString(Convert.ToDouble(trackBar3.Value) / 10);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            label13.Text = Convert.ToString(Convert.ToDouble(trackBar4.Value) / 10);
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            label20.Text = Convert.ToString(Convert.ToDouble(trackBar5.Value) / 20);
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            label22.Text = Convert.ToString(Convert.ToDouble(trackBar6.Value) / 20);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.BackColor = colorDialog1.Color;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.BackColor = colorDialog1.Color;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.BackColor = colorDialog1.Color;
            }
        }

        private void groupBox1_Click(object sender, EventArgs e)
        {
        }
    }
}
