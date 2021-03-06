﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Configuration;


namespace iMacrosPostingDashboard
{
    public class BckWorkerTemplate
    {
        int totalErr = 0;
        private string locProjName = "";
        private string locTopicTable = "";
        private string locAnswTmpl = "";
        private ProjectTableRow localTableRow;

        private int locpausebfconfirm;
        private int locpausebfnextpost;
        private bool locpostQnA;

        private List<string> ResultBoxVirtual = new List<string>();
        // private int ProgressBarVirtual = 0;

        System.ComponentModel.BackgroundWorker bw;

        public BckWorkerTemplate()
        {
        
        }
        public BckWorkerTemplate(string ProjName, string TopicTable, string AnswTmpl, int pausebfconfirm, int pausebfnextpost, bool postQnA, ref ProjectTableRow CntRow)
        {
            bw = new System.ComponentModel.BackgroundWorker();
            locProjName = ProjName;
            locTopicTable = TopicTable;
            locAnswTmpl = AnswTmpl;
            locpausebfconfirm = pausebfconfirm;
            locpausebfnextpost = pausebfnextpost;
            locpostQnA = postQnA;

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWorkDataGrid);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompdDataGrid);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChdDataGrid);

            localTableRow = CntRow;

            // e.g.  "MumsNetUK", "topicsmumsnet", "AmazonUK-one-link"
        }

        private void EmailProjStatus()
        {
            // READ FROM CONFIG FILE 
            string fromSender = (string)ConfigurationManager.AppSettings["Sender"];
            string toReceiver = (string)ConfigurationManager.AppSettings["ReceiverAdmin"];
            string SenderPass = (string)ConfigurationManager.AppSettings["SenderPass"];

            // STANDARD SMTP SENDER CODE 
            var fromAddress = new MailAddress(fromSender, "Forumu Statusas");
            var toAddress = new MailAddress(toReceiver, "");
            string fromPassword = SenderPass;
            string subject5orless = "Pranešimas: forumas " + locProjName + " sustojo";
            string subjectNone = "Pranešimas: forumas " + locProjName + " sustojo";
            string subject = "Pranešimas: forumas " + locProjName + " sustojo";

            string body = @" \n";

            // if (Anykwdsleft) subject = subject5orless;
            // else subject = subjectNone;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
        public void bwStartDataGrid(Object sender, EventArgs e)
        {
            // btnStart.Enabled = false;
            totalErr = 0;

            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
                
                // ResultBoxVirtual = new VTextBox();
                // ProgressBarVirtual = new VProgressBar();

                localTableRow.ProgressReport = "Running..." + Environment.NewLine;
            }
        }
        private void bw_DoWorkDataGrid(Object sender, DoWorkEventArgs e)
        {
            
            GenericPoster poster2 = new GenericPoster(ref sender, ref e, locProjName, locTopicTable, locAnswTmpl);
            poster2.RunningStatus = true;
            
            if (!locpostQnA)
                poster2.RunSimplePoster(locpausebfconfirm, locpausebfnextpost);

            /*
            int i = 0;
            while (!bw.CancellationPending)
            {
                System.Threading.Thread.Sleep((1000 * 1));
                bw.ReportProgress(i, i.ToString());
                i++;
                if (i > 50) i = 0;
            }
            e.Cancel = true;
            return;
        */
        }
        private void bw_RunWorkerCompdDataGrid(Object sender, RunWorkerCompletedEventArgs e)
        {
            EmailProjStatus();
            if (e.Error != null)
            {
                localTableRow.ProgressReport += "Errors occured. Please see log above." + Environment.NewLine;
                totalErr++;
            }
            else
            {
                if (e.Cancelled)
                {
                    localTableRow.ProgressReport += "Cancelled." + Environment.NewLine;
                    totalErr++;
                }
                else
                {
                    // WorkResult result = e.Result as WorkResult;
                    // if (result.ToString() != null)
                    
                    localTableRow.ProgressReport += "Process finished." + Environment.NewLine;
                    totalErr++;
                }
            }

            // btnStart.Enabled = true;
        }
        private void bw_ProgressChdDataGrid(Object sender, ProgressChangedEventArgs e)
        {
            localTableRow.ProgressBar = e.ProgressPercentage;
            string reportmsg = e.UserState as String;
            localTableRow.ProgressReport = reportmsg + Environment.NewLine;
        }
        public void bwStopDataGrid(Object sender, EventArgs e)
        {
            if (bw.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                bw.CancelAsync();
                // btnStart.Enabled = true;
            }
        }

    }

}
