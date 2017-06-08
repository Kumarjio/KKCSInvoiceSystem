﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;

namespace KKCSInvoiceProject
{
    public partial class NewCarReturns : Form
    {
        #region GlobalVariables

        string m_strDataBaseFilePath = ConfigurationManager.ConnectionStrings["DatabaseFilePath"].ConnectionString;

        private OleDbConnection connection = new OleDbConnection();

        OleDbDataReader reader;

        OleDbCommand command;

        Panel pnl;

        List<Panel> lstReturnPanels;
        List<Panel> lstUnknownPanels;

        string sList = "TodaysReturns";

        DateTime dtDate;

        //Checks to see if the car has already been picked up
        bool bPickedUp = false;

        bool bFirstTimeDividier = false;
        int iNoMoreThan1Divider = 0;

        int iInitialPanelLocationY = 0;

        #endregion

        #region Load

        public NewCarReturns()
        {
            InitializeComponent();

            connection.ConnectionString = m_strDataBaseFilePath;

            RefreshReturnDate();
        }

        #endregion

        #region CreatePanels

        void RefreshReturnDate()
        {
            // Set the initial location for the title
            iInitialPanelLocationY = pnl_template.Location.Y;

            lstReturnPanels = new List<Panel>();
            lstUnknownPanels = new List<Panel>();

            // Creates the Title Header
            TitleHeaders(0);

            // Create todays date for the query
            DateTime now = dt_timepicked.Value;
            //now = now.AddDays(-2);
            dtDate = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

            sList = "TodaysReturns";

            // Creates a query for todays returns
            string sTodaysQuerys = "select * from CustomerInvoices WHERE DTReturnDate = @dtDate ORDER BY ReturnTime,KeyNumber ASC";
            CreateReturns(sTodaysQuerys);

            iInitialPanelLocationY += 50;

            // Creates the Title Header
            TitleHeaders(1);

            sList = "UnknownReturns";

            string sUnknownQuerys = "select * from CustomerInvoices WHERE (DTReturnDate < @dtDate AND PickUp = false) ORDER BY dtReturnDate,ReturnTime,KeyNumber ASC";
            CreateReturns(sUnknownQuerys);

            iInitialPanelLocationY += 10;

            Label lblBlank = new Label();
            lblBlank.Location = new Point(0, iInitialPanelLocationY);
            Controls.Add(lblBlank);
        }

        void RefreshCarBroughtIn()
        {
            // Set the initial location for the title
            iInitialPanelLocationY = pnl_template.Location.Y;

            // Creates the Title Header
            TitleHeaders(2);

            // Create todays date for the query
            DateTime now = dt_timepicked.Value;
            //now = now.AddDays(-1);
            dtDate = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

            // Creates a query for todays returns
            string sTodaysQuerys = "select * from CustomerInvoices WHERE DTDateIn = @dtDate ORDER BY ReturnTime ASC";
            CreateReturns(sTodaysQuerys);

            iInitialPanelLocationY += 10;

            Label lblBlank = new Label();
            lblBlank.Name = "lbl_blank";
            lblBlank.Location = new Point(0, iInitialPanelLocationY);
            Controls.Add(lblBlank);
        }

        void RefreshDatePaid()
        {
            // Set the initial location for the title
            iInitialPanelLocationY = pnl_template.Location.Y;

            // Creates the Title Header
            TitleHeaders(3);

            // Create todays date for the query
            DateTime now = dt_timepicked.Value;
            //now = now.AddDays(-1);
            dtDate = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

            // Creates a query for todays returns
            string sTodaysQuerys = "select * from CustomerInvoices WHERE DTDatePaid = @dtDate ORDER BY ReturnTime ASC";
            CreateReturns(sTodaysQuerys);

            iInitialPanelLocationY += 10;

            Label lblBlank = new Label();
            lblBlank.Name = "lbl_blank";
            lblBlank.Location = new Point(0, iInitialPanelLocationY);
            Controls.Add(lblBlank);
        }

        void RefreshBadDebots()
        {
            // Set the initial location for the title
            iInitialPanelLocationY = pnl_template.Location.Y;

            // Creates the Title Header
            TitleHeaders(3);

            // Create todays date for the query
            DateTime now = dt_timepicked.Value;
            //now = now.AddDays(-1);
            dtDate = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

            // Creates a query for todays returns
            string sTodaysQuerys = "select * from CustomerInvoices WHERE PickUp = true AND PaidStatus = 'To Pay' ORDER BY DTReturnDate ASC";
            CreateReturns(sTodaysQuerys);

            iInitialPanelLocationY += 10;

            Label lblBlank = new Label();
            lblBlank.Name = "lbl_blank";
            lblBlank.Location = new Point(0, iInitialPanelLocationY);
            Controls.Add(lblBlank);
        }

