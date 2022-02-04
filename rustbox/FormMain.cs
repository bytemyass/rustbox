using rustbox.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static rustbox.Features.UpdateLoop;

namespace rustbox
{
    public partial class FormMain : Form
    {
        public int selectedChams = 0;
        public bool needRewrite = false;
        public int selectedFilter = 0;
        public FormMain()
        {
            InitializeComponent();

            UpdateLoop.formMain = this;
            DMAController.formMain = this;

            comboBoxChams.SelectedIndex = 0;

            SetupRecoilDict();
        }

        public void SetupRecoilDict()
        {
            UpdateLoop.weaponDict.Add(1545779598, new Weapon(8, -30, 0.5f, 0.5f)); //ak
            UpdateLoop.weaponDict.Add(1796682209, new Weapon(10, -15, 0.5f, 0.5f)); //custom
            UpdateLoop.weaponDict.Add(2482412119, new Weapon(5, -12, 0.5f, 0.5f)); //lr
            UpdateLoop.weaponDict.Add(1318558775, new Weapon(6, -10, 0.5f, 0.5f)); //mp5
            UpdateLoop.weaponDict.Add(2536594571, new Weapon(10, -15, 0.5f, 0.5f)); //thompson

            try
            {
                if (File.Exists(Environment.CurrentDirectory + "\\recoilSave.cfg"))
                {
                    var weapons = File.ReadAllLines(Environment.CurrentDirectory + "\\recoilSave.cfg");

                    foreach (var weapon in weapons)
                    {
                        var vals = weapon.Split(' ');

                        uint weaponID = Convert.ToUInt32(vals[0]);
                        float pitchMultiplier = (float)Convert.ToDouble(vals[1]);
                        float yawMultiplier = (float)Convert.ToDouble(vals[2]);

                        Weapon dictWeapon;
                        if (weaponDict.TryGetValue(weaponID, out dictWeapon))
                        {
                            weaponDict.Remove(weaponID);
                            dictWeapon.multiplierPitch = pitchMultiplier;
                            dictWeapon.multiplierYaw = yawMultiplier;
                            weaponDict.Add(weaponID, dictWeapon);

                            switch (weaponID)
                            {
                                case 1545779598:
                                    trackBarAkPitch.Value = (int)(pitchMultiplier * 100);
                                    trackBarAkYaw.Value = (int)(yawMultiplier * 100);
                                    labelAkPitch.Text = (int)(pitchMultiplier * 100) + " %";
                                    labelAkYaw.Text = (int)(yawMultiplier * 100) + " %";
                                    break;
                                case 1796682209:
                                    trackBarCustomSMGPitch.Value = (int)(pitchMultiplier * 100);
                                    trackBarCustomSMGYaw.Value = (int)(yawMultiplier * 100);
                                    labelCustomSMGPitch.Text = (int)(pitchMultiplier * 100) + " %";
                                    labelCustomSMGYaw.Text = (int)(yawMultiplier * 100) + " %";
                                    break;
                                case 2482412119:
                                    trackBarLR300Pitch.Value = (int)(pitchMultiplier * 100);
                                    trackBarLR300Yaw.Value = (int)(yawMultiplier * 100);
                                    labelLR300Pitch.Text = (int)(pitchMultiplier * 100) + " %";
                                    labelLR300Yaw.Text = (int)(yawMultiplier * 100) + " %";
                                    break;
                                case 1318558775:
                                    trackBarlabelMP5Pitch.Value = (int)(pitchMultiplier * 100);
                                    trackBarlabelMP5Yaw.Value = (int)(yawMultiplier * 100);
                                    labelMP5Pitch.Text = (int)(pitchMultiplier * 100) + " %";
                                    labelMP5Yaw.Text = (int)(yawMultiplier * 100) + " %";
                                    break;
                                case 2536594571:
                                    trackBarThompsonPitch.Value = (int)(pitchMultiplier * 100);
                                    trackBarThompsonYaw.Value = (int)(yawMultiplier * 100);
                                    labelThompsonPitch.Text = (int)(pitchMultiplier * 100) + " %";
                                    labelThompsonYaw.Text = (int)(yawMultiplier * 100) + " %";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not read recoil config.");
            }
        }

        private void buttonAttach_Click(object sender, EventArgs e)
        {
            this.buttonAttach.Enabled = false;
            new Task(DMAController.Attach).Start();
        }

        private void buttonDetach_Click(object sender, EventArgs e)
        {
            buttonDetach.Enabled = false;
            DMAController.Cleanup();
            buttonAttach.Enabled = true;
            labelStatus.Text = "Detached";
            labelStatus.ForeColor = Color.Red;
        }

        private void checkBoxAdminflag_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.adminFlag = checkBoxAdminflag.Checked;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DMAController.Cleanup();
        }

        private void checkBoxNoRecoil_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.noRecoil = checkBoxNoRecoil.Checked;
        }

        private void trackBarCustomSMGPitch_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1796682209, out weapon))
            {
                weaponDict.Remove(1796682209);
                weapon.multiplierPitch = (float)trackBarCustomSMGPitch.Value / 100;
                labelCustomSMGPitch.Text = trackBarCustomSMGPitch.Value.ToString() + " %";
                weaponDict.Add(1796682209, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarCustomSMGYaw_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1796682209, out weapon))
            {
                weaponDict.Remove(1796682209);
                weapon.multiplierYaw = (float)trackBarCustomSMGYaw.Value / 100;
                labelCustomSMGYaw.Text = trackBarCustomSMGYaw.Value.ToString() + " %";
                weaponDict.Add(1796682209, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarlabelMP5Pitch_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1318558775, out weapon))
            {
                weaponDict.Remove(1318558775);
                weapon.multiplierPitch = (float)trackBarlabelMP5Pitch.Value / 100;
                labelMP5Pitch.Text = trackBarlabelMP5Pitch.Value.ToString() + " %";
                weaponDict.Add(1318558775, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarlabelMP5Yaw_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1318558775, out weapon))
            {
                weaponDict.Remove(1318558775);
                weapon.multiplierYaw = (float)trackBarlabelMP5Yaw.Value / 100;
                labelMP5Yaw.Text = trackBarlabelMP5Yaw.Value.ToString() + " %";
                weaponDict.Add(1318558775, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarThompsonPitch_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(2536594571, out weapon))
            {
                weaponDict.Remove(2536594571);
                weapon.multiplierPitch = (float)trackBarThompsonPitch.Value / 100;
                labelThompsonPitch.Text = trackBarThompsonPitch.Value.ToString() + " %";
                weaponDict.Add(2536594571, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarThompsonYaw_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(2536594571, out weapon))
            {
                weaponDict.Remove(2536594571);
                weapon.multiplierYaw = (float)trackBarThompsonYaw.Value / 100;
                labelThompsonYaw.Text = trackBarThompsonYaw.Value.ToString() + " %";
                weaponDict.Add(2536594571, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarAkPitch_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1545779598, out weapon))
            {
                weaponDict.Remove(1545779598);
                weapon.multiplierPitch = (float)trackBarAkPitch.Value / 100;
                labelAkPitch.Text = trackBarAkPitch.Value.ToString() + " %";
                weaponDict.Add(1545779598, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarAkYaw_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(1545779598, out weapon))
            {
                weaponDict.Remove(1545779598);
                weapon.multiplierYaw = (float)trackBarAkYaw.Value / 100;
                labelAkYaw.Text = trackBarAkYaw.Value.ToString() + " %";
                weaponDict.Add(1545779598, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarLR300Pitch_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(2482412119, out weapon))
            {
                weaponDict.Remove(2482412119);
                weapon.multiplierPitch = (float)trackBarLR300Pitch.Value / 100;
                labelLR300Pitch.Text = trackBarLR300Pitch.Value.ToString() + " %";
                weaponDict.Add(2482412119, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void trackBarLR300Yaw_Scroll(object sender, EventArgs e)
        {
            Weapon weapon;
            if (weaponDict.TryGetValue(2482412119, out weapon))
            {
                weaponDict.Remove(2482412119);
                weapon.multiplierYaw = (float)trackBarLR300Yaw.Value / 100;
                labelLR300Yaw.Text = trackBarLR300Yaw.Value.ToString() + " %";
                weaponDict.Add(2482412119, weapon);
            }
            UpdateLoop.rewrite = true;
        }

        private void buttonSaveRecoil_Click(object sender, EventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "\\recoilSave.cfg"))
            {
                File.Delete(Environment.CurrentDirectory + "\\recoilSave.cfg");
            }

            string[] weapons = new string[weaponDict.Count];

            for (int i = 0; i < weaponDict.Count; i++)
            {
                var currentElement = weaponDict.ElementAt(i);
                weapons[i] = currentElement.Key.ToString() + " " + currentElement.Value.multiplierPitch.ToString() + " " + currentElement.Value.multiplierYaw.ToString();
            }

            File.WriteAllLines(Environment.CurrentDirectory + "\\recoilSave.cfg", weapons);

            MessageBox.Show("Saved recoil config.");
        }

        private void checkBoxKeepRecoil_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.keepRecoil = checkBoxKeepRecoil.Checked;
        }

        private void comboBoxChams_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedChams = comboBoxChams.SelectedIndex;
            needRewrite = true;
        }

        private void checkBoxChams_CheckedChanged(object sender, EventArgs e)
        {
            needRewrite = true;
        }

        private void checkBoxInteractiveDebugCamera_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxSpiderman_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.spiderman = checkBoxSpiderman.Checked;
        }

        private void checkBoxJump_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.canJump = checkBoxJump.Checked;
        }

