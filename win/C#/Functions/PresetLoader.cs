﻿/*  PresetLoader.cs $
 	
 	   This file is part of the HandBrake source code.
 	   Homepage: <http://handbrake.fr>.
 	   It may be used under the terms of the GNU General Public License. */

using System;
using System.Windows.Forms;

namespace Handbrake.Functions
{
    class PresetLoader
    {
        /// <summary>
        /// This function takes in a Query which has been parsed by QueryParser and
        /// set's all the GUI widgets correctly.
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="presetQuery">The Parsed CLI Query</param>
        /// <param name="name">Name of the preset</param>
        /// <param name="pictureSettings">Save picture settings in the preset</param>
        public static void presetLoader(frmMain mainWindow, QueryParser presetQuery, string name, Boolean pictureSettings)
        {
            // ---------------------------
            // Setup the GUI
            // ---------------------------

            #region Source
            // Reset some vaules to stock first to prevent errors.
            mainWindow.check_iPodAtom.CheckState = CheckState.Unchecked;

            // Now load all the new settings onto the main window
            if (presetQuery.Format != null)
            {
                string destination = mainWindow.text_destination.Text;
                destination = destination.Replace(".mp4", "." + presetQuery.Format);
                destination = destination.Replace(".m4v", "." + presetQuery.Format);
                destination = destination.Replace(".mkv", "." + presetQuery.Format);
                mainWindow.text_destination.Text = destination;
            }

            #endregion

            #region Destination and Output Settings

            if (presetQuery.Format != null)
            {
                if (presetQuery.Format == "mp4")
                    mainWindow.drop_format.SelectedIndex = 0;
                else if (presetQuery.Format == "m4v")
                    mainWindow.drop_format.SelectedIndex = 1;
                else if (presetQuery.Format == "mkv")
                    mainWindow.drop_format.SelectedIndex = 2;

                if (presetQuery.ChapterMarkers && presetQuery.Format == "mp4")
                    mainWindow.drop_format.SelectedIndex = 1;
            }

            mainWindow.check_iPodAtom.CheckState = presetQuery.IpodAtom ? CheckState.Checked : CheckState.Unchecked;

            mainWindow.check_optimiseMP4.CheckState = presetQuery.OptimizeMP4 ? CheckState.Checked : CheckState.Unchecked;

            mainWindow.check_largeFile.CheckState = presetQuery.LargeMP4 ? CheckState.Checked : CheckState.Unchecked;

            mainWindow.setContainerOpts(); // select the container options according to the selected format

            #endregion

            #region Picture
            mainWindow.check_autoCrop.Checked = true;
            if (presetQuery.CropBottom == "0" && presetQuery.CropTop == "0")
                if (presetQuery.CropLeft == "0" && presetQuery.CropRight == "0")
                    mainWindow.check_customCrop.Checked = true;

            mainWindow.text_width.Text = "";
            mainWindow.text_height.Text = "";

            if (pictureSettings)
            {
                if (presetQuery.CropTop != null)
                {
                    int top, bottom, left, right;
                    int.TryParse(presetQuery.CropTop, out top);
                    int.TryParse(presetQuery.CropBottom, out bottom);
                    int.TryParse(presetQuery.CropLeft, out left);
                    int.TryParse(presetQuery.CropRight, out right);

                    mainWindow.check_customCrop.Checked = true;
                    mainWindow.text_top.Value = top;
                    mainWindow.text_bottom.Value = bottom;
                    mainWindow.text_left.Value = left;
                    mainWindow.text_right.Value = right;
                }

                if (presetQuery.Width != 0)
                    mainWindow.text_width.Text = presetQuery.Width.ToString();

                if (presetQuery.Height != 0)
                    mainWindow.text_height.Text = presetQuery.Height.ToString();
            }

            mainWindow.drp_anamorphic.SelectedIndex = presetQuery.Anamorphic ? 1 : 0;

            if (presetQuery.LooseAnamorphic)
                mainWindow.drp_anamorphic.SelectedIndex = 2;
            else
            {
                if (presetQuery.Anamorphic != true)
                    mainWindow.drp_anamorphic.SelectedIndex = 0;
            }

            // Set the public max width and max height varibles in frmMain
            // These are used by the query generator to determine if it should use -X or -w  / -Y or -h
            if (presetQuery.MaxWidth != 0)
            {
                mainWindow.text_width.Text = presetQuery.MaxWidth.ToString();
                mainWindow.maxWidth = presetQuery.MaxWidth;
            }

            if (presetQuery.MaxHeight != 0)
            {
                mainWindow.text_height.Text = presetQuery.MaxHeight.ToString();
                mainWindow.maxHeight = presetQuery.MaxHeight;
            }


            #endregion

            #region Filters

            mainWindow.ctl_decomb.setOption(presetQuery.Decomb);

            if (mainWindow.ctl_decomb.getDropValue == "Off")
                mainWindow.ctl_deinterlace.setOption(presetQuery.DeInterlace);
            else
                mainWindow.ctl_deinterlace.setOption("None"); // Don't want decomb and deinterlace on at the same time

            mainWindow.ctl_denoise.setOption(presetQuery.DeNoise);
            mainWindow.ctl_detelecine.setOption(presetQuery.DeTelecine);

            if (presetQuery.DeBlock != 0)
            {
                mainWindow.slider_deblock.Value = presetQuery.DeBlock;
                mainWindow.lbl_deblockVal.Text = presetQuery.DeBlock.ToString();
            }
            else
            {
                mainWindow.slider_deblock.Value = 4;
                mainWindow.lbl_deblockVal.Text = "Off";
            }
            #endregion

            #region Video
            mainWindow.drp_videoEncoder.Text = presetQuery.VideoEncoder;

            if (presetQuery.AverageVideoBitrate != null)
            {
                mainWindow.radio_avgBitrate.Checked = true;
                mainWindow.text_bitrate.Text = presetQuery.AverageVideoBitrate;
            }
            if (presetQuery.VideoTargetSize != null)
            {
                mainWindow.radio_targetFilesize.Checked = true;
                mainWindow.text_filesize.Text = presetQuery.VideoTargetSize;
            }

            // Quality
            if (presetQuery.VideoQuality != 0)
            {
                mainWindow.radio_cq.Checked = true;
                if (presetQuery.VideoEncoder == "H.264 (x264)")
                {
                    int value;
                    System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

                    double x264step;
                    double presetValue = presetQuery.VideoQuality;
                    double.TryParse(Properties.Settings.Default.x264cqstep,
                                    System.Globalization.NumberStyles.Number,
                                    culture,
                                    out x264step);

                    double x = 51 / x264step;

                    double calculated = presetValue / x264step;
                    calculated = x - calculated;

                    int.TryParse(calculated.ToString(), out value);

                    // This will sometimes occur when the preset was generated 
                    // with a different granularity, so, round and try again.
                    if (value == 0)
                    {
                        double val = Math.Round(calculated, 0);
                        int.TryParse(val.ToString(), out value);
                    }

                    mainWindow.slider_videoQuality.Value = value;
                }
                else
                {
                    int presetVal;
                    int.TryParse(presetQuery.VideoQuality.ToString(), out presetVal);
                    mainWindow.slider_videoQuality.Value = presetVal;
                }
            }

            mainWindow.check_2PassEncode.CheckState = presetQuery.TwoPass ? CheckState.Checked : CheckState.Unchecked;

            mainWindow.check_grayscale.CheckState = presetQuery.Grayscale ? CheckState.Checked : CheckState.Unchecked;

            mainWindow.drp_videoFramerate.Text = presetQuery.VideoFramerate;

            mainWindow.check_turbo.CheckState = presetQuery.TurboFirstPass ? CheckState.Checked : CheckState.Unchecked;

            #endregion

            #region Chapter Markers

            if (presetQuery.ChapterMarkers)
            {
                mainWindow.Check_ChapterMarkers.CheckState = CheckState.Checked;
                mainWindow.Check_ChapterMarkers.Enabled = true;
            }
            else
                mainWindow.Check_ChapterMarkers.CheckState = CheckState.Unchecked;

            #endregion

            #region Audio
            // Clear the audio listing
            mainWindow.audioPanel.clearAudioList();

            if (presetQuery.AudioInformation != null)
                foreach (AudioTrack track in presetQuery.AudioInformation)
                {
                    ListViewItem newTrack = new ListViewItem(mainWindow.audioPanel.getNewID().ToString());

                    newTrack.SubItems.Add("Automatic");
                    newTrack.SubItems.Add(track.Encoder);
                    newTrack.SubItems.Add(track.MixDown);
                    newTrack.SubItems.Add(track.SampleRate);
                    newTrack.SubItems.Add(track.Bitrate);
                    newTrack.SubItems.Add(track.DRC);
                    mainWindow.audioPanel.addTrackForPreset(newTrack);
                }

            // Subtitle Stuff
            mainWindow.drp_subtitle.Text = presetQuery.Subtitles;

            if (presetQuery.ForcedSubtitles)
            {
                mainWindow.check_forced.CheckState = CheckState.Checked;
                mainWindow.check_forced.Enabled = true;
            }
            else
                mainWindow.check_forced.CheckState = CheckState.Unchecked;

            #endregion

            #region Other
            mainWindow.x264Panel.x264Query = presetQuery.H264Query;

            // Set the preset name
            mainWindow.groupBox_output.Text = "Output Settings (Preset: " + name + ")";
            #endregion
        }
    }
}