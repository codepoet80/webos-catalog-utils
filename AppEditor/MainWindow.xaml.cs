﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace webOS.AppCatalog.AppEditor
{
    
    public partial class MainWindow : Window
    {
        public static string appcatalogDir = @"C:\Users\Jonathan Wise\Projects\_webOSAppCatalog";
        public string currentFile = "";
        public string welcomeText = "Open file first...";
        public bool unsaved = false;
        public MainWindow()
        {
            InitializeComponent();
            txtMetaJson.Text = welcomeText;
        }

        #region UI Handlers
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (radioTouchPad.IsChecked == true)
            {
                checkTouchPad.IsChecked = true;
                checkPre3.IsChecked = false;
                checkPre2.IsChecked = false;
                checkPre.IsChecked = false;
                checkPixi.IsChecked = false;
                checkVeer.IsChecked = false;
            }
        }

        private void checkPhone_Checked(object sender, RoutedEventArgs e)
        {
            radioTouchPad.IsChecked = false;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    txtFilename.Text = openFileDialog.FileName;
                    if (txtMetaJson.Text == "")
                    {
                        btnOpen_Click(sender, e);
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(txtFilename.Text))
            {
                txtMetaJson.Text = File.ReadAllText(txtFilename.Text);
                List<AppDefinition> catalogApps = ReadCatalogFile(Path.Combine(appcatalogDir, "masterAppData.json"));
                foreach (AppDefinition findApp in catalogApps)
                {
                    if (findApp.id.ToString() == System.IO.Path.GetFileNameWithoutExtension(txtFilename.Text))
                    {
                        currentFile = txtFilename.Text;
                        btnOpen.Content = "Revert";
                        btnSave.Content = "Save";
                        LoadAppMetaData(findApp);
                    }
                }
                unsaved = true;
            }
        }

        private void LoadAppMetaData(AppDefinition thisApp)
        {
            txtAppId.Text = thisApp.id.ToString();
            txtTitle.Text = thisApp.title;
            txtAuthor.Text = thisApp.author;
            foreach (ComboBoxItem comboChoice in comboCategory.Items)
            {
                if (comboChoice.Content.ToString().ToLower() == thisApp.category.ToLower())
                    comboCategory.SelectedItem = comboChoice;
            }
            txtSummary.Text = thisApp.summary;
            txtAppIcon.Text = thisApp.appIcon;
            txtAppIconBig.Text = thisApp.appIconBig;
            checkPixi.IsChecked = thisApp.Pixi;
            checkVeer.IsChecked = thisApp.Veer;
            checkPre.IsChecked = thisApp.Pre;
            checkPre2.IsChecked = thisApp.Pre2;
            checkPre3.IsChecked = thisApp.Pre3;
            checkTouchPad.IsChecked = thisApp.TouchPad;
            radioTouchPad.IsChecked = thisApp.touchpad_exclusive;
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            validateMetaJson();
        }

        private bool validateMetaJson()
        {
            try
            {
                JObject.Parse(txtMetaJson.Text);
                MessageBox.Show("JSON checks out OK!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error parsing JSON:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        private bool validateMasterFields()
        {
            if (!int.TryParse(txtAppId.Text, out int checkInt))
                return false;
            if (txtAppId.Text == "" ||
                txtTitle.Text == "" ||
                txtAuthor.Text == "" ||
                comboCategory.SelectedIndex < 0 ||
                txtSummary.Text == "" ||
                txtAppIcon.Text == "")
            {
                return false;
            }
            else
                return true;
        }

        private void txtFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMetaJson.Text == welcomeText)
            {
                txtMetaJson.Text = "";
                btnOpen.IsEnabled = true;
            }
            if (txtFilename.Text != currentFile)
                btnOpen.Content = "Open";
            else
                btnOpen.Content = "Revert";
            if (txtFilename.Text != "")
                btnSave.Content = "Save";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btnSave.Content == "Save")
                doSave();
            else
                doPatch();
        }

        private void doSave()
        {
            if (validateMetaJson() && validateMasterFields())
            {
                if (TrySaveAppInfo())
                {
                    MessageBox.Show("App info saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    unsaved = false;
                }
                else
                {
                    MessageBox.Show("Something went wrong :-(");
                }
            }
            else
            {
                if (!validateMasterFields())
                    MessageBox.Show("Cannot save. One or more required catalog fields is empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Cannot save. Metadata JSON is invalid.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void doPatch()
        {
            if (!unsaved || (unsaved && MessageBox.Show("Are you sure you want to patch without saving?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                MessageBox.Show("I could run a batch file now, if you want...");
        }

        private void txtMetaJson_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtMetaJson.Text == welcomeText)
            {
                txtMetaJson.Text = "";
            }
        }


        private void btnSave_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0 || (Keyboard.Modifiers & ModifierKeys.Shift) > 0)
            {
                btnSave.Content = "Patch";
            }
        }

        private void btnSave_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (txtMetaJson.Text != "" && txtMetaJson.Text != welcomeText)
                btnSave.Content = "Save";
        }

        #endregion

        #region CatalogUtils
        private List<AppDefinition> ReadCatalogFile(string catalogFileName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppDefinition>>(File.ReadAllText(catalogFileName));
        }

        private bool TrySaveAppInfo()
        {
            //Make sure we have a valid App ID
            if (!int.TryParse(txtAppId.Text, out int appIdInt))
            {
                MessageBox.Show("The App ID does not parse to an integer and cannot be used.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAppId.Focus();
                return false;
            }
            else
            {
                string saveFileId = Path.GetFileNameWithoutExtension(txtFilename.Text);
                if (txtAppId.Text != saveFileId)
                {
                    MessageBoxResult confirm = MessageBox.Show("Filename App ID and Catalog App ID do not match. Are you sure you wish to save?", "Caution", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.No)
                        return false;
                }
            }

            //Make fields into valid AppDefinition Object
            AppDefinition appUpdate = new AppDefinition();
            try
            {
                appUpdate.id = appIdInt;
                appUpdate.title = txtTitle.Text;
                appUpdate.summary = txtSummary.Text;
                appUpdate.author = txtAuthor.Text;
                appUpdate.category = comboCategory.Text;
                appUpdate.appIcon = txtAppIcon.Text;
                appUpdate.appIconBig = txtAppIconBig.Text;
                appUpdate.touchpad_exclusive = (bool)radioTouchPad.IsChecked;
                appUpdate.TouchPad = (bool)checkTouchPad.IsChecked;
                appUpdate.Pre3 = (bool)checkPre3.IsChecked;
                appUpdate.Pre2 = (bool)checkPre2.IsChecked;
                appUpdate.Pre = (bool)checkPre.IsChecked;
                appUpdate.Veer = (bool)checkVeer.IsChecked;
                appUpdate.Pixi = (bool)checkPixi.IsChecked;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error mapping catalog fields: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //Try to write catalog files
            bool catalogWriteFailure = true;
            List<AppDefinition> masterAppCatalog = ReadCatalogFile(Path.Combine(appcatalogDir, "masterAppData.json"));
            if (TryReplaceAppInCatalog(appUpdate, masterAppCatalog, out masterAppCatalog))
            {
                if (!WriteCatalogFile("master", masterAppCatalog))
                    return false;
                else
                {
                    List<AppDefinition> extantAppCatalog = ReadCatalogFile(Path.Combine(appcatalogDir, "extantAppData.json"));
                    if (!TryReplaceAppInCatalog(appUpdate, extantAppCatalog, out extantAppCatalog))
                    {
                        List<AppDefinition> missingAppCatalog = ReadCatalogFile(Path.Combine(appcatalogDir, "missingAppData.json"));
                        if (!TryReplaceAppInCatalog(appUpdate, missingAppCatalog, out missingAppCatalog))
                        {
                            MessageBox.Show("Although the app was updated in the masterAppCatalog, it could not be updated in the extant/missing catalogs", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                        else
                        {
                            if (WriteCatalogFile("missing", missingAppCatalog))
                                catalogWriteFailure = false;
                        }
                    }
                    else
                    {
                        if (WriteCatalogFile("extant", extantAppCatalog))
                            catalogWriteFailure = false;
                    }
                    catalogWriteFailure = false;
                }
            }
            else
            {
                MessageBox.Show("The app could not be updated in the masterAppCatalog, so it was not updated in the extant/missing catalogs either.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (catalogWriteFailure)
                return false;
            else
            {
                //Finally actually update the metadata JSON
                try
                {
                    File.WriteAllText(txtFilename.Text, txtMetaJson.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured saving metadata file: " + txtFilename.Text + "-test" + "\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return true;
        }

        private bool TryReplaceAppInCatalog(AppDefinition newApp, List<AppDefinition>catalogToUpdate, out List<AppDefinition>updatedCatalog)
        {
            AppDefinition appToReplace = new AppDefinition();
            bool found = false;
            foreach (AppDefinition checkApp in catalogToUpdate)
            {
                if (checkApp.id == newApp.id)
                {
                    appToReplace = checkApp;
                    found = true;
                }
            }
            if (found)
            {
                catalogToUpdate[catalogToUpdate.IndexOf(appToReplace)] = newApp;
                updatedCatalog = catalogToUpdate;
                return true;
            }
            updatedCatalog = catalogToUpdate;
            return false;
        }

        private bool WriteCatalogFile(string catalogName, List<AppDefinition> newAppCatalog)
        {
            try
            {
                Console.WriteLine(catalogName + " app catalog count now: " + newAppCatalog.Count);
                string newAppCatJson = Newtonsoft.Json.JsonConvert.SerializeObject(newAppCatalog);
                StreamWriter objWriter;
                objWriter = new StreamWriter(System.IO.Path.Combine(appcatalogDir, catalogName + "AppData.json"));
                objWriter.WriteLine(newAppCatJson);
                objWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured saving catalog file: " + catalogName + "\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        #endregion

    }
}