        private void trackBarFOVSlider_Scroll(object sender, EventArgs e)
        {
            UpdateLoop.fov = trackBarFOVSlider.Value;
            labelFOV.Text = trackBarFOVSlider.Value.ToString() + " "; //
        }

        private void checkBoxFOV_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.setFov = checkBoxFOV.Checked;
        }

        private void checkBoxCullingESP_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoop.cullingESP = checkBoxCullingESP.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void buttonCullingOFF_Click(object sender, EventArgs e)
        {
            this.buttonCullingOFF.Enabled = false;

            ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
            ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);
            DMAController.WriteMemory<bool>(otherCulling + Offsets.debugShow, false);


            this.buttonCullingOFF.Visible = false;
            this.buttonCullingON.Enabled = true;
            this.buttonCullingON.Visible = true;
        }

       

        private void buttonCullingON_Click(object sender, EventArgs e)
        {
            this.buttonCullingON.Enabled = false;
            
            //get OcclusionCulling
            ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
            ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);
            //

            //PLAYER ONLY FILTER
           // int layerFilter = 131072;
            //ulong singletonPointer = DMAController.ReadMemory<ulong>(otherCulling + 0x80);
           // ulong debugSettings = DMAController.ReadMemory<ulong>(singletonPointer + Offsets.debugSettings);

           // DMAController.WriteMemory<int>(debugSettings + 0x20, layerFilter); //writes the layer filter
            //                                                              

            DMAController.WriteMemory<bool>(otherCulling + Offsets.debugShow, true); //writes culling on


            this.buttonCullingON.Visible = false;
            this.buttonCullingOFF.Enabled = true;
            this.buttonCullingOFF.Visible = true;
        }
        private void buttonPlayerFilter_Click(object sender, EventArgs e)
        {
            ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
            ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);

            int playerFilter = 131072;
            ulong singletonPointer = DMAController.ReadMemory<ulong>(otherCulling + 0x80);
            ulong debugSettings = DMAController.ReadMemory<ulong>(singletonPointer + Offsets.debugSettings);

            DMAController.WriteMemory<int>(debugSettings + 0x20, playerFilter); //writes the layer filter

        }

        private void buttonNpcFilter_Click(object sender, EventArgs e)
        {
            ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
            ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);

            int npcFilter = 2048;
            ulong singletonPointer = DMAController.ReadMemory<ulong>(otherCulling + 0x80);
            ulong debugSettings = DMAController.ReadMemory<ulong>(singletonPointer + Offsets.debugSettings);

            DMAController.WriteMemory<int>(debugSettings + 0x20, npcFilter); //writes the layer filter
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFilter = comboBoxFilter.SelectedIndex;
        }
        private void buttonEnableRisky_Click(object sender, EventArgs e)
        {
            buttonEnableRisky.Visible = false;
            buttonDisableRisky.Visible = true;

            checkBoxAdminflag.Enabled = true;
            checkBoxJump.Enabled = true;
            checkBoxSpiderman.Enabled = true;
        }

        private void buttonDisableRisky_Click(object sender, EventArgs e)
        {
            buttonDisableRisky.Visible = false;
            buttonEnableRisky.Visible = true;
            

            checkBoxAdminflag.Enabled = false;
            checkBoxJump.Enabled = false;
            checkBoxSpiderman.Enabled = false;
        }

        private void buttonApplyFilter_Click(object sender, EventArgs e)
        {
            ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
            ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);


            ulong singletonPointer = DMAController.ReadMemory<ulong>(otherCulling + 0x80);
            ulong debugSettings = DMAController.ReadMemory<ulong>(singletonPointer + Offsets.debugSettings);
            if (selectedFilter == 0) //player
            {
                int layerFilter = 131072;
                DMAController.WriteMemory<int>(debugSettings + 0x20, layerFilter); //writes the layer filter
            }

            if (selectedFilter == 1) //npc
            {
                int layerFilter = 2048;
                DMAController.WriteMemory<int>(debugSettings + 0x20, layerFilter); //writes the layer filter
            }

            if (selectedFilter == 2) //corpse
            {
                int layerFilter = 512;
                DMAController.WriteMemory<int>(debugSettings + 0x20, layerFilter); //writes the layer filter
            }


        }

        private void buttonSetFOV_Click(object sender, EventArgs e)
        {
            
            ulong camManager = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.GraphicsCVar);
            ulong camMan = DMAController.ReadMemory<ulong>(camManager + 0xB8);

            DMAController.WriteMemory<float>(camMan + Offsets.cameraFov, fov);
        }
    }
}
