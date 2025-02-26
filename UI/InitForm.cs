﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI;
using UI.User;

namespace ServerProj
{
    public partial class InitForm : Form
    {
        public InitForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnPlay_MouseEnter(object sender, EventArgs e)
        {
            ChangeForeColor(btnPlay, Color.Black);
        }

        private void btnPlay_MouseLeave(object sender, EventArgs e)
        {
            ChangeForeColor(btnPlay, Color.White);
        }

        private void btnServer_MouseEnter(object sender, EventArgs e)
        {
            ChangeForeColor(btnServer, Color.Black);
        }

        private void btnServer_MouseLeave(object sender, EventArgs e)
        {
            ChangeForeColor(btnServer, Color.White);
        }


        #region Little functions

        private void ChangeForeColor(Button btn, Color color) => btn.ForeColor = color;

        #endregion

        private void btnServer_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                Application.Run(new ServerForm());
            })).Start();

            //var server = new ServerForm();
            this.Hide();
            //server.ShowDialog();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!User.Instance.ConnectToTheServer())
                return;

            User.Instance.StartNewThread();

            var login = new LogInForm();
            this.Hide();
            login.ShowDialog();

        }

        private void InitForm_Load(object sender, EventArgs e)
        {

        }
    }
}
