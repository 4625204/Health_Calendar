﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Health_Calendar
{
    public partial class MainWindow : Form
    {
        private DateTime activeDate;
        private DailyRecord activeRecord;
        private List<DietPanel> dietPanels = new List<DietPanel>();
        public MainWindow()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 600);
            MaximumSize = new Size(800, 600);

            Exercise.selectAllExercise();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            mainTabControl.Size = new Size(720, 455);
            mainTabControl.ItemSize = new Size(150, 25);
            mainTabControl.Location = new Point((this.Size.Width - mainTabControl.Size.Width)/2 - 15, (this.Size.Height - mainTabControl.Size.Height) / 2);
            calendarTabControl.Size = new Size(720, 455);
            calendarTabControl.Location = new Point(0, 0);
            generate_calendar(2018, 6);
            calendarTabControl.ItemSize = new Size(0, 1);
            calendarTabControl.SizeMode = TabSizeMode.Fixed;
        }

        private void mainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            StringFormat StrFormat = new StringFormat();
            StrFormat.LineAlignment = StringAlignment.Center;
            StrFormat.Alignment = StringAlignment.Center;
            Font font = new Font("新細明體", 12, GraphicsUnit.Pixel);
            string text = mainTabControl.TabPages[e.Index].Text;
            Rectangle rect = mainTabControl.GetTabRect(e.Index);
            g.DrawString(text, mainTabControl.Font, Brushes.Black, rect, StrFormat);
        }

        private void generate_calendar(int year, int month)
        {
            int[] W = new int[12] { 6, 2, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };
            int[] D = new int[12] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int w = W[month - 1] + year + (year / 4) - (year / 100) + (year / 400);
            int X = 25, Y = 60;
            if (((year % 4) == 0) && (month < 3))
            {
                w--;
                D[1]++;
                if ((year % 100) == 0)
                {
                    w++;
                    D[1]--;
                }
                if ((year % 400) == 0)
                {
                    w--;
                    D[1]++;
                }
            }

            for (int i = 1, j = 0; i <= D[month - 1]; i++)
            {
                DateButton dateBtn = new DateButton(X + 90 * ((w + i) % 7), Y + 70 * j, new DateTime(year, month, i), "");
                dateBtn.Click += (sender, e) => DateButton_Click(sender, e, dateBtn.date);
                calendarPage.Controls.Add(dateBtn);
                if ((w + i) % 7 == 6) j++;
            }
        }
        public void DateButton_Click(object sender, EventArgs e, DateTime d)
        {
            activeDate = d;
            if (DateTime.Compare(d, DateTime.Today) < 0)
                calendarTabControl.SelectedTab = recordViewPage;
            else if (DateTime.Compare(d, DateTime.Today) == 0)
                calendarTabControl.SelectedTab = recordEditPage;
            else
                MessageBox.Show("還不能新增記錄喔~", "Future Date");
        }

        private void recordViewPage_Enter(object sender, EventArgs e)
        {
            activeRecord = new DailyRecord(activeDate);
            dateViewLabel.Text = activeDate.Month + " 月 " + activeDate.Day + " 日";
            weightViewLabel.Text = "體重： " + activeRecord.weight + " kg";
            waistlineViewLabel.Text = "腰圍： " + activeRecord.waistline + " cm";
            SBPViewLabel.Text = "收縮壓： " + activeRecord.SBP + " mmHg";
            DBPViewLabel.Text = "舒張壓： " + activeRecord.DBP + " mmHg";
            BMIViewLabel.Text = "BMI： " + activeRecord.BMI;
        }

        private void recordViewPage_Leave(object sender, EventArgs e)
        {
            if (activeRecord != null)
                activeRecord = null;
        }

        private void recordEditPage_Enter(object sender, EventArgs e)
        {
            activeRecord = new DailyRecord(activeDate);
            dateEditLabel.Text = activeDate.Month + " 月 " + activeDate.Day + " 日";

            if (activeRecord.modified)
            {
                weightEditText.Text = activeRecord.weight.ToString();
                waistlineEditText.Text = activeRecord.waistline.ToString();
                SBPEditText.Text = activeRecord.SBP.ToString();
                DBPEditText.Text = activeRecord.DBP.ToString();
            }
            else
            {
                weightEditText.Text = "0";
                waistlineEditText.Text = "0";
                SBPEditText.Text = "0";
                DBPEditText.Text = "0";
            }

            foreach (Diet d in activeRecord.diets)
            {
                DietPanel dp = new DietPanel(this, dietPanels.Count, recordEditDietLabel.Location.X, recordEditDietLabel.Location.Y + recordEditDietLabel.Height + 30);
                dp.setData(d.meal_id, d.calorie, d.diet);
                dietPanels.Add(dp);
                editPanel.Controls.Add(dp);
            }
            foreach (Exercise ex in Exercise.exerciseList)
                exerciseCheckBox.Items.Add(ex.title);
            foreach (Exercise ex in activeRecord.exercises)
                for (int i = 0; i < exerciseCheckBox.Items.Count; ++i)
                    if (exerciseCheckBox.Items[i].ToString() == ex.title)
                    {
                        exerciseCheckBox.SetItemChecked(i, true);
                        break;
                    }
        }

        private void recordEditPage_Leave(object sender, EventArgs e)
        {
            if (activeRecord != null)
                activeRecord = null;
            exerciseCheckBox.Items.Clear();
            for (int i = dietPanels.Count - 1; i >= 0; --i)
                removeDietPanel(i);
        }

        private void backViewButton_Click(object sender, EventArgs e)
        {
            calendarTabControl.SelectedTab = calendarPage;
        }

        private void editViewButton_Click(object sender, EventArgs e)
        {
            calendarTabControl.SelectedTab = recordEditPage;
        }

        private void updateRecordButton_Click(object sender, EventArgs e)
        {
            bool valid = true;
            valid = checkTextNotEmpty(weightEditText);
            valid = checkTextNotEmpty(waistlineEditText);
            valid = checkTextNotEmpty(SBPEditText);
            valid = checkTextNotEmpty(DBPEditText);

            for (int i = 0; i < dietPanels.Count; ++i)
                valid = dietPanels[i].checkValid();

            if (!valid) return;
            else
            {
                activeRecord.weight = Convert.ToInt32(weightEditText.Text);
                activeRecord.waistline = Convert.ToInt32(waistlineEditText.Text);
                activeRecord.SBP = Convert.ToInt32(SBPEditText.Text);
                activeRecord.DBP = Convert.ToInt32(DBPEditText.Text);
                activeRecord.diets.Clear();
                for (int i = 0; i < dietPanels.Count; ++i)
                        activeRecord.diets.Add(new Diet(dietPanels[i].meal, dietPanels[i].diet, dietPanels[i].calorie));

                activeRecord.exercises.Clear();
                foreach (int index in exerciseCheckBox.CheckedIndices)
                    activeRecord.exercises.Add(new Exercise(index + 1, "", "", 0, 0));
                activeRecord.update();
                calendarTabControl.SelectedTab = recordViewPage;
            }
        }
        private void cancelRecordButton_Click(object sender, EventArgs e)
        {
            calendarTabControl.SelectedTab = recordViewPage;
        }

        private void addDietRecordButton_Click(object sender, EventArgs e)
        {
            DietPanel dp = new DietPanel(this, dietPanels.Count, recordEditDietLabel.Location.X, recordEditDietLabel.Location.Y + recordEditDietLabel.Height + 30);
            dietPanels.Add(dp);
            editPanel.Controls.Add(dp);
        }

        public void removeDietPanel(int index) {
            DietPanel tmp = dietPanels[index];
            dietPanels.RemoveAt(index);
            tmp.Dispose();

            for (int i = 0; i < dietPanels.Count; ++i)
            {
                dietPanels[i].setPosition(i);
            }
        }

        private bool checkTextNotEmpty(TextBox t)
        {
            if (t.Text != "")
            {
                t.BackColor = Color.White;
                return true;
            }
            else
            {
                t.BackColor = Color.LightCoral;
                return false;
            }
        }
    }
}
