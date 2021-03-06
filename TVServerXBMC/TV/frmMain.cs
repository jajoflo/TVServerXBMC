using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using TvDatabase;

namespace MPTvClient
{

    public partial class frmMain : Form
    {

        ServerInterface serverIntf;
        ExternalPlayer extPlayer;

        // processed
        private void SetDisconected()
        {
            tmrRefresh.Enabled = false;
            extPlayer.Stop();
            serverIntf.ResetConnection();
            StBarLabel.Text = "Not connected.";
            StBar.Update();
            MessageBox.Show(serverIntf.lastException.ToString(), "Exception raised", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnConnect.Visible = true;
        }

        private void UpdateTVChannels()
        {
            if (cbGroups.SelectedIndex == -1)
                return;
            StBarLabel.Text = "Loading referenced channels...";
            StBar.Update();
            List<ChannelInfo> refChannelInfos = serverIntf.GetChannelInfosForGroup(cbGroups.SelectedItem.ToString());
            if (refChannelInfos == null)
                SetDisconected();
            gridTVChannels.Rows.Clear();
            foreach (ChannelInfo chanInfo in refChannelInfos)
                gridTVChannels.Rows.Add(chanInfo.channelID, chanInfo.name, chanInfo.epgNow.timeInfo + "\n" + chanInfo.epgNext.timeInfo, chanInfo.epgNow.description + "\n" + chanInfo.epgNext.description);
            gridTVChannels.AutoResizeColumns();
            StBarLabel.Text = "";
        }
        private void UpdateRadioChannels()
        {
            StBarLabel.Text = "Loading radio channels...";
            StBar.Update();
            List<ChannelInfo> channelInfos = serverIntf.GetRadioChannels();
            if (channelInfos == null)
                SetDisconected();
            gridRadioChannels.Rows.Clear();
            foreach (ChannelInfo chanInfo in channelInfos)
            {
                string type = "DVB";
                if (chanInfo.isWebStream)
                    type = "WebStream";
                gridRadioChannels.Rows.Add(chanInfo.channelID, chanInfo.name, type, chanInfo.epgNow.timeInfo + "\n" + chanInfo.epgNext.timeInfo, chanInfo.epgNow.description + "\n" + chanInfo.epgNext.description);
            }
            gridRadioChannels.AutoResizeColumns();
            StBarLabel.Text = "";
        }
        private void UpdateRecordings()
        {
            StBarLabel.Text = "Loading recordings...";
            StBar.Update();
            List<RecordingInfo> recInfos = serverIntf.GetRecordings();
            if (recInfos == null)
                SetDisconected();
            gridRecordings.Rows.Clear();
            foreach (RecordingInfo rec in recInfos)
                gridRecordings.Rows.Add(rec.recordingID, rec.timeInfo, rec.genre, rec.title, rec.description);
            gridRecordings.AutoResizeColumns();
            StBarLabel.Text = "";
        }
        private void UpdateSchedules()
        {
            StBarLabel.Text = "Loading schedules...";
            StBar.Update();
            List<ScheduleInfo> schedInfos = serverIntf.GetSchedules();
            if (schedInfos == null)
                SetDisconected();
            gridSchedules.Rows.Clear();
            foreach (ScheduleInfo sched in schedInfos)
                gridSchedules.Rows.Add(sched.scheduleID, sched.startTime.ToString(), sched.endTime.ToString(), sched.description, sched.channelName, sched.type);
            gridSchedules.AutoResizeColumns();
            StBarLabel.Text = "";
        }
        private void cbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTVChannels();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!extPlayer.IsRunning())
                serverIntf.StopTimeShifting();
            ReceptionDetails recDetails = serverIntf.GetReceptionDetails();
            if (recDetails == null)
            {
                SetDisconected();
                return;
            }
            prLevel.Value = recDetails.signalLevel;
            prQuality.Value = recDetails.signalQuality;
            List<StreamingStatus> statusList = serverIntf.GetStreamingStatus();
            if (statusList == null)
            {
                SetDisconected();
                return;
            }
            lvStatus.Items.Clear();
            foreach (StreamingStatus sstate in statusList)
            {
                ListViewItem item = lvStatus.Items.Add(sstate.cardId.ToString());
                item.SubItems.Add(sstate.cardName);
                item.SubItems.Add(sstate.cardType);
                item.SubItems.Add(sstate.status);
                item.SubItems.Add(sstate.channelName);
                item.SubItems.Add(sstate.userName);
            }
            lvStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void gridChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            StBarLabel.Text = "Trying to start timeshifting...";
            StBar.Update();
            serverIntf.StopTimeShifting();
            extPlayer.Stop();
            string rtspURL = "";
            TvResult result = serverIntf.StartTimeShifting(int.Parse(gridTVChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
            StBarLabel.Text = "";
            StBar.Update();
            if (result != TvResult.Succeeded)
                MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
            else
            {
                string args = string.Format(ClientSettings.playerArgs, rtspURL);
                if (!extPlayer.Start(ClientSettings.playerPath, args))
                    MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (extPlayer.IsRunning())
                extPlayer.Stop();
            serverIntf.StopTimeShifting();
            serverIntf.ResetConnection();
            ClientSettings.frmLeft = this.Left;
            ClientSettings.frmTop = this.Top;
            ClientSettings.frmWidth = this.Width;
            ClientSettings.frmHeight = this.Height;
            ClientSettings.Save();
        }


        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (tabCtrl.SelectedIndex)
            {
                case 0:
                    UpdateTVChannels();
                    break;
                case 1:
                    UpdateRadioChannels();
                    break;
                case 2:
                    UpdateRecordings();
                    break;
                case 3:
                    UpdateSchedules();
                    break;
            }
        }

        private void gridTVChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            StBarLabel.Text = "Trying to start timeshifting...";
            StBar.Update();
            serverIntf.StopTimeShifting();
            extPlayer.Stop();
            string rtspURL = "";
            TvResult result = serverIntf.StartTimeShifting(int.Parse(gridTVChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
            StBarLabel.Text = "";
            StBar.Update();
            if (result != TvResult.Succeeded)
                MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
            else
            {
                if (ClientSettings.useOverride)
                    rtspURL = ClientSettings.overrideURL;
                string args = string.Format(ClientSettings.playerArgs, rtspURL);
                if (!extPlayer.Start(ClientSettings.playerPath, args))
                    MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridRadioChannels_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            serverIntf.StopTimeShifting();
            extPlayer.Stop();
            string rtspURL = "";
            if (gridRadioChannels.SelectedRows[0].Cells[2].Value.ToString() == "DVB")
            {
                StBarLabel.Text = "Trying to start timeshifting...";
                StBar.Update();
                TvResult result = serverIntf.StartTimeShifting(int.Parse(gridRadioChannels.SelectedRows[0].Cells[0].Value.ToString()), ref rtspURL);
                StBarLabel.Text = "";
                StBar.Update();
                if (result != TvResult.Succeeded)
                    MessageBox.Show("Could not start timeshifting\nReason: " + result.ToString());
            }
            else
                rtspURL = serverIntf.GetWebStreamURL(int.Parse(gridRadioChannels.SelectedRows[0].Cells[0].Value.ToString()));
            if (ClientSettings.useOverride)
                rtspURL = ClientSettings.overrideURL;
            string args = string.Format(ClientSettings.playerArgs, rtspURL);
            if (!extPlayer.Start(ClientSettings.playerPath, args))
                MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void gridRecordings_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            StBarLabel.Text = "Trying to replay recording...";
            StBar.Update();
            serverIntf.StopTimeShifting();
            extPlayer.Stop();
            string rtspURL = serverIntf.GetRecordingURL(int.Parse(gridRecordings.SelectedRows[0].Cells[0].Value.ToString()));
            StBarLabel.Text = "";
            StBar.Update();
            if (rtspURL == "")
                MessageBox.Show("Could not start recording");
            else
            {
                string args = string.Format(ClientSettings.playerArgs, rtspURL);
                if (!extPlayer.Start(ClientSettings.playerPath, args))
                    MessageBox.Show("Failed to start external player.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            miTimeShift.Visible = false;
            miReplay.Visible = false;
            miDelete.Visible = false;
            switch (tabCtrl.SelectedIndex)
            {
                case 0:
                    miTimeShift.Visible = true;
                    break;
                case 1:
                    miTimeShift.Visible = true;
                    break;
                case 2:
                    miReplay.Visible = true;
                    miDelete.Visible = true;
                    break;
                case 3:
                    miDelete.Visible = true;
                    break;
            }
        }

        private void miRefresh_Click(object sender, EventArgs e)
        {
            refreshToolStripMenuItem_Click(sender, e);
        }

        private void miTimeShift_Click(object sender, EventArgs e)
        {
            if (tabCtrl.SelectedIndex == 0)
                gridTVChannels_CellDoubleClick(sender, null);
            if (tabCtrl.SelectedIndex == 1)
                gridRadioChannels_CellDoubleClick(sender, null);
        }

        private void miReplay_Click(object sender, EventArgs e)
        {
            gridRecordings_CellDoubleClick(sender, null);
        }

        private void miDelete_Click(object sender, EventArgs e)
        {
            if (tabCtrl.SelectedIndex == 2)
            {
                string idRecording = gridRecordings.SelectedRows[0].Cells[0].Value.ToString();
                if (idRecording == "")
                    return;
                serverIntf.DeleteRecording(int.Parse(idRecording));
                UpdateRecordings();
            }
            if (tabCtrl.SelectedIndex == 3)
            {
                string idSchedule = gridSchedules.SelectedRows[0].Cells[0].Value.ToString();
                if (idSchedule == "")
                    return;
                serverIntf.DeleteSchedule(int.Parse(idSchedule));
                UpdateSchedules();
            }
        }

        private void btnShowEPG_Click(object sender, EventArgs e)
        {
            List<ChannelInfo> infos = new List<ChannelInfo>();
            foreach (DataGridViewRow row in gridTVChannels.Rows)
            {
                ChannelInfo info = new ChannelInfo();
                info.channelID = row.Cells[0].Value.ToString();
                info.name = row.Cells[1].Value.ToString();
                infos.Add(info);
            }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (ClientSettings.frmLeft != 0 && ClientSettings.frmTop != 0)
            {
                this.Left = ClientSettings.frmLeft;
                this.Top = ClientSettings.frmTop;
                this.Width = ClientSettings.frmWidth;
                this.Height = ClientSettings.frmHeight;
            }
        }
        private bool Test()
        {
            bool yes = false;
            try
            {
                yes = true;
                return yes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception");
            }
            finally
            {
                MessageBox.Show("finally");
            }
            return false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Returned: " + Test().ToString());
        }
    }
}