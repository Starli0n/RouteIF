﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace RouteIF
{
    public partial class RouteIF : Form
    {
        private string m_sFavorite;
        private BindingList<NetInterface> m_bindingList = new BindingList<NetInterface>();
        private Dictionary<string, string> m_DescriptionToDefaultGateway = new Dictionary<string, string>();
        private FormBorderStyle m_initialFormBorderStyle;
        private bool m_bChanging;

        public RouteIF()
        {
            InitializeComponent();
            m_sFavorite = ConfigurationManager.AppSettings["Favorite"];
            m_cbNetInterface.DisplayMember = "Description";
            m_cbNetInterface.ValueMember = "DefaultGateway";
            m_cbNetInterface.DataSource = m_bindingList;
            m_toolStripComboBox.ComboBox.DisplayMember = m_cbNetInterface.DisplayMember;
            m_toolStripComboBox.ComboBox.ValueMember = m_cbNetInterface.ValueMember;
            m_toolStripComboBox.ComboBox.DataSource = m_cbNetInterface.DataSource;
            m_toolStripComboBox.ComboBox.BindingContext = BindingContext;
            m_initialFormBorderStyle = FormBorderStyle;
            m_bChanging = false;
        }

        private NetInterface GetNetInterfaceByDescription(string sDescription)
        {
            return m_bindingList.Single(s => s.Description == sDescription);
        }

        private NetInterface GetNetInterfaceByDefaultGetway(string sDefaultGetway)
        {
            if (string.IsNullOrEmpty(sDefaultGetway))
                return m_bindingList.FirstOrDefault();
            else
                return m_bindingList.Single(s => s.DefaultGateway == sDefaultGetway);
        }

        private void Reload()
        {
            m_bindingList.Clear();
            foreach (NetInterface ni in Command.GetNetInterfaces())
            {
                if (string.IsNullOrEmpty(ni.DefaultGateway))
                {
                    if (m_DescriptionToDefaultGateway.ContainsKey(ni.Description))
                        ni.DefaultGateway = m_DescriptionToDefaultGateway[ni.Description];
                    else
                    {
                        MessageBox.Show("Default Gateway is empty for:\n  " + ni.Description, "Default Gateway",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }
                }
                else
                    m_DescriptionToDefaultGateway[ni.Description] = ni.DefaultGateway;
                m_bindingList.Add(ni);
            }
            string sDefaultGetway = Command.GetDefaultGetway();
            NetInterface netInterface = GetNetInterfaceByDefaultGetway(sDefaultGetway);
            ChangeSelection(m_cbNetInterface.FindString(netInterface.Description), string.IsNullOrEmpty(sDefaultGetway) ? netInterface : null);
        }

        private void OnResize(FormWindowState windowState)
        {
            if (FormWindowState.Minimized == windowState)
            {
                m_notifyIcon.Visible = true;
                Hide();
                ShowInTaskbar = false;
                m_toolStripComboBox.Visible = true;
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
            }
            else if (FormWindowState.Normal == windowState)
            {
                m_notifyIcon.Visible = false;
                ShowInTaskbar = true;
                m_toolStripComboBox.Visible = false;
                FormBorderStyle = m_initialFormBorderStyle;
            }
        }

        private void ChangeSelection(int index, NetInterface selectedNetInterface = null)
        {
            m_bChanging = true;
            if (selectedNetInterface != null)
            {
                if (!Command.SetDefaultGetway(selectedNetInterface.DefaultGateway))
                    MessageBox.Show("Default Gateway is not set correctly to:\n  " + selectedNetInterface.Description, "Default Gateway",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            m_cbNetInterface.SelectedIndex = index;
            m_toolStripComboBox.ComboBox.SelectedIndex = index;
            m_bChanging = false;
        }

        private void RouteIF_Load(object sender, EventArgs e)
        {
            Reload();
        }

        private void RouteIF_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(m_sFavorite))
                return;
            NetInterface netInterface = (NetInterface)m_cbNetInterface.SelectedItem;
            if (m_sFavorite != netInterface.Description)
            {
                netInterface = GetNetInterfaceByDescription(m_sFavorite);
                ChangeSelection(m_cbNetInterface.FindString(m_sFavorite), netInterface);
            }
        }

        private void m_cbNetInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bChanging)
                return;
            ChangeSelection(m_cbNetInterface.SelectedIndex, (NetInterface) m_cbNetInterface.SelectedItem);
        }

        private void m_toolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bChanging)
                return;
            ChangeSelection(m_toolStripComboBox.SelectedIndex, (NetInterface) m_toolStripComboBox.SelectedItem);
        }

        private void RouteIF_Resize(object sender, EventArgs e)
        {
            OnResize(WindowState);
        }

        private void m_notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            OnResize(WindowState = FormWindowState.Normal);
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reload();
        }
    }
}
