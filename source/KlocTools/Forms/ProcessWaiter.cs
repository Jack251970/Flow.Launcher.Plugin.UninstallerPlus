/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Windows.Forms;

namespace Klocman.Forms
{
    public partial class ProcessWaiter : Form
    {
        private ProcessWaiter()
        {
            InitializeComponent();

            DialogResult = DialogResult.Cancel;

            processWaiterControl1.AllProcessesClosed += ProcessWaiterControl1_AllProcessesClosed;
            processWaiterControl1.CancelClicked += ProcessWaiterControl1_CancelClicked;
        }

        #region For WPF_TEST

        private readonly Action<ProcessWaiter> _closedAction;
        private bool _initialized;

        public ProcessWaiter(System.Drawing.Icon icon, Action<ProcessWaiter> closeAction)
        {
            _closedAction = closeAction;
            FormClosed += JunkRemoveWindow_FormClosed;

            InitializeComponent();

            Icon = icon;
            DialogResult = DialogResult.Cancel;

            processWaiterControl1.AllProcessesClosed += ProcessWaiterControl1_AllProcessesClosed;
            processWaiterControl1.CancelClicked += ProcessWaiterControl1_CancelClicked;
        }

        private void JunkRemoveWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_initialized)
            {
                _closedAction.Invoke(this);
            }
        }

        public void Initialize(int[] processIDs, bool processChildren)
        {
            processWaiterControl1.Initialize(processIDs, processChildren);
            _initialized = true;
        }

        #endregion

        private void ProcessWaiterControl1_CancelClicked(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ProcessWaiterControl1_AllProcessesClosed(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        
        /// <summary>
        ///     Returns flase if user cancels the operation.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="processIDs">IDs of processes to check</param>
        /// <param name="processChildren">Check child processes as well</param>
        /// <returns></returns>
        public static bool ShowDialog(Form owner, int[] processIDs, bool processChildren)
        {
            using (var pw = new ProcessWaiter())
            {
                pw.Icon = owner.Icon;
                pw.processWaiterControl1.Initialize(processIDs,processChildren);
                return pw.ShowDialog(owner) == DialogResult.OK;
            }
        }

        private void ProcessWaiter_Shown(object sender, EventArgs e)
        {
            processWaiterControl1.StartUpdating();
        }
    }
}