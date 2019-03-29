﻿using System;
using System.Globalization;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using MilSpace.Core.Tools;
using static MilSpace.Profile.DockableWindowMilSpaceProfileCalc;

namespace MilSpace.Profile
{
    public class PickCoordinates : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public PickCoordinates()
        {

            Point = new PointClass();
            Point.PutCoords(0, 0);
        }

        public IPoint Point { get; private set; }

        protected override void OnUpdate()
        {

        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {
            UID toolId = new UIDClass();
            toolId.Value = ThisAddIn.IDs.PickCoordinates;
            var mxdDoc = ArcMap.Document;
            var screenDisplay = mxdDoc.ActiveView.ScreenDisplay;
            var point = screenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
            var pointToSave = screenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);

            point.SpatialReference = mxdDoc.FocusMap.SpatialReference;

            pointToSave.SpatialReference = mxdDoc.FocusMap.SpatialReference;

            EsriTools.ProjectToWgs84(point);

            var winImpl = AddIn.FromID<DockableWindowMilSpaceProfileCalc.AddinImpl>(ThisAddIn.IDs.DockableWindowMilSpaceProfileCalc);

            if (winImpl.MilSpaceProfileCalsController.View.ActiveButton == ProfileSettingsPointButton.PointsFist)
            {

                winImpl.MilSpaceProfileCalsController.SetFirsPointForLineProfile(point, pointToSave);

            }

            if (winImpl.MilSpaceProfileCalsController.View.ActiveButton == ProfileSettingsPointButton.PointsSecond)
            {
                winImpl.MilSpaceProfileCalsController.SetSecondfPointForLineProfile(point, pointToSave);
            }

            if (winImpl.MilSpaceProfileCalsController.View.ActiveButton == ProfileSettingsPointButton.CenterFun)
            {

                winImpl.MilSpaceProfileCalsController.SetCenterPointForFunProfile(point, pointToSave);
            }

            var settings = winImpl.MilSpaceProfileCalsController.ProfileSettings[ProfileSettingsTypeEnum.Points];
        }
    }

}