        void CreateReturns(string _sQuery)
        {
            // Opens and creates the connection for the database
            connection.Open();

            // Make new OleDbCommand object
            command = new OleDbCommand();

            // Create the connection
            command.Connection = connection;

            command.CommandText = _sQuery;
            command.Parameters.AddWithValue("@dtDate", dtDate);

            reader = command.ExecuteReader();

            // Moves the location down for the first panel
            iInitialPanelLocationY += 50;

            CreatePanels();

            connection.Close();
        }

        void TitleHeaders(int _iPickTitle)
        {
            DateTime now = DateTime.Now;

            Label lblTitle = new Label();
            lblTitle.Location = new Point(pnl_template.Location.X, iInitialPanelLocationY);

            lblTitle.Font = new Font("Arial", 20, FontStyle.Bold);
            lblTitle.Size = new Size(1520, 40);
            lblTitle.BringToFront();

            string g_strDatePicked = now.DayOfWeek.ToString() + ", " +
            now.Day.ToString() + " " +
            now.ToString("MMMM") + " " +
            now.Year.ToString();

            if (_iPickTitle == 0)
            {
                lblTitle.BackColor = System.Drawing.Color.LightBlue;
                lblTitle.ForeColor = System.Drawing.Color.Black;

                lblTitle.Name = "lbl_returndate";

                g_strDatePicked += " - Return Date";

                lblTitle.Text = g_strDatePicked;
            }
            else if(_iPickTitle == 1)
            {

                lblTitle.BackColor = System.Drawing.Color.LightSalmon;
                lblTitle.ForeColor = System.Drawing.Color.Black;

                lblTitle.Name = "lbl_unknown";

                lblTitle.Text = "Unknown/Overdue";
            }
            else if(_iPickTitle == 2)
            {
                lblTitle.BackColor = System.Drawing.Color.LightBlue;
                lblTitle.ForeColor = System.Drawing.Color.Black;

                lblTitle.Name = "lbl_title";

                g_strDatePicked += " - Date Car Left In Yard";

                lblTitle.Text = g_strDatePicked;
            }
            else if (_iPickTitle == 3)
            {
                lblTitle.BackColor = System.Drawing.Color.LightBlue;
                lblTitle.ForeColor = System.Drawing.Color.Black;

                lblTitle.Name = "lbl_title";

                g_strDatePicked += " - Date Customer Paid";

                lblTitle.Text = g_strDatePicked;
            }

            Controls.Add(lblTitle);
        }

        void CreatePanels()
        {
            // Stores the time from the table
            string StoreTime = "";

            // Stores time at end to compare and see if a new time has shown
            string StoreTimeSecond = "";

            // Skips the very first check as there is no time to compare on the first
            bool bSkipFirstCheck = true;

            while (reader.Read())
            {
                // Gets the current time of the record
                StoreTime = reader["ReturnTime"].ToString();

                // Compares the 2 times together to see if they are different or not
                // Skips the first check
                if (StoreTime != StoreTimeSecond && !bSkipFirstCheck)
                {
                    iInitialPanelLocationY += 50;

                    if (sList == "TodaysReturns" && bFirstTimeDividier && iNoMoreThan1Divider < 1)
                    {
                        Panel pnl = new Panel();
                        pnl.Name = "pnlDivider";
                        lstReturnPanels.Add(pnl);

                        iNoMoreThan1Divider++;
                    }

                    if (sList == "UnknownReturns" & bFirstTimeDividier && iNoMoreThan1Divider < 1)
                    {
                        Panel pnl = new Panel();
                        pnl.Name = "pnlDivider";
                        lstUnknownPanels.Add(pnl);

                        iNoMoreThan1Divider++;
                    }
                }

                // Creates the labels for that record
                CreateIndividualPanel();

                // Makes the Second time = the first time for comparision purposes
                StoreTimeSecond = StoreTime;

                // Makes the first check to false for using
                bSkipFirstCheck = false;
            }
        }

        void CreateIndividualPanel()
        {
            pnl = new Panel();
            pnl.Name = reader["InvoiceNumber"].ToString();

            pnl.Location = new Point(pnl_template.Location.X, iInitialPanelLocationY);
            iInitialPanelLocationY += 50;

            pnl.Size = pnl_template.Size;
            pnl.BackColor = pnl_template.BackColor;
            pnl.BorderStyle = pnl_template.BorderStyle;

            // Checks to see if the car has been picked up or not already
            bPickedUp = (bool)reader["PickUp"];

            // Handles the controls within the panel
            foreach (Control p in pnl_template.Controls)
            {
                // Handles all the button controls
                if (p.GetType() == typeof(Button))
                {
                    ControlButtons(p);
                }
                // Handles all the Label Controlls
                if (p.GetType() == typeof(Label))
                {
                    ControlLabels(p);
                }
            }

            ExtraLabelsForPrinting();

            if (sList == "TodaysReturns")
            {
                if(!bPickedUp)
                {
                    lstReturnPanels.Add(pnl);

                    bFirstTimeDividier = true;

                    iNoMoreThan1Divider = 0;
                }
            }
            else if(sList == "UnknownReturns")
            {
                lstUnknownPanels.Add(pnl);

                bFirstTimeDividier = true;

                iNoMoreThan1Divider = 0;
            }

            Controls.Add(pnl);
        }

