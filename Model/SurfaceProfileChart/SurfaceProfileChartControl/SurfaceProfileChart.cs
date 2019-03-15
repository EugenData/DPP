﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MilSpace.DataAccess.DataTransfer;

namespace SurfaceProfileChart.SurfaceProfileChartControl
{
    public partial class SurfaceProfileChart : UserControl
    {
        private SurfaceProfileChartController _controller;

        [Category("Profile"), Description("")]
        public double PathLength { get; set; }

        [Category("Profile"), Description("")]
        public double MinAngel { get; set; }

        [Category("Profile"), Description("")]
        public double MaxAngel { get; set; }

        [Category("Profile"), Description("")]
        public double MinHeight { get; set; }

        [Category("Profile"), Description("")]
        public double MaxHeight { get; set; }

        public bool Current { get; set; }
        public int SelectedProfileIndex { get; set; }

        public SurfaceProfileChart()
        {
            Current = false;
            SelectedProfileIndex = -1;

            _controller = new SurfaceProfileChartController(this);

            InitializeComponent();
        }

        public void InitializeProfile(ProfileSession profileSession)
        {
            profileChart.Series.Clear();
           
            foreach (var line in profileSession.ProfileLines)
            {
                profileChart.Series.Add(new Series
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.ForestGreen,
                    Name = line.Id.ToString()
                });

                var profileSurface =
                    profileSession.ProfileSurfaces.First(surface => surface.LineId == line.Id);

                foreach (var point in profileSurface.ProfileSurfacePoints)
                {
                    profileChart.Series.Last().Points.AddXY(point.Distance, point.Z);
                }
            }
        }

        public void AddInvisibleLine(ProfileSurface surface)
        {
           foreach (var point in surface.ProfileSurfacePoints)
           {
               profileChart.Series[surface.LineId.ToString()].Points
                   .FirstOrDefault(linePoint => linePoint.XValue == point.Distance).Color = Color.Red;
           }
        }


        private void SurfaceProfileChart_Load(object sender, EventArgs e)
        {
            profileChart.ChartAreas["Default"].CursorX.IsUserEnabled = true;
            profileChart.ChartAreas["Default"].CursorX.IsUserSelectionEnabled = true;
            profileChart.ChartAreas["Default"].AxisX.ScaleView.Zoomable = true;

            _controller.LoadSeries();
            _controller.AddInvisibleZones();
        }

        private void Profile_MouseDown(object sender, MouseEventArgs e)
        {
            var selectedPoint = profileChart.HitTest(e.X, e.Y);

            if (selectedPoint.ChartElementType == ChartElementType.DataPoint)
            {
                SelectedProfileIndex = profileChart.Series.IndexOf(selectedPoint.Series.Name);

                //todo fire event 
            }

        }
    }
}
