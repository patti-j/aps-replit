using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PT.Common;

public class MainForm : Form
{
    private ListBox filesList;
    private Button resetButton;
    private TextBox outputBox;

    public MainForm(string[] initialArgs)
    {
        Text = "PTAssemblyVersionReader";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

    filesList = new ListBox() { Dock = DockStyle.Top, Height = 200 };
        filesList.AllowDrop = true;
        filesList.HorizontalScrollbar = true;
        filesList.DragEnter += FilesList_DragEnter;
        filesList.DragDrop += FilesList_DragDrop;

    resetButton = new Button() { Text = "Reset", Dock = DockStyle.Top, Height = 30 };
    resetButton.Click += ResetButton_Click;

        outputBox = new TextBox() { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9) };
    Controls.Add(outputBox);
    Controls.Add(resetButton);
    Controls.Add(filesList);

        // If initial args provided (files dropped onto exe), add them
        if (initialArgs != null && initialArgs.Length > 0)
        {
            foreach (var a in initialArgs)
            {
                AddPath(a);
            }
        }
    }

    private void FilesList_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void FilesList_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var f in files)
            {
                AddPath(f);
            }
        }
    }

    private void AddPath(string path)
    {
        try
        {
            var full = Path.GetFullPath(path.Trim('"'));
            if (!filesList.Items.Contains(full))
            {
                filesList.Items.Add(full);
                // Auto-process the file as soon as it's added
                try
                {
                    ProcessFile(full);
                }
                catch (Exception ex)
                {
                    AppendLine($"Auto-process failed for {full}: {ex.Message}");
                }
            }
        }
        catch
        {
            // ignore invalid paths
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        filesList.Items.Clear();
        outputBox.Clear();
    }

    private void ProcessFile(string f)
    {
        if (!File.Exists(f))
        {
            AppendLine($"File not found: {f}");
            return;
        }

        if (!string.Equals(Path.GetExtension(f), ".dat", StringComparison.OrdinalIgnoreCase))
        {
            AppendLine($"Rejected (not a .dat file): {f}");
            return;
        }

        try
        {
            AppendLine($"Reading: {f}");
            using var reader = new BinaryFileReader(f);
            AppendLine($" Serialization Version: {reader.VersionNumber}");
            int readerAssemblyVersionMajor = reader.AssemblyVersionMajor;
            if (readerAssemblyVersionMajor <= 0)
            {
                AppendLine(" Software Version: [Prior to 12.3.0.76]");
            }
            else
            {
                
                AppendLine($" Software Version: {reader.AssemblyVersion}");
            }

            reader.Close();
        }
        catch (Exception ex)
        {
            // If the reader threw because assembly version components were out of int range,
            // the message contains the raw four parts. Display them for human readability
            // without changing the BinaryFileReader implementation.
            if (ex is InvalidDataException && ex.Message.StartsWith("Assembly version component out of range:"))
            {
                var parts = ex.Message.Substring("Assembly version component out of range:".Length).Trim();
                AppendLine($"  AssemblyVersion: {parts} (out of int range)");
            }

            AppendLine($"Error reading {f}: {ex.Message}");
        }
    }


    private void AppendLine(string s)
    {
        outputBox.AppendText(s + Environment.NewLine);
    }}
