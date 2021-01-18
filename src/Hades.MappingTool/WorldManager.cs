﻿using Darkages;
using Darkages.Storage;
using Darkages.Templates;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class WorldManager : Form
    {
        public WorldManager()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            propertyGrid1.SelectedObject = new WorldMapTemplate();
        }

        private Area SelectedArea;
        private Point SelectedPoint;

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var bg = pictureBox1.BackgroundImage;

            SelectedPoint = e.Location;


            using (var gfx = pictureBox1.CreateGraphics())
            {
                gfx.Clear(Color.Black);
                if (bg != null)
                    gfx.DrawImage(bg, pictureBox1.DisplayRectangle);
                gfx.DrawRectangle(Pens.Red, e.X - 10, e.Y, 10, 10);
            }


            label1.Text = "Selected Point: " + SelectedPoint.ToString();

            var world = ServerContext.GlobalWorldMapTemplateCache.Values.ElementAt(comboBox2.SelectedIndex);

            if (world == null)
                return;

            using (var gfx = pictureBox1.CreateGraphics())
            {
                foreach (var portal in world.Portals)
                {
                    gfx.DrawString(portal.DisplayName, Font, Brushes.Yellow, new Point(portal.PointY, portal.PointX));
                    gfx.DrawRectangle(Pens.Cyan, portal.PointY - 10, portal.PointX, 10, 10);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Error, You Must give it a name.");
                return;
            }

            if (SelectedPoint.X == 0 || SelectedPoint.Y == 0)
            {
                MessageBox.Show("Error, You Select a point in the map first.");
                return;
            }

            if (SelectedArea == null)
            {
                MessageBox.Show("Error, You Must select a map to warp to.");
                return;
            }

            var world = ServerContext.GlobalWorldMapTemplateCache.Values.ElementAt(comboBox2.SelectedIndex);

            if (world == null)
            {
                MessageBox.Show("Error, World was invalid.");
                return;
            }

            if (world.Portals.Any(i => i.DisplayName.Equals(textBox1.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Error, Portal with this name already exists.");
                return;
            }

           


            var portals = world.Portals;

            var ArrivalX = -1;
            var ArrivalY = -1;

            int.TryParse(textBox2.Text, out ArrivalX);
            int.TryParse(textBox3.Text, out ArrivalY);

            if (ArrivalX < 0 || ArrivalY < 0)
            {
                MessageBox.Show("Error, Arrival Location is invalid.");
                return;
            }

            if (ArrivalX > byte.MaxValue)
                ArrivalX = byte.MaxValue;

            if (ArrivalY > byte.MaxValue)
                ArrivalY = byte.MaxValue;

            portals.Add(new WorldPortal()
            {
                Destination = new Warp()
                {
                    AreaID = SelectedArea.ID,
                    Location = new Position(Convert.ToByte(ArrivalX), Convert.ToByte(ArrivalY))
                },
                DisplayName = textBox1.Text,
                PointX = (short) SelectedPoint.Y,
                PointY = (short) SelectedPoint.X
            });

            StorageManager.WorldMapBucket.Save(world, true);

            ServerContext.LoadAndCacheStorage();
            comboBox1.DataSource =
                ServerContext.GlobalMapCache.Select(i => i.Value.ID + " |    " + i.Value.Name).ToList();
            timer1.Enabled = true;

            LoadSources();
            MessageBox.Show("Updated World Map Success.");
        }

        private void WorldManager_Load(object sender, EventArgs e)
        {
            pictureBox1.BackgroundImage = Image.FromFile(ServerContext.StoragePath + $"\\static\\fieldmap_images\\{numericUpDown1.Value}.png");

            LoadSources();
        }

        private void LoadSources()
        {
            comboBox2.DataSource = ServerContext.GlobalWorldMapTemplateCache.Select(n => n.Value.Name).ToList();

            comboBox1.DataSource =
                ServerContext.GlobalMapCache.Select(i => i.Value.ID + " |    " + i.Value.Name).ToList();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            PictureBox1_MouseClick(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            {
                timer1.Enabled = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var id = comboBox1.Text.Split('|').FirstOrDefault().Trim();

            if (id == null)
                return;

            var area = ServerContext.GlobalMapCache[Convert.ToInt32(id)];

            if (area != null) SelectedArea = area;
        }

        private List<Position> Activations = new List<Position>();

        private void button1_Click(object sender, EventArgs e)
        {
            var x = textBox5.Text;
            var y = textBox4.Text;

            var X = -1;
            var Y = -1;

            int.TryParse(x, out X);
            int.TryParse(y, out Y);

            if (X < 0 || Y < 0)
                return;

            Activations.Add(new Position(X, Y));
            listView1.Items.Add(new ListViewItem(new[] {x, y}));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var idx = listView1.SelectedIndices;

            foreach (int id in idx)
                if (id >= 0)
                {
                    Activations.RemoveAt(id);
                    listView1.Items.RemoveAt(id);
                }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Activations.Clear();
            listView1.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Error, You Must give it a name.");
                return;
            }

            if (SelectedPoint.X == 0 || SelectedPoint.Y == 0)
            {
                MessageBox.Show("Error, You Select a point in the map first.");
                return;
            }

            if (SelectedArea == null)
            {
                MessageBox.Show("Error, You Must select a map to warp to.");
                return;
            }

            try
            {
                var template = new WarpTemplate();

                template.To = new Warp()
                {
                    AreaID = 0,
                    PortalKey = (int) numericUpDown1.Value
                };

                template.ActivationMapId = SelectedArea.ID;
                template.Activations = new List<Warp>();

                foreach (var activation in Activations)
                    template.Activations.Add(new Warp()
                    {
                        AreaID = SelectedArea.ID,
                        Location = activation
                    });

                template.LevelRequired = 1;
                template.WarpType = WarpType.World;
                template.WarpRadius = 0;
                template.Name = $"Warp from {SelectedArea.ID} to World Map.";

                StorageManager.WarpBucket.Save(template);
                ServerContext.LoadAndCacheStorage(true);
                MessageBox.Show("Warp Added. for this return point.");
            }
            catch (Exception)
            {
                MessageBox.Show("Error, Warp could not be saved.");
                return;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var template = (WorldMapTemplate)propertyGrid1.SelectedObject;

            if (template != null)
            {
                StorageManager.WorldMapBucket.Save(template);
                ServerContext.LoadAndCacheStorage(true);
                MessageBox.Show("New World Created.");
                LoadSources();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}