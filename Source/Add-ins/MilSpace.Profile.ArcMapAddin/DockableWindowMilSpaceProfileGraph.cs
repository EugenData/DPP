using MilSpace.Profile.SurfaceProfileChartControl;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MilSpace.Profile
{
    [Guid("80eb5b70-d4ba-476a-a107-49e96cf1b38d")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("MilSpace.Profile.DockableWindowMilSpaceProfileGraph")]
    public partial class DockableWindowMilSpaceProfileGraph : UserControl
    {
        private MilSpaceProfileGraphsController controller;

        private object Hook
        {
            get;
            set;
        }

        public DockableWindowMilSpaceProfileGraph Instance { get; }

        public DockableWindowMilSpaceProfileGraph(MilSpaceProfileGraphsController controller)
        {
            this.Instance = this;
            SetController(controller);
            controller.SetView(this);
        }

        public DockableWindowMilSpaceProfileGraph(object hook, MilSpaceProfileGraphsController controller)
        {
            InitializeComponent();

            profilesTabControl.TabPages.Clear();

            SetController(controller);
            controller.SetView(this);

            this.Hook = hook;
            this.Instance = this;
            SubscribeForEvents();
        }

        internal bool CheckTabExistance(string sessionName)
        {
            TabPage tabPage = null;
            return CheckTabExistance(sessionName, out tabPage);
        }

        private bool CheckTabExistance(string sessionName, out TabPage foundTab )
        {
         
            foundTab = null;
            foreach (TabPage tab in profilesTabControl.TabPages)
            {
                if (tab.Tag.ToString() == sessionName)
                {
                    foundTab = tab;
                    return true;
                }
            }

            return false;
        }

        internal void AddNewTab(SurfaceProfileChart surfaceProfileChart, string sessionName)
        {
            TabPage tabPage = null;
            if (CheckTabExistance(sessionName, out tabPage))
            {
                profilesTabControl.SelectedTab = tabPage;
                return;
            }

            string title = $"Graph {profilesTabControl.TabPages.Count + 1}";
            int i = 1;

            while (profilesTabControl.TabPages.ContainsKey(title))
            {
                title = $"Graph {i + 1}";
                i++;
            }

            tabPage = new TabPage(title);
            tabPage.Name = title;
            tabPage.Tag = sessionName;

            profilesTabControl.TabPages.Add(tabPage);

            var curTab = profilesTabControl.TabPages[profilesTabControl.TabCount - 1];

            surfaceProfileChart.Width = curTab.Width;
            surfaceProfileChart.Height = curTab.Height;
            surfaceProfileChart.Name = "profileChart";
            curTab.Controls.Add(surfaceProfileChart);

            profilesTabControl.SelectTab(profilesTabControl.TabCount - 1);
            controller.SetChart(surfaceProfileChart);

            surfaceProfileChart.SetControlSize();
        }

        internal void RemoveCurrentTab()
        {
            profilesTabControl.TabPages.RemoveAt(profilesTabControl.SelectedIndex);
        }

        #region AddIn Instance

        public void SetController(MilSpaceProfileGraphsController controller)
        {
            this.controller = controller;
        }

        private void SubscribeForEvents()
        {

        }

        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private DockableWindowMilSpaceProfileGraph m_windowUI;
            private MilSpaceProfileGraphsController controller;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                controller = new MilSpaceProfileGraphsController();

                m_windowUI = new DockableWindowMilSpaceProfileGraph(this.Hook, controller);


                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

            internal DockableWindowMilSpaceProfileGraph DockableWindowUI => m_windowUI;


            internal MilSpaceProfileGraphsController MilSpaceProfileGraphsController => controller;

        }
        #endregion

        private void DockableWindowMilSpaceProfileGraph_Resize(object sender, EventArgs e)
        {
            profilesTabControl.Width = this.Width;
            profilesTabControl.Height = this.Height;
        }

        private void ProfilesTabControl_Resize(object sender, EventArgs e)
        {
            foreach (TabPage page in profilesTabControl.TabPages)
            {
                page.Controls["profileChart"].Width = profilesTabControl.Width - 10;
                page.Controls["profileChart"].Height = profilesTabControl.Height - 30;
            }
        }

        private void ProfilesTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (profilesTabControl.TabPages.Count > 0)
            {
                controller.SetChart((SurfaceProfileChart)profilesTabControl.SelectedTab.Controls["profileChart"]);
            }
        }
    }
}
