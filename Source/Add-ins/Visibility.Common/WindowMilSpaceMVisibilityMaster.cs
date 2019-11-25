﻿using ESRI.ArcGIS.Carto;
using MilSpace.Core;
using MilSpace.Core.Tools;
using MilSpace.DataAccess.DataTransfer;
using MilSpace.Tools;
using MilSpace.Visibility.Localization;
using MilSpace.Visibility.ViewController;
using MilSpace.Visibility.ViewController.WizardController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace MilSpace.Visibility
{
    public partial class WindowMilSpaceMVisibilityMaster : Form, IWizardView
    {
        private string _previousPickedRasterLayer;
        private VisibilityCalcTypeEnum _stepControl = VisibilityCalcTypeEnum.None;
        private const string _allValuesFilterText = "All";
        private WizardViewController controller = new WizardViewController(ArcMap.Document);
        private BindingList<CheckObservPointGui> _observPointGuis;
        private BindingList<CheckObservPointGui> _observationObjects;
        internal WizardResult FinalResult = new WizardResult();

        private static IActiveView ActiveView => ArcMap.Document.ActiveView;

        private MapLayersManager manager = new MapLayersManager(ActiveView);

        public WindowMilSpaceMVisibilityMaster(string selectedObservPoints, string selectedObservObjects, string previousPickedRaster)
        {
            InitializeComponent();
            controller.SetView(this);
            _previousPickedRasterLayer = previousPickedRaster;

            //label слой ПН\ТН
            ObservPointLabel.Text = selectedObservPoints;
            //label слой ОН
            observObjectsLabel.Text = selectedObservObjects;

            cmbEmptyDataValue.SelectedIndex = 1;

        }

        protected override void OnLoad(EventArgs e)
        {
            FillComboBoxes();

            //disable all tabs
            foreach (TabPage tab in StepsTabControl.TabPages)
            {
                tab.Enabled = false;
            }
            (StepsTabControl.TabPages[0] as TabPage).Enabled = true;

            panel1.Enabled = false;
        }

        public void DisabelObjList()
        {
            dgvObjects.Enabled = false;
            splitContainer1.Panel2.Enabled = false;
        }
        public void EanableObjList()
        {
            dgvObjects.Enabled = true;
            dgvObjects.Refresh();
            splitContainer1.Panel2.Enabled = true;

        }
        public void FirstTypePicked()//triggers when user picks first type
        {
            _stepControl = VisibilityCalcTypeEnum.OpservationPoints;
            controller.UpdateObservationPointsList();
            PopulateComboBox();
            DisabelObjList();
            controller.UpdateObservationPointsList_OnCurrentExtend(ActiveView);
            dgvObjects.DataSource = null;
        }
        public void SecondTypePicked()//triggers when user picks second type
        {
            _stepControl = VisibilityCalcTypeEnum.ObservationObjects;
            controller.UpdateObservationPointsList();
            EanableObjList();
            PopulateComboBox();
            controller.UpdateObservObjectsList(true);
            controller.UpdateObservationPointsList_OnCurrentExtend(ActiveView);
        }

        public void FillObservPointLabel()
        {
            var temp = controller.GetObservationPointsLayers(ActiveView).ToArray();

        }
        public void FillComboBoxes()
        {
            var list = new List<string>();
            list.AddRange(controller.GetObservationPointTypes().ToArray());
            cmbAffiliation.Items.Clear();

            cmbAffiliation.Items.Add(controller.GetAllAffiliationType());
            cmbAffiliation.Items.AddRange(list.ToArray());
            cmbAffiliation.SelectedItem = controller.GetAllAffiliationType();

            list = new List<string>();
            list.AddRange(controller.GetObservationObjectTypes().ToArray());
            cmbObservObject.Items.Clear();

            cmbObservObject.Items.AddRange(list.ToArray());
            cmbObservObject.Items.Add(controller.GetAllAffiliationType_for_objects());

            cmbObservObject.SelectedItem = _allValuesFilterText;

            list = new List<string>();
            cmbType.Items.Clear();
            list.AddRange(controller.GetObservationPointMobilityTypes().ToArray());
            cmbType.Items.AddRange(list.ToArray());
            cmbType.Items.Add(controller.GetAllMobilityType());
            cmbType.SelectedItem = controller.GetAllMobilityType();

            cmbMapLayers.Items.Clear();
            cmbMapLayers.Items.AddRange(controller.GetAllLayers().ToArray());
            cmbMapLayers.SelectedItem = controller.GetLastLayer();

            cmbPositions.Items.Clear();
            cmbPositions.Items.AddRange(controller.GetLayerPositions().ToArray());
            cmbPositions.SelectedItem = controller.GetDefaultLayerPosition();
        }
        public void PopulateComboBox()
        {
            imagesComboBox.DataSource = null;
            imagesComboBox.DataSource = (manager.RasterLayers.Select(i => i.Name).ToArray());

            if (_previousPickedRasterLayer != null)
            {
                imagesComboBox.SelectedItem = _previousPickedRasterLayer;
            }
        }

        public void FillObservationPointList(IEnumerable<ObservationPoint> observationPoints )
        {
            if (observationPoints != null && observationPoints.Any())
            {
                var ItemsToShow = observationPoints.Select(t => new CheckObservPointGui
                {
                    Title = t.Title,
                    Type = t.Type,
                    Affiliation = t.Affiliation,
                    Date = t.Dto.Value.ToShortDateString(),
                    Id = t.Objectid

                }).ToList();

                dvgCheckList.Rows.Clear();
                dvgCheckList.CurrentCell = null;

                _observPointGuis = new BindingList<CheckObservPointGui>(ItemsToShow);
                dvgCheckList.DataSource = _observPointGuis;

                SetDataGridView();

                dvgCheckList.Update();
                dvgCheckList.Rows[0].Selected = true;
              
            }

        }
        public void FillObservPointsOnCurrentView(IEnumerable<ObservationPoint> observationPoints)
        {
            if (observationPoints != null && observationPoints.Any())
            {
                var ItemsToShow = observationPoints.Select(t => new CheckObservPointGui
                {
                    Title = t.Title,
                    Type = t.Type,
                    Affiliation = t.Affiliation,
                    Date = t.Dto.Value.ToString(Helper.DateFormatSmall),
                    Id = t.Objectid

                });

                //Finding coincidence
                var commonT = (_observPointGuis.Select(a => a.Id).Intersect(ItemsToShow.Select(b => b.Id))).ToList();

                foreach (CheckObservPointGui e in _observPointGuis)
                {
                    if (commonT.Contains(e.Id))
                    {
                        e.Check = true;
                    }
                }

                BindingList<CheckObservPointGui> temp = new BindingList<CheckObservPointGui>(_observPointGuis);

                dvgCheckList.CurrentCell = null;

                dvgCheckList.DataSource = temp;
                SetDataGridView();

                dvgCheckList.Update();
            }
        }
        public void FillObsObj(IEnumerable<ObservationObject> All, bool useCurrentExtent = false)
        {
            try
            {
            
                var itemsToShow = All.Select(t => new CheckObservPointGui
                {
                    Title = t.Title,
                    Affiliation = t.ObjectType.ToString(),
                    Id = t.ObjectId,
                    Type = t.Group,
                    Date = t.DTO.ToString(Helper.DateFormatSmall)
                });

                dgvObjects.DataSource = null; //Clearing listbox

                _observationObjects = new BindingList<CheckObservPointGui>(itemsToShow.ToArray());

                if (_observationObjects != null)
                {
                    dgvObjects.DataSource = _observationObjects;
                    SetDataGridView_For_Objects();
                }
                if (useCurrentExtent)
                {
                    var onCurrent = controller
                                .GetObservObjectsOnCurrentMapExtent(ActiveView);

                    var commonO = (_observationObjects.Select(a => a.Id).Intersect(onCurrent.Select(b => b.ObjectId)));

                    foreach (CheckObservPointGui e in _observationObjects)
                    {
                        if (commonO.Contains(e.Id))
                        {
                            e.Check = true;
                        }
                    }
                    dgvObjects.Refresh();
                }
            }
            catch (ArgumentNullException)
            {
                dgvObjects.Text = "Objects are not found";
            }

        }
        private void SetDataGridView_For_Objects()
        {
            try
            {
                dgvObjects.Columns["Check"].HeaderText = "";
                dgvObjects.Columns["Type"].HeaderText = "Group";//stands for "Afillation"

                dgvObjects.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvObjects.Columns["Affiliation"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;//stands for "Group"
                dgvObjects.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;//stands for "Afillation"
                dgvObjects.Columns["Date"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                dgvObjects.Columns["Check"].MinimumWidth = 25;

                dgvObjects.Columns["Type"].ReadOnly = true;
                dgvObjects.Columns["Title"].ReadOnly = true;
                dgvObjects.Columns["Affiliation"].ReadOnly = true;

                dgvObjects.Columns["Type"].Visible = true;//basically its an "Afillation" column
                dgvObjects.Columns["Id"].Visible = false;
                dgvObjects.Columns["Date"].Visible = true;
            }
            catch (NullReferenceException)
            {
                dgvObjects.Text = "some error occurred";
            }
        }

        private void SetDataGridView()
        {
            try
            {
                dvgCheckList.Columns["Check"].HeaderText = "";
                dvgCheckList.Columns["Id"].Visible = false;

                dvgCheckList.Columns["Date"].ReadOnly = true;
                dvgCheckList.Columns["Type"].ReadOnly = true;
                dvgCheckList.Columns["Affiliation"].ReadOnly = true;
                dvgCheckList.Columns["Title"].ReadOnly = true;

                dvgCheckList.Columns["Affiliation"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                dvgCheckList.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dvgCheckList.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;

                dvgCheckList.Columns["Affiliation"].MinimumWidth = 50;
                dvgCheckList.Columns["Title"].MinimumWidth = 50;
                dvgCheckList.Columns["Type"].MinimumWidth = 50;
                dvgCheckList.Columns["Date"].MinimumWidth = 50;
                dvgCheckList.Columns["Check"].MinimumWidth = 25;

            }
            catch (NullReferenceException)
            {
                dvgCheckList.Text = "some error occurred";
            }
        }

        private void DisplaySelectedColumns_Points(DataGridView D)
        {
            try
            {
                D.Columns["Affiliation"].Visible = checkAffiliation.Checked;
                D.Columns["Type"].Visible = checkType.Checked;
                D.Columns["Date"].Visible = checkDate.Checked;
            }
            catch (NullReferenceException)
            {

            }
        }
        private void DisplaySelectedColumns_Objects(DataGridView D)
        {
            try
            {
                D.Columns["Affiliation"].Visible = checkB_Affilation.Checked;
                D.Columns["Date"].Visible = checkDate_Object.Checked;
            }
            catch (NullReferenceException)
            {

            }

        }

        private void Filter_CheckedChanged(object sender, EventArgs e)
        {
            DisplaySelectedColumns_Points(dvgCheckList);
        }

        private void Filter_For_Object_CheckedChanged(object sender, EventArgs e)
        {
            DisplaySelectedColumns_Objects(dgvObjects);
        }

        private void Select_All(object sender, EventArgs e)
        {

            CheckBox_All(_observationObjects, dgvObjects,checkB_All_object);

        }

        private void Select_All_Points(object sender, EventArgs e)
        {

            CheckBox_All(_observPointGuis, dvgCheckList,checkB_Select_All_Points);

        }

        private void CheckBox_All(BindingList<CheckObservPointGui> Bin_list, DataGridView dgv, CheckBox Box)
        {
            foreach (CheckObservPointGui o in Bin_list)
            {
                o.Check = Box.Checked;
            }
            dgv.DataSource = Bin_list;
            dgv.Refresh();
        }

        public VeluableObservPointFieldsEnum GetFilter
        {
            get
            {
                var result = VeluableObservPointFieldsEnum.All;

                if (checkAffiliation.Checked)
                {
                    result = result | VeluableObservPointFieldsEnum.Affiliation;
                }
                if (checkDate.Checked)
                {
                    result = result | VeluableObservPointFieldsEnum.Date;
                }

                if (checkType.Checked)
                {
                    result = result | VeluableObservPointFieldsEnum.Type;
                }

                return result;
            }
        }

        private void FilterData(DataGridView Grid, ComboBox combo)
        {
            if (Grid.Rows.Count == 0)
            {
                return;
            }

            Grid.CurrentCell = null;

            foreach (DataGridViewRow row in Grid.Rows)
            {
                CheckRowForFilter(row, combo);
            }

            if (Grid.FirstDisplayedScrollingRowIndex != -1)
            {
                Grid.Rows[Grid.FirstDisplayedScrollingRowIndex].Selected = true;
            }
        }

        private void CheckRowForFilter(DataGridViewRow row, ComboBox combo)
        {
            if (combo.SelectedItem != null && combo.SelectedItem.ToString() != _allValuesFilterText)
            {
                row.Visible = (row.Cells["Affiliation"].Value.ToString() == combo.SelectedItem.ToString());
                if (!row.Visible) return;
            }

            if (row.Cells["Type"].Value != null && cmbType.SelectedItem != null && cmbType.SelectedItem.ToString() != _allValuesFilterText)
            {
                row.Visible = (row.Cells["Type"].Value.ToString() == cmbType.SelectedItem.ToString());
                return;
            }

            row.Visible = true;
        }
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData(dvgCheckList, cmbAffiliation);
        }

        private void cmbAffiliation_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData(dvgCheckList, cmbAffiliation);
        }
        private void cmbObservObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData(dgvObjects, cmbObservObject);
        }

        private void AssemblyWizardResult()
        {

            FinalResult = new WizardResult
            {
                ObservPointIDs = _observPointGuis.Where(p => p.Check).Select(i => i.Id).ToArray(),
                Table = TableChkBox.Checked,
                SumFieldOfView = SumChkBox.Checked,
                RasterLayerName = imagesComboBox.SelectedItem.ToString(),
                RelativeLayerName = cmbMapLayers.SelectedItem.ToString(),
                ResultLayerPosition = controller.GetPositionByStringValue(cmbPositions.SelectedItem.ToString()),
                ResultLayerTransparency = Convert.ToInt16(tbTransparency.Text),
                CalculationType = _stepControl,
                TaskName = VisibilityManager.GenerateResultId(LocalizationContext.Instance.CalcTypeLocalisationShort[_stepControl])
            };


            if (_stepControl == VisibilityCalcTypeEnum.OpservationPoints)
            {
                FinalResult.VisibilityCalculationResults = SumChkBox.Checked ?
                    VisibilityCalculationresultsEnum.ObservationPoints | VisibilityCalculationresultsEnum.VisibilityAreaRaster :
                    VisibilityCalculationresultsEnum.None;
            }
            else if (_stepControl == VisibilityCalcTypeEnum.ObservationObjects)
            {
                FinalResult.ObservObjectIDs = _observationObjects.Where(o => o.Check).Select(i => i.Id).ToArray();
                FinalResult.VisibilityCalculationResults = (SumChkBox.Checked ?
                                                    VisibilityCalculationresultsEnum.ObservationPoints | VisibilityCalculationresultsEnum.VisibilityAreaRaster :
                                                    VisibilityCalculationresultsEnum.None)
                                                    | VisibilityCalculationresultsEnum.ObservationStations | VisibilityCalculationresultsEnum.VisibilityObservStationClip;

            }

            //Trim by real Area
            if (chkTrimRaster.Checked)
            {
                FinalResult.VisibilityCalculationResults |= VisibilityCalculationresultsEnum.VisibilityAreasTrimmedByPoly;
                if (checkBoxOP.Checked)
                {
                    FinalResult.VisibilityCalculationResults |= VisibilityCalculationresultsEnum.VisibilityAreaTrimmedByPolySingle;
                }
            }

            if (chkConvertToPolygon.Checked)
            {
                FinalResult.VisibilityCalculationResults |= VisibilityCalculationresultsEnum.VisibilityAreaPolygons;
                if (checkBoxOP.Checked)
                {
                    FinalResult.VisibilityCalculationResults |= VisibilityCalculationresultsEnum.VisibilityAreaPolygonSingle;
                }
            }

            if (checkBoxOP.Checked)
            {
                FinalResult.VisibilityCalculationResults |= VisibilityCalculationresultsEnum.VisibilityAreaRasterSingle | VisibilityCalculationresultsEnum.ObservationPointSingle;
            }
        }
        public void SummaryInfo()
        {
            lblCalcType.Text = LocalizationContext.Instance.CalcTypeLocalisation[FinalResult.CalculationType];
            lblTaskName.Text = FinalResult.TaskName;
            lblDEMName.Text = FinalResult.RasterLayerName;
            lblObservObjectsSingle.Text = checkBoxOP.Checked ? LocalizationContext.Instance.YesWord : LocalizationContext.Instance.NoWord;
            lblObservObjectsAll.Text = checkBoxOP.Checked ? LocalizationContext.Instance.YesWord : LocalizationContext.Instance.NoWord;
            lblReferencedLayerName.Text = cmbPositions.SelectedItem.ToString();
            var selectedObservPoints = _observPointGuis != null ? _observPointGuis.Where(p => p.Check).Select(o => o.Title).ToArray() : null;
            var selectedObservObjects = _observationObjects != null ? _observationObjects.Where(p => p.Check).Select(o => o.Title).ToArray() : null;

            lblObservPointsSummary.Text = selectedObservPoints == null ? string.Empty : $"{selectedObservPoints.Length} - {string.Join("; ", selectedObservPoints)}";
            lblObservObjectsSummary.Text = selectedObservObjects == null ? string.Empty : $"{selectedObservObjects.Length} - {string.Join("; ", selectedObservObjects)}";

            lblTrimCalcresults.Text = !chkTrimRaster.Enabled ? string.Empty : (chkTrimRaster.Checked ? LocalizationContext.Instance.YesWord : LocalizationContext.Instance.NoWord);
        }

        public string ObservationStationFeatureClass => observObjectsLabel.Text;
        public string ObservationPointsFeatureClass => ObservPointLabel.Text;




        private void NextStepButton_Click(object sender, EventArgs e)
        {

            if (StepsTabControl.SelectedIndex == 2)
            {
                AssemblyWizardResult();
                SummaryInfo();
            }

            if (StepsTabControl.SelectedIndex == StepsTabControl.TabCount - 1)
            {
                if (!FinalResult.VisibilityCalculationResults.HasFlag(VisibilityCalculationresultsEnum.ObservationPoints) && !FinalResult.VisibilityCalculationResults.HasFlag(VisibilityCalculationresultsEnum.ObservationPointSingle))
                {
                    //TODO: Localise
                    MessageBox.Show("The is no results sources for calculating. Please, select source!", "SPPRD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (string.IsNullOrEmpty(imagesComboBox.Text))
                {
                    //TODO: Localise
                    MessageBox.Show("The Raster layer must be selected!", "SPPRD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                DialogResult = DialogResult.OK;
                this.Close();

            }
            //StepsTabControl.SelectedTab.Enabled = false;
            if (StepsTabControl.TabPages.Count - 1 == StepsTabControl.SelectedIndex)
            {
                return;
            }

            if (StepsTabControl.SelectedIndex < StepsTabControl.TabCount - 1 && StepsTabControl.SelectedIndex != 0)
            {
                StepsTabControl.SelectedTab.Enabled = false;

                var nextTab = StepsTabControl
                    .TabPages[StepsTabControl.SelectedIndex + 1] as TabPage;

                nextTab.Enabled = true;
                StepsTabControl.SelectedIndex++;
            }

        }

        private void PreviousStepButton_Click(object sender, EventArgs e)
        {
            if (StepsTabControl.SelectedIndex != 0)
            {
                StepsTabControl.SelectedTab.Enabled = false;
                var prevTab = StepsTabControl.TabPages[StepsTabControl.SelectedIndex - 1] as TabPage;
                prevTab.Enabled = true;

                StepsTabControl.SelectedIndex--;
            }
            if (StepsTabControl.SelectedIndex == 0)
            {
                panel1.Enabled = false;
            }
        }
        private void ultraButton1_Click(object sender, EventArgs e)
        {
            SecondTypePicked();
            StepsTabControl.SelectedTab.Enabled = chkTrimRaster.Enabled = chkTrimRaster.Checked = false;
            (StepsTabControl.TabPages[StepsTabControl.SelectedIndex + 1] as TabPage).Enabled = panel1.Enabled = true;
            StepsTabControl.SelectedIndex++;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            FirstTypePicked();

            StepsTabControl.SelectedTab.Enabled = false;
            (StepsTabControl.TabPages[StepsTabControl.SelectedIndex + 1] as TabPage).Enabled = panel1.Enabled = chkTrimRaster.Enabled = chkTrimRaster.Checked = true;
            StepsTabControl.SelectedIndex++;
        }
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            var nextTab = StepsTabControl.TabPages[StepsTabControl.SelectedIndex] as TabPage;
            if (!nextTab.Enabled)
            {
                e.Cancel = true;
            }
        }
      
        private void TbTransparency_Leave(object sender, EventArgs e)
        {
            if (!Int16.TryParse(tbTransparency.Text, out short res) || (res < 0 || res > 100))
            {
                MessageBox.Show($"Invalid data.\nInsert the value in the range from 0 to 100");
                tbTransparency.Text = "33";
            }
        }


        private void tbTransparency_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
