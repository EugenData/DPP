﻿using ESRI.ArcGIS.Geodatabase;
using MilSpace.Core;
using MilSpace.Core.Actions.ActionResults;
using MilSpace.Core.Actions.Base;
using MilSpace.Core.Actions.Interfaces;
using MilSpace.DataAccess.DataTransfer;
using MilSpace.DataAccess.Facade;
using MilSpace.Tools.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using A = MilSpace.Core.Actions.Base;

namespace MilSpace.Tools.SurfaceProfile.Actions
{
    class BuildStackVisibilityAction : A.Action<VisibilityCalculationResult>
    {
        private IFeatureClass obserpPointsfeatureClass;
        private IFeatureClass obserpStationsfeatureClass;
        private string rasterSource;
        private string outputSourceName;
        private string outGraphName;
        private VisibilitySession session;

        private VisibilityCalculationresultsEnum calcResults;

        /// <summary>
        /// Ids are selected to expotr for visibility calculation
        /// </summary>
        private int[] pointsFilteringIds;
        private int[] stationsFilteringIds;
        static Logger logger = Logger.GetLoggerEx("BuildStackVisibilityAction");

        private VisibilityCalculationResult result;

        public BuildStackVisibilityAction() : base()
        {
        }

        public BuildStackVisibilityAction(IActionProcessor parameters)
                : base(parameters)
        {

            obserpPointsfeatureClass = parameters.GetParameterWithValidition<IFeatureClass>(ActionParameters.FeatureClass, null).Value;
            obserpStationsfeatureClass = parameters.GetParameter<IFeatureClass>(ActionParameters.FeatureClassX, null).Value;
            rasterSource = parameters.GetParameterWithValidition<string>(ActionParameters.ProfileSource, null).Value;
            pointsFilteringIds = parameters.GetParameterWithValidition<int[]>(ActionParameters.FilteringPointsIds, null).Value;
            stationsFilteringIds = parameters.GetParameterWithValidition<int[]>(ActionParameters.FilteringStationsIds, null).Value;
            calcResults = parameters.GetParameterWithValidition<VisibilityCalculationresultsEnum>(ActionParameters.Calculationresults, VisibilityCalculationresultsEnum.None).Value;
            outputSourceName = parameters.GetParameterWithValidition<string>(ActionParameters.OutputSourceName, string.Empty).Value;
            session = parameters.GetParameterWithValidition<VisibilitySession>(ActionParameters.Session, null).Value;
        }

        public override string ActionId => ActionsEnum.vblt.ToString();

        public override IActionParam[] ParametersTemplate
        {
            get
            {
                return new IActionParam[]
               {
                   new ActionParam<IFeatureClass>() { ParamName = ActionParameters.FeatureClass, Value = null},
                   new ActionParam<IFeatureClass>() { ParamName = ActionParameters.FeatureClassX, Value = null},
                   new ActionParam<int[]>() { ParamName = ActionParameters.FilteringStationsIds, Value = null},
                   new ActionParam<int[]>() { ParamName = ActionParameters.FilteringPointsIds, Value = null},
                   new ActionParam<string>() { ParamName = ActionParameters.ProfileSource, Value = string.Empty},
                   new ActionParam<VisibilityCalculationresultsEnum>() { ParamName = ActionParameters.Calculationresults, Value = VisibilityCalculationresultsEnum.None},
                   new ActionParam<string>() { ParamName = ActionParameters.OutputSourceName, Value = null},
                   new ActionParam<string>() { ParamName = ActionParameters.Session, Value = null}
               };
            }
        }


        public override VisibilityCalculationResult GetResult()
        {
            return result;
        }

