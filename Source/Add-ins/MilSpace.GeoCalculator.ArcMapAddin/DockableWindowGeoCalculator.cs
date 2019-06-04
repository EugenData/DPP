﻿using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using MilSpace.GeoCalculator.BusinessLogic;
using MilSpace.GeoCalculator.BusinessLogic.Interfaces;
using MilSpace.GeoCalculator.BusinessLogic.Models;
using MilSpace.GeoCalculator.BusinessLogic.ReferenceData;
using MilSpace.GeoCalculator.BusinessLogic.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;

namespace ArcMapAddin
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    [Guid("CBD4D23F-9477-493E-B9D3-ADFC0753E38E")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ArcMapAddin_DockableWindowGeoCalculator")]
    public partial class DockableWindowGeoCalculator : UserControl
    {
        private readonly IBusinessLogic _businessLogic;
        private readonly ProjectionsModel _projectionsModel;
        private PointModel _pointModel;
        private readonly IList<IPoint> ClickedPointsList = new List<IPoint>();

        public DockableWindowGeoCalculator(object hook, IBusinessLogic businessLogic, ProjectionsModel projectionsModel)
        {
            InitializeComponent();
            this.Hook = hook;
            _businessLogic = businessLogic ?? throw new ArgumentNullException(nameof(businessLogic));
            _projectionsModel = projectionsModel ?? throw new ArgumentNullException(nameof(projectionsModel));

            LocalizeComponents();
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        #region AddInImpl
        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private DockableWindowGeoCalculator m_windowUI;

            public AddinImpl()
            {
            }
            internal DockableWindowGeoCalculator UI
            {
                get { return m_windowUI; }
            }

            protected override IntPtr OnCreateChild()
            {
                if (this.Hook is IApplication arcMap)
                {
                    m_windowUI = new DockableWindowGeoCalculator(this.Hook, new BusinessLogic(arcMap, new DataExport()),
                                                                        CreateProjecstionsModelFromSettings());
                    return m_windowUI.Handle;
                }
                else return IntPtr.Zero;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

        }
        #endregion

        #region UserControl events handlers
        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (_pointModel == null) MessageBox.Show("Please select a point on the map.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            var folderBrowserResult = saveButtonFileDialog.ShowDialog();
            if (folderBrowserResult == DialogResult.OK)
                await _businessLogic.SaveProjectionsToXmlFileAsync(_pointModel, saveButtonFileDialog.FileName).ConfigureAwait(false);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (_pointModel == null) MessageBox.Show("Please select a point on the map.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else _businessLogic.CopyCoordinatesToClipboard(_pointModel);
        }

        private async void MoveToCenterButton_Click(object sender, EventArgs e)
        {
            var centerPoint = await _businessLogic.GetDisplayCenterAsync().ConfigureAwait(false);
            await ProjectPointAsync(centerPoint).ConfigureAwait(false);
        }

        private void PointsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TODO: We need actual point here to project further.
            //var selectedPoint = ClickedPointsList[PointsListBox.SelectedIndex];
            //await ProjectPointAsync(selectedPoint).ConfigureAwait(false);
        }
        #endregion

        #region ArcMap events handlers
        internal async void ArcMap_OnMouseDown(int x, int y)
        {
            var clickedPoint = await _businessLogic.GetSelectedPointAsync(x, y).ConfigureAwait(false);
            ClickedPointsList.Add(clickedPoint);
            PointsListBox.Items.Add($"{clickedPoint.X.ToRoundedString()}  {clickedPoint.Y.ToRoundedString()}");
            PointsListBox.Refresh();            
            await ProjectPointAsync(clickedPoint).ConfigureAwait(false);            
        }

        internal async void ArcMap_OnMouseMove(int x, int y)
        {            
            var currentPoint = await _businessLogic.GetSelectedPointAsync(x, y).ConfigureAwait(false);
            XCoordinateTextBox.Text = currentPoint.X.ToString();
            YCoordinateTextBox.Text = currentPoint.Y.ToString();            
        }        
        #endregion

        #region Private methods
        private void LocalizeComponents()
        {
            try
            {
                var context = new LocalizationContext();
                this.Text = context.CoordinatesConverterWindowCaption;
                this.LatitudeLongitudeGroup.Text = context.LatitudeLongitudeLabel;
                this.CurrentMapLabel.Text = context.CurrentMapLabel;
                this.WgsCoordinatesLabel.Text = context.WgsLabel;
                this.PulkovoCoordinatesLabel.Text = context.PulkovoLabel;
                this.UkraineCoordinatesLabel.Text = context.UkraineLabel;
                this.MgrsNotationLabel.Text = context.MgrsLabel;
                this.SaveButton.Text = context.SaveButton;
                this.CopyButton.Text = context.CopyButton;
                this.MoveToCenterButton.Text = context.MoveToCenterButton;
            }
            catch { MessageBox.Show("No Localization.xml found or there is an error during loading. Coordinates Converter window is not fully localized."); }
        }

        private async Task ProjectPointAsync(IPoint inputPoint)
        {
            _pointModel = new PointModel();

            if (inputPoint == null) throw new ArgumentNullException(nameof(inputPoint));
            if (inputPoint.SpatialReference == null) throw new NullReferenceException($"Point with ID = {inputPoint.ID} has no spatial reference.");

            XCoordinateTextBox.Text = inputPoint.X.ToRoundedString();
            YCoordinateTextBox.Text = inputPoint.Y.ToRoundedString();

            _pointModel.XCoord = inputPoint.X.ToRoundedDouble();
            _pointModel.YCoord = inputPoint.Y.ToRoundedDouble();

            //MGRS string MUST be calculated using WGS84 projected point, thus the next lines order matters!
            var wgsPoint = await _businessLogic.ProjectPointAsync(inputPoint, _projectionsModel.WGS84Projection);
            WgsXCoordinateTextBox.Text = wgsPoint.X.ToRoundedString();
            WgsYCoordinateTextBox.Text = wgsPoint.Y.ToRoundedString();

            _pointModel.WgsXCoord = wgsPoint.X.ToRoundedDouble();
            _pointModel.WgsYCoord = wgsPoint.Y.ToRoundedDouble();

            MgrsNotationTextBox.Text = (await _businessLogic.ConvertToMgrs(wgsPoint))?.ToSeparatedMgrs();

            UTMNotationTextBox.Text = await _businessLogic.ConvertToUtm(wgsPoint);

            _pointModel.MgrsRepresentation = MgrsNotationTextBox.Text;

            _pointModel.UtmRepresentation = UTMNotationTextBox.Text;

            var pulkovoPoint = await _businessLogic.ProjectPointAsync(inputPoint, _projectionsModel.Pulkovo1942Projection);
            PulkovoXCoordinateTextBox.Text = pulkovoPoint.X.ToRoundedString();
            PulkovoYCoordinateTextBox.Text = pulkovoPoint.Y.ToRoundedString();

            _pointModel.PulkovoXCoord = pulkovoPoint.X.ToRoundedDouble();
            _pointModel.PulkovoYCoord = pulkovoPoint.Y.ToRoundedDouble();

            var ukrainePoint = await _businessLogic.ProjectPointAsync(inputPoint, _projectionsModel.Ukraine2000Projection);
            UkraineXCoordinateTextBox.Text = ukrainePoint.X.ToRoundedString();
            UkraineYCoordinateTextBox.Text = ukrainePoint.Y.ToRoundedString();

            _pointModel.UkraineXCoord = ukrainePoint.X.ToRoundedDouble();
            _pointModel.UkraineYCoord = ukrainePoint.Y.ToRoundedDouble();

        }

        private static ProjectionsModel CreateProjecstionsModelFromSettings()
        {
            return new ProjectionsModel(new SingleProjectionModel((int)esriSRProjCSType.esriSRProjCS_WGS1984UTM_36N, 30.000, 0.000),
                                        new SingleProjectionModel((int)esriSRProjCSType.esriSRProjCS_Pulkovo1942GK_6N, 30.000, 44.330),
                                        new SingleProjectionModel(Constants.Ukraine2000ID[2], 30.000, 43.190));
        }
        #endregion        
    }
}