        void ControlButtons(Control _p)
        {
            // Creates a new button
            Button btn = new Button();

            // Is it the Picked Up Button
            if (_p.Name == "btn_pickedup")
            {
                // The car has already been picked up
                if (bPickedUp)
                {
                    btn.Text = "Yes";
                    btn.BackColor = Color.Green;
                    pnl.BackColor = Color.LightGreen;
                }
                // The car has not been picked up
                else
                {
                    btn.Text = "No";
                    btn.BackColor = Color.Red;

                    btn.Click += new EventHandler(PickUpStatus_Click);
                }

                btn.Name = reader["InvoiceNumber"].ToString();
            }

            // Is it the Invoice No Button
            if (_p.Name == "btn_invno")
            {
                btn.Text = reader["InvoiceNumber"].ToString();
                btn.BackColor = _p.BackColor;

                btn.Name = reader["InvoiceNumber"].ToString();

                btn.Click += new EventHandler(InvoiceButton_Click);
            }

            if (_p.Name == "btn_notes")
            {
                if (reader["Notes"].ToString() != "")
                {
                    btn.BackColor = _p.BackColor;
                    btn.Text = _p.Text;
                    btn.ForeColor = _p.ForeColor;
                    btn.Visible = true;

                    btn.Name = reader["InvoiceNumber"].ToString();

                    btn.Click += new EventHandler(NotesButton_Click);
                }
                else
                {
                    btn.Visible = false;
                }
            }

            if (_p.Name == "btn_alerts")
            {
                if (reader["Alerts"].ToString() != "")
                {
                    btn.BackColor = _p.BackColor;
                    btn.Text = _p.Text;
                    btn.ForeColor = _p.ForeColor;
                    btn.Visible = true;

                    btn.Name = reader["InvoiceNumber"].ToString();

                    btn.Click += new EventHandler(AlertsButton_Click);
                }
                else
                {
                    btn.Visible = false;
                }
            }

            btn.Location = _p.Location;
            btn.Size = _p.Size;
            pnl.Controls.Add(btn);
        }

        void ControlLabels(Control _p)
        {
            Label lbl = new Label();

            lbl.Name = _p.Name;

            // Handles the customer name
            if (_p.Name == "lbl_customername")
            {
                string sNameLength = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                string sStoreName = "";

                if (sNameLength.Length > 21)
                {
                    lbl.Font = new Font(_p.Font.FontFamily, 8);
                    lbl.Size = new Size(_p.Size.Width, _p.Size.Height + 10);
                    sStoreName = reader["FirstName"].ToString() + "\r\n" + reader["LastName"].ToString();

                    lbl.Text = sStoreName;
                }
                else
                {
                    lbl.Font = _p.Font;
                    lbl.Size = _p.Size;

                    lbl.Text = sNameLength;
                }

                if (bPickedUp)
                {
                    lbl.BackColor = Color.LightGreen;
                }
            }

            if (_p.Name == "lbl_rego")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                lbl.Text = reader["Rego"].ToString();
            }

            if (_p.Name == "lbl_keyno")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;
                lbl.BackColor = _p.BackColor;

                lbl.Text = reader["KeyNumber"].ToString();
            }

            if (_p.Name == "lbl_amount")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;
                lbl.BackColor = _p.BackColor;

                float fTotalPay = 0.0f;
                float.TryParse(reader["TotalPay"].ToString(), out fTotalPay);

                lbl.Text = "$" + fTotalPay.ToString("0.00");