        public override void Process()
        {
            result = new VisibilityCalculationResult();
            result.Result.Session = session;

            var results = new List<string>();

            if (calcResults == VisibilityCalculationresultsEnum.None)
            {
                string errorMessage = $"The value {VisibilityCalculationresultsEnum.None.ToString()} is not acceptable for calculatuion results";
                logger.WarnEx(errorMessage);
                result.Exception = new MilSpaceVisibilityCalcFailedException(errorMessage);
                return;
            }

            if (!(calcResults.HasFlag(VisibilityCalculationresultsEnum.VisibilityAreaRaster) || calcResults.HasFlag(VisibilityCalculationresultsEnum.VisibilityAreaRasterSingle)))
            {
                string errorMessage = $"The values {VisibilityCalculationresultsEnum.ObservationPoints.ToString()} and {VisibilityCalculationresultsEnum.VisibilityAreaRaster.ToString()} must be in calculatuion results";
                logger.WarnEx(errorMessage);
                result.Exception = new MilSpaceVisibilityCalcFailedException(errorMessage);
                return;
            }

            try
            {

                var messages = ProcessObservationPoint(results);

                //Here is the place to extend handle the rest of results 

                result.Result.CalculationMessages = results;
                if (messages != null && messages.Any())
                {
                    messages.ToList().ForEach(m => { if (result.Exception != null) logger.ErrorEx(m); else logger.InfoEx(m); });
                }

            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }


        private IEnumerable<string> ProcessObservationPoint(List<string> results)
        {

            IEnumerable<string> messages = null;


            List<KeyValuePair<VisibilityCalculationresultsEnum, int[]>> pointsIDs = new List<KeyValuePair<VisibilityCalculationresultsEnum, int[]>>();


            if (calcResults.HasFlag(VisibilityCalculationresultsEnum.ObservationPoints))
            {
                pointsIDs.Add(new KeyValuePair<VisibilityCalculationresultsEnum, int[]>(VisibilityCalculationresultsEnum.ObservationPoints, pointsFilteringIds));
            }

            if (calcResults.HasFlag(VisibilityCalculationresultsEnum.ObservationPointSingle) && pointsFilteringIds.Length > 1)
            {
                pointsIDs.AddRange(
                    pointsFilteringIds.Select(id => new KeyValuePair<VisibilityCalculationresultsEnum, int[]>(VisibilityCalculationresultsEnum.ObservationPoints, new int[] { id })).ToArray());
            }

            int index = -1;

            foreach (var curPoints in pointsIDs)
            {
                var pointId = curPoints.Value.Length == 1 ? ++index : -1;

                var oservPointFeatureClassName = VisibilitySession.GetResultName(curPoints.Key, outputSourceName, pointId);

                var exportedFeatureClass = GdbAccess.Instance.ExportObservationFeatureClass(obserpPointsfeatureClass as IDataset, oservPointFeatureClassName, curPoints.Value);

                if (string.IsNullOrWhiteSpace(exportedFeatureClass))
                {
                    string errorMessage = $"The feature calss {oservPointFeatureClassName} was not exported";
                    result.Exception = new MilSpaceVisibilityCalcFailedException(errorMessage);
                    result.ErrorMessage = errorMessage;
                    logger.ErrorEx(errorMessage);
                    return messages;
                }

                results.Add(exportedFeatureClass);

                //Generate Visibility Raster
                string featureClass = oservPointFeatureClassName;


                outGraphName = VisibilitySession.GetResultName(pointId == -1 ? VisibilityCalculationresultsEnum.VisibilityAreaRaster : VisibilityCalculationresultsEnum.VisibilityAreaRasterSingle, outputSourceName, pointId);

                if (!ProfileLibrary.GenerateVisibilityData(rasterSource, featureClass, VisibilityAnalysisTypesEnum.Frequency, outGraphName, messages))
                {
                    result.Exception = new MilSpaceVisibilityCalcFailedException();
                }
                else
                {
                    results.Add(outGraphName);
                }
            }


            if (calcResults.HasFlag(VisibilityCalculationresultsEnum.ObservationStations))
            {
                var oservStationsFeatureClassName = VisibilitySession.GetResultName(VisibilityCalculationresultsEnum.ObservationStations, outputSourceName);
                var exportedFeatureClass = GdbAccess.Instance.ExportObservationFeatureClass(obserpStationsfeatureClass as IDataset, oservStationsFeatureClassName, stationsFilteringIds);
                if (!string.IsNullOrWhiteSpace(exportedFeatureClass))
                {
                    results.Add(exportedFeatureClass);
                }
                else
                {
                    string errorMessage = $"The feature calss {oservStationsFeatureClassName} was not exported";
                    result.ErrorMessage = errorMessage;
                    logger.ErrorEx(errorMessage);
                }
            }


            return messages;
        }
    }
}