                // Makes the background colour of the label green to fit with the panel colour
                if (bPickedUp)
                {
                    lbl.BackColor = Color.LightGreen;
                }
            }

            if (_p.Name == "lbl_paidstatus")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                string sPaidStatus = reader["PaidStatus"].ToString();

                if (sPaidStatus == "To Pay")
                {
                    lbl.BackColor = Color.Yellow;
                }
                else if (sPaidStatus == "OnAcc" || sPaidStatus == "N/A")
                {
                    lbl.BackColor = Color.Violet;
                }
                else
                {
                    lbl.BackColor = _p.BackColor;
                }

                lbl.Text = reader["PaidStatus"].ToString();
            }

            if (_p.Name == "lbl_returntime")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                // Makes the background colour of the label green to fit with the panel colour
                if (bPickedUp)
                {
                    lbl.BackColor = Color.LightGreen;
                }

                lbl.Text = reader["ReturnTime"].ToString();
            }

            if (_p.Name == "lbl_returndate")
            {
                bool bIsUnknown = (bool)reader["UnknownDate"];

                if (!bIsUnknown)
                {
                    DateTime dt = (DateTime)reader["DTReturnDate"];

                    string Date = dt.ToString("ddd").ToUpper() + ", " +
                    dt.Day.ToString() + "-" +
                    dt.ToString("MM") + "-" +
                    dt.ToString("yy");

                    lbl.Text = Date;
                }
                else
                {
                    lbl.Text = "Unknown";
                }

                lbl.Font = _p.Font;
                lbl.Size = _p.Size;
            }

            if(_p.Name == "lbl_ph")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                if (bPickedUp)
                {
                    lbl.BackColor = Color.LightGreen;
                }

                lbl.Text = reader["PhoneNumber"].ToString();
            }

            if(_p.Name == "lbl_make")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                lbl.Text = reader["MakeModel"].ToString();

                // Makes the background colour of the label green to fit with the panel colour
                if (bPickedUp)
                {
                    lbl.BackColor = Color.LightGreen;
                }
            }

            if (_p.Name == "lbl_location")
            {
                lbl.Font = _p.Font;
                lbl.Size = _p.Size;

                lbl.Text = reader["CarLocation"].ToString();

                if(reader["CarLocation"].ToString() == "Front")
                {
                    lbl.BackColor = Color.GreenYellow;
                }
                else
                {
                    lbl.BackColor = Color.Red;
                }
            }

            lbl.Location = _p.Location;

            pnl.Controls.Add(lbl);
        }

        void ExtraLabelsForPrinting()
        {
            Label lblInvNo = new Label();
            lblInvNo.Name = "lbl_InvNo";
            lblInvNo.Text = reader["InvoiceNumber"].ToString();
            lblInvNo.Location = new Point(-100, -100);
            pnl.Controls.Add(lblInvNo);

            Label lblDateIn = new Label();
            lblDateIn.Name = "lbl_DateIn";
            lblDateIn.Location = new Point(-100, -100);

            Label lblReturnDate = new Label();
            lblReturnDate.Name = "lbl_ReturnDate";
            lblReturnDate.Location = new Point(-100, -100);

            DateTime dtDateIn = (DateTime)reader["DTDateIn"];
            DateTime dtReturnTime = (DateTime)reader["DTReturnDate"];

            string sTimeIn = reader["TimeIn"].ToString();
            string sReturnTime = reader["ReturnTime"].ToString();

            lblDateIn.Text = dtDateIn.Day + "/" + dtDateIn.Month + "/" + dtDateIn.Year + " - " + sTimeIn;
            lblReturnDate.Text = dtReturnTime.Day + "/" + dtReturnTime.Month + "/" + dtReturnTime.Year + " - " + sReturnTime;

            pnl.Controls.Add(lblDateIn);
            pnl.Controls.Add(lblReturnDate);
        }

        #endregion

        #region ButtonClicks

        int m_iInvoiceNumber = 0;

        #region PickUp
        private void PickUpStatus_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            m_iInvoiceNumber = 0;
            Int32.TryParse(btn.Name, out m_iInvoiceNumber);

            if (btn.Text == "No")
            {
                SetPickUp();
            }
        }

        void SetPickUp()
        {
            string sTabsStillOpen = "";
            sTabsStillOpen = "Has the car being picked up,\r\nand do you wish to release the keybox number?";

            WarningSystem ws = new WarningSystem(sTabsStillOpen, true);
            ws.ShowDialog();

            if (ws.DialogResult == DialogResult.OK)
            {
                connection.Open();

                command = new OleDbCommand();

                command.Connection = connection;

                command.CommandText = @"UPDATE CustomerInvoices
                                        SET [PickUp] = True
                                        WHERE [InvoiceNumber] = " + m_iInvoiceNumber + "";

                command.ExecuteNonQuery();

                connection.Close();

                ReloadPageFromInvoice();
            }
        }

        public void ReloadPageFromInvoice()
        {
            // Stores the original position of the scroll bar to restore after refreshing
            int iOriPosOfScrollBar = VerticalScroll.Value;

            if (chk_returndate.Checked)
            {
                DeleteControls();

                RefreshReturnDate();
            }
            else if (chk_datebroughtin.Checked)
            {
                DeleteControls();

                RefreshCarBroughtIn();
            }
            else if (chk_datepaid.Checked)
            {
                DeleteControls();

                RefreshDatePaid();
            }

            // Restores the original location of the vertical scroll bar
            VerticalScroll.Value = iOriPosOfScrollBar;
        }        
        
        #endregion

        #region AlertAndNotesButton
        private void AlertsButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            connection.Open();

            command = new OleDbCommand();

            command.Connection = connection;

            int x = 0;
            Int32.TryParse(btn.Name, out x);

            string query = @"SELECT Alerts FROM CustomerInvoices
                             WHERE InvoiceNumber = " + x + "";

             command.CommandText = query;

            reader = command.ExecuteReader();

            while (reader.Read())
            {
                string tempStr = reader["Alerts"].ToString();
                MessageBox.Show(tempStr, "Alert");

                break;
            }

            connection.Close();
        }

        private void NotesButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            connection.Open();

            command = new OleDbCommand();

            command.Connection = connection;

            int x = 0;
            Int32.TryParse(btn.Name, out x);

            string query = @"SELECT Notes FROM CustomerInvoices
                             WHERE InvoiceNumber = " + x + "";

            command.CommandText = query;

            reader = command.ExecuteReader();

            while (reader.Read())
            {
                string tempStr = reader["Notes"].ToString();
                MessageBox.Show(tempStr, "Note");

                break;
            }

            connection.Close();
        }
        #endregion

        #region InvoiceButton
        private void InvoiceButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            int x = 0;
            Int32.TryParse(btn.Name, out x);

            Invoice inv = new Invoice(true);

            inv.SetUpFromCarReturns(x, this);

            inv.Show();
        }
        #endregion

        #endregion

        #region ButtonsOthers

        private void btn_left_Click(object sender, EventArgs e)
        {
            DateTime dtStore = dt_timepicked.Value;

            dtStore = dtStore.AddDays(-1);

            dt_timepicked.Value = dtStore;
        }

        private void btn_right_Click(object sender, EventArgs e)
        {
            DateTime dtStore = dt_timepicked.Value;

            dtStore = dtStore.AddDays(1);

            dt_timepicked.Value = dtStore;
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift) && ModifierKeys.HasFlag(Keys.Control) && ModifierKeys.HasFlag(Keys.Alt))
            {
                WarningSystem ws = new WarningSystem("Would you like to sort the return list by bad debtors?", true);
                ws.ShowDialog();

                if(ws.DialogResult == DialogResult.OK)
                {
                    DeleteControls();

                    RefreshBadDebots();
                }
            }
            else
            {
                if (chk_returndate.Checked)
                {
                    DeleteControls();

                    RefreshReturnDate();
                }
                else if (chk_datebroughtin.Checked)
                {
                    DeleteControls();

                    RefreshCarBroughtIn();
                }
                else if (chk_datepaid.Checked)
                {
                    DeleteControls();

                    RefreshDatePaid();
                }
            }
        }

        #endregion

        #region CheckBoxes

        private void chk_returndate_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_returndate.Checked == true)
            {
                chk_datebroughtin.Checked = false;
                chk_datepaid.Checked = false;

                DeleteControls();

                RefreshReturnDate();
            }
        }

        private void chk_datebroughtin_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_datebroughtin.Checked == true)
            {
                chk_returndate.Checked = false;
                chk_datepaid.Checked = false;

                DeleteControls();

                RefreshCarBroughtIn();
            }
        }

        private void chk_datepaid_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_datepaid.Checked == true)
            {
                chk_returndate.Checked = false;
                chk_datebroughtin.Checked = false;

                DeleteControls();

                RefreshDatePaid();
            }
        }

        #endregion

        void DeleteControls()
        {
            // Deletes all the buttons in the table apart from the Load button
            foreach (Panel pnl in this.Controls.OfType<Panel>().ToArray())
            {
                if (pnl.Name == "pnl_template")
                {
                    // Do Nothing
                }
                else
                {
                    Controls.Remove(pnl);
                }
            }

            // Deletes all the buttons in the table apart from the Load button
            foreach (Label lbl in this.Controls.OfType <Label>().ToArray())
            {
                if (lbl.Text == "Unknown/Overdue" || lbl.Name == "lbl_blank" || lbl.Name == "lbl_returndate" 
                    || lbl.Name == "lbl_unknown" || lbl.Name == "lbl_title")
                {
                    Controls.Remove(lbl);
                }
            }
        }

        private void dt_timepicked_ValueChanged(object sender, EventArgs e)
        {
            if (chk_returndate.Checked)
            {
                DeleteControls();

                RefreshReturnDate();
            }
            else if (chk_datebroughtin.Checked)
            {
                DeleteControls();

                RefreshCarBroughtIn();
            }
            else if(chk_datepaid.Checked)
            {
                DeleteControls();

                RefreshDatePaid();
            }
        }

        #region ButtonShortcuts

        private void btn_mainmenu_Click(object sender, EventArgs e)
        {
            Form fm = Application.OpenForms["MainMenu"];

            if (fm.WindowState == FormWindowState.Minimized)
            {
                fm.WindowState = FormWindowState.Normal;
            }

            fm.BringToFront();
        }

        private void btn_keybox_Click(object sender, EventArgs e)
        {
            Form fm = Application.OpenForms["KeyBox"];

            if (fm != null)
            {
                if (fm.WindowState == FormWindowState.Minimized)
                {
                    fm.WindowState = FormWindowState.Normal;
                }

                fm.BringToFront();
            }
            else
            {
                KeyBox kb = new KeyBox();
                kb.Show();
            }
        }

        private void btn_invoice_Click(object sender, EventArgs e)
        {
            Form fm = Application.OpenForms["InvoiceManager"];

            if (fm != null)
            {
                if (fm.WindowState == FormWindowState.Minimized)
                {
                    fm.WindowState = FormWindowState.Maximized;
                }

                fm.BringToFront();
            }
            else
            {
                InvoiceManager ip = new InvoiceManager();
                ip.Show();
            }
        }

        #endregion

        /*
            while (reader.Read())
            {
                // Gets the current time of the record
                StoreTime = reader["ReturnTime"].ToString();

                // Compares the 2 times together to see if they are different or not
                // Skips the first check
                if (StoreTime != StoreTimeSecond && !bSkipFirstCheck)
                {
                    iInitialPanelLocationY += 50;
                }

                // Creates the labels for that record
                CreateIndividualPanel();

                // Makes the Second time = the first time for comparision purposes
                StoreTimeSecond = StoreTime;

                // Makes the first check to false for using
                bSkipFirstCheck = false;
            }
        */

        #region Printing

        #region GlobalVariables
        int iLocationY = 50;
        int iItemsPerPage = 0;

        int iListCount = 0;

        int iPageNumber = 1;

        Brush blackBrush = new SolidBrush(Color.Black);
        #endregion

        #region PrintReturns

        public void PrintReturns()
        {
            iLocationY = 50;
            iItemsPerPage = 0;

            iListCount = 0;

            iPageNumber = 1;

            pnl_printtitles.Visible = true;

            PrintDocument PrintDocument = new PrintDocument();

            PaperSize ps = new PaperSize();
            ps.RawKind = (int)PaperKind.A4;

            PrintDocument.DefaultPageSettings.PaperSize = ps;

            //PrintDocument.PrinterSettings.PrinterName = "Adobe PDF";
            //PrintDocument.PrinterSettings.PrinterName = "CutePDF Writer";
            PrintDocument.OriginAtMargins = false;
            PrintDocument.DefaultPageSettings.Landscape = true;
            PrintDocument.PrintPage += new PrintPageEventHandler(doc_PrintReturnsPage);

            PrintDocument.Print();

            pnl_printtitles.Visible = false;

           PrintUnknowns();
        }

        private void doc_PrintReturnsPage(object sender, PrintPageEventArgs e)
        {
            #region TodaysDate
            // Creates Todays Date
            DateTime dtTodaysDate = DateTime.Now;
            string g_strDatePicked = "";

            g_strDatePicked = dtTodaysDate.DayOfWeek.ToString() + ", " +
            dtTodaysDate.Day.ToString() + " " +
            dtTodaysDate.ToString("MMMM") + " " +
            dtTodaysDate.Year.ToString();

            g_strDatePicked += " - Page " + iPageNumber.ToString();

            e.Graphics.FillRectangle(Brushes.LightBlue, 5, 7, 1150, 30);
            e.Graphics.DrawString(g_strDatePicked, new Font("Courier New", 20), new SolidBrush(Color.Black), 500, 7);
            #endregion

            iPageNumber++;

            NewPrintHeader(e);

            PrintLines(e);

            while (iListCount < lstReturnPanels.Count)
            {
                PrintingPanels(e, iListCount);

                iListCount++;

                if (iItemsPerPage < 16)
                {
                    iItemsPerPage++;
                    e.HasMorePages = false;
                }
                else
                {
                    iItemsPerPage = 0;
                    e.HasMorePages = true;

                    iLocationY = 50;

                    return;
                }
            }

            e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 4000, 4000);
        }

        void NewPrintHeader(PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(pnl_printtitles.Width, pnl_printtitles.Height, pnl_printtitles.CreateGraphics());
            pnl_printtitles.DrawToBitmap(bmp, new Rectangle(0, 0, pnl_printtitles.Width, pnl_printtitles.Height));

            RectangleF bounds = e.PageSettings.PrintableArea;
            float factor = ((float)bmp.Height / (float)bmp.Width);

            float fSize = 1.4f;

            e.Graphics.DrawImage(bmp, bounds.Left, bounds.Top + iLocationY, bounds.Width * fSize, factor * (bounds.Width * fSize));

            iLocationY += 40;
        }

        void PrintingPanels(PrintPageEventArgs e, int _iReturnPanel)
        {
            if (lstReturnPanels[_iReturnPanel].Name == "pnlDivider")
            {
                e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 1200, 40);

                iLocationY += 40;
            }
            else
            {
                foreach (Control pReturns in lstReturnPanels[_iReturnPanel].Controls)
                {
                    switch (pReturns.Name)
                    {
                        case "lbl_customername":
                            DrawString(e, pReturns, lbl_printname, true);
                            break;
                        case "lbl_rego":
                            DrawString(e, pReturns, lbl_printrego, false);
                            break;
                        case "lbl_make":
                            DrawString(e, pReturns, lbl_printmake, true);
                            break;
                        case "lbl_location":
                            DrawString(e, pReturns, lbl_printlocation, false);
                            break;
                        case "lbl_InvNo":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printinvno, false);
                            break;
                        case "lbl_keyno":
                            DrawString(e, pReturns, lbl_printkeyno, false);
                            break;
                        case "lbl_amount":
                            DrawString(e, pReturns, lbl_printamount, false);
                            break;
                        case "lbl_paidstatus":
                            DrawString(e, pReturns, lbl_printpaid, true);
                            break;
                        case "lbl_ph":
                            DrawString(e, pReturns, lbl_printphone, false);
                            break;
                        case "lbl_DateIn":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printdatein, false);
                            break;
                        case "lbl_ReturnDate":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printreturndate, false);
                            break;
                        default:
                            break;
                    }
                }

                iLocationY += 40;
            }
        }

        void DrawString(PrintPageEventArgs _e, Control _pReturns, Label _Label, bool _bCheckFontSize)
        {
            //_e.Graphics.FillRectangle(Brushes.Black, _Label.Bounds.Location.X, iLocationY + 2, 40, 40);

            PrintHorizontalLine(_e, 0);

            PointF pf = new PointF(_Label.Bounds.Location.X, _pReturns.Bounds.Location.Y + iLocationY);
            Font f = new Font("Microsoft Sans Serif", 12, FontStyle.Regular);

            if (_bCheckFontSize)
            {
                if (_pReturns.Text.Length > 10)
                {
                    f = new Font("Microsoft Sans Serif", 8, FontStyle.Regular);
                }
            }
            //blackBrush = new SolidBrush(Color.White);
            _e.Graphics.DrawString(_pReturns.Text, f, blackBrush, pf);

            PrintHorizontalLine(_e, 40);
        }

        void PrintLineLocations(int _iX1, int _iY1, int _iX2, int _iY2, PrintPageEventArgs _e)
        {
            Pen blackPen = new Pen(Color.Black);
            blackPen.Width = 1.5f;

            PointF line = new PointF(_iX1, _iY1);
            PointF line2 = new PointF(_iX2, _iY2);

            _e.Graphics.DrawLine(blackPen, line, line2);
        }

        void PrintLines(PrintPageEventArgs _e)
        {
            PrintLineLocations(10, 90, 10, 1600, _e);
            PrintLineLocations(170, 90, 170, 1600, _e);

            PrintLineLocations(280, 90, 280, 1600, _e);
            PrintLineLocations(400, 90, 400, 1600, _e);

            PrintLineLocations(463, 90, 463, 1600, _e);
            PrintLineLocations(515, 90, 515, 1600, _e);

            PrintLineLocations(550, 90, 550, 1600, _e);
            PrintLineLocations(630, 90, 630, 1600, _e);

            PrintLineLocations(705, 90, 705, 1600, _e);
            PrintLineLocations(865, 90, 865, 1600, _e);

            PrintLineLocations(1020, 90, 1020, 1600, _e);
        }

        void PrintHorizontalLine(PrintPageEventArgs _e, int _iMove)
        {
            Pen blackPen = new Pen(Color.Black);
            blackPen.Width = 1.5f;

            PointF line = new PointF(10, iLocationY + _iMove);
            PointF line2 = new PointF(1300, iLocationY + _iMove);

            _e.Graphics.DrawLine(blackPen, line, line2);
        }

        #endregion

        #region PrintUnknowns

        void PrintUnknowns()
        {
            iLocationY = 50;
            iItemsPerPage = 0;

            iListCount = 0;

            iPageNumber = 1;

            pnl_printtitles.Visible = true;

            PrintDocument PrintDocument = new PrintDocument();

            PaperSize ps = new PaperSize();
            ps.RawKind = (int)PaperKind.A4;

            PrintDocument.DefaultPageSettings.PaperSize = ps;

            PrintDocument.OriginAtMargins = false;
            PrintDocument.DefaultPageSettings.Landscape = true;
            PrintDocument.PrintPage += new PrintPageEventHandler(doc_PrintUnknownsPage);

            PrintDocument.Print();

            pnl_printtitles.Visible = false;
        }

        private void doc_PrintUnknownsPage(object sender, PrintPageEventArgs e)
        {
            string g_strDatePicked = "Unknown/Overdue - Page " + iPageNumber.ToString();

            e.Graphics.FillRectangle(Brushes.LightBlue, 5, 7, 1150, 30);
            e.Graphics.DrawString(g_strDatePicked, new Font("Courier New", 20), new SolidBrush(Color.Black), 500, 7);

            iPageNumber++;

            NewPrintHeader(e);

            PrintLines(e);

            while (iListCount < lstUnknownPanels.Count)
            {
                PrintingUnknownPanels(e, iListCount);

                iListCount++;

                if (iItemsPerPage < 16)
                {
                    iItemsPerPage++;
                    e.HasMorePages = false;
                }
                else
                {
                    iItemsPerPage = 0;
                    e.HasMorePages = true;

                    iLocationY = 50;

                    return;
                }
            }

            e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 4000, 4000);
        }

        void PrintingUnknownPanels(PrintPageEventArgs e, int _iReturnPanel)
        {
            if (lstUnknownPanels[_iReturnPanel].Name == "pnlDivider")
            {
                e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 1200, 40);

                iLocationY += 40;
            }
            else
            {
                foreach (Control pReturns in lstUnknownPanels[_iReturnPanel].Controls)
                {
                    switch (pReturns.Name)
                    {
                        case "lbl_customername":
                            DrawString(e, pReturns, lbl_printname, true);
                            break;
                        case "lbl_rego":
                            DrawString(e, pReturns, lbl_printrego, false);
                            break;
                        case "lbl_make":
                            DrawString(e, pReturns, lbl_printmake, true);
                            break;
                        case "lbl_location":
                            DrawString(e, pReturns, lbl_printlocation, false);
                            break;
                        case "lbl_InvNo":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printinvno, false);
                            break;
                        case "lbl_keyno":
                            DrawString(e, pReturns, lbl_printkeyno, false);
                            break;
                        case "lbl_amount":
                            DrawString(e, pReturns, lbl_printamount, false);
                            break;
                        case "lbl_paidstatus":
                            DrawString(e, pReturns, lbl_printpaid, true);
                            break;
                        case "lbl_ph":
                            DrawString(e, pReturns, lbl_printphone, false);
                            break;
                        case "lbl_DateIn":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printdatein, false);
                            break;
                        case "lbl_ReturnDate":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printreturndate, false);
                            break;
                        default:
                            break;
                    }
                }

                iLocationY += 40;
            }
        }

        #endregion PrintUnknowns

        #region BadDebotors

        void PrintBadDebt()
        {
            iLocationY = 50;
            iItemsPerPage = 0;

            iListCount = 0;

            iPageNumber = 1;

            pnl_printtitles.Visible = true;

            PrintDocument PrintDocument = new PrintDocument();

            PaperSize ps = new PaperSize();
            ps.RawKind = (int)PaperKind.A4;

            PrintDocument.DefaultPageSettings.PaperSize = ps;

            PrintDocument.OriginAtMargins = false;
            PrintDocument.DefaultPageSettings.Landscape = true;
            PrintDocument.PrintPage += new PrintPageEventHandler(doc_PrintBadDebtPage);

            PrintDocument.Print();

            pnl_printtitles.Visible = false;
        }

        private void doc_PrintBadDebtPage(object sender, PrintPageEventArgs e)
        {
            string g_strDatePicked = "Bad Debtors - Page " + iPageNumber.ToString();

            e.Graphics.FillRectangle(Brushes.LightBlue, 5, 7, 1150, 30);
            e.Graphics.DrawString(g_strDatePicked, new Font("Courier New", 20), new SolidBrush(Color.Black), 500, 7);

            iPageNumber++;

            NewPrintHeader(e);

            PrintLines(e);

            while (iListCount < lstUnknownPanels.Count)
            {
                PrintingUnknownPanels(e, iListCount);

                iListCount++;

                if (iItemsPerPage < 16)
                {
                    iItemsPerPage++;
                    e.HasMorePages = false;
                }
                else
                {
                    iItemsPerPage = 0;
                    e.HasMorePages = true;

                    iLocationY = 50;

                    return;
                }
            }

            e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 4000, 4000);
        }

        void PrintingBadDebtPanels(PrintPageEventArgs e, int _iReturnPanel)
        {
            if (lstUnknownPanels[_iReturnPanel].Name == "pnlDivider")
            {
                e.Graphics.FillRectangle(Brushes.White, 0, iLocationY + 2, 1200, 40);

                iLocationY += 40;
            }
            else
            {
                foreach (Control pReturns in lstUnknownPanels[_iReturnPanel].Controls)
                {
                    switch (pReturns.Name)
                    {
                        case "lbl_customername":
                            DrawString(e, pReturns, lbl_printname, true);
                            break;
                        case "lbl_rego":
                            DrawString(e, pReturns, lbl_printrego, false);
                            break;
                        case "lbl_make":
                            DrawString(e, pReturns, lbl_printmake, true);
                            break;
                        case "lbl_location":
                            DrawString(e, pReturns, lbl_printlocation, false);
                            break;
                        case "lbl_InvNo":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printinvno, false);
                            break;
                        case "lbl_keyno":
                            DrawString(e, pReturns, lbl_printkeyno, false);
                            break;
                        case "lbl_amount":
                            DrawString(e, pReturns, lbl_printamount, false);
                            break;
                        case "lbl_paidstatus":
                            DrawString(e, pReturns, lbl_printpaid, true);
                            break;
                        case "lbl_ph":
                            DrawString(e, pReturns, lbl_printphone, false);
                            break;
                        case "lbl_DateIn":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printdatein, false);
                            break;
                        case "lbl_ReturnDate":
                            pReturns.Location = new Point(pReturns.Location.X, 10);
                            DrawString(e, pReturns, lbl_printreturndate, false);
                            break;
                        default:
                            break;
                    }
                }

                iLocationY += 40;
            }
        }

        #endregion BadDebotors

        #endregion
    }
}