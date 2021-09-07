using System.Globalization;
using System.Text;
using System.Threading;
using  LitJson;
using System.Windows.Forms;
using System.IO;

namespace DspFindSeed
{
    public partial class MainWindow
    {
        private SearchCondition FetchCondition ()
        {
            var condition = new SearchCondition ();
            condition.planetCount1     = int.Parse (planetCount1.Text);
            condition.planetCount2     = int.Parse (planetCount2.Text);
            condition.planetCount3     = int.Parse (planetCount3.Text);
            condition.isBluePlanet     = IsInBluePlanet.IsChecked ?? false;
            condition.GasCount         = int.Parse (GasCount.Text);
            condition.IcePlanetCount   = int.Parse (IcePlanetCount.Text);
            condition.dysonLumino      = float.Parse (dysonLumino.Text);
            condition.distanceToBirth  = float.Parse (distanceToBirth.Text);
            condition.isInDsp          = IsInDsp.IsChecked ?? false;
            condition.isInDsp2         = IsInDsp2.IsChecked ?? false;
            condition.resourceCount[0] = int.Parse (resource0.Text);
            condition.resourceCount[1] = int.Parse (resource1.Text);
            condition.resourceCount[2] = int.Parse (resource2.Text);
            condition.resourceCount[3] = int.Parse (resource3.Text);
            condition.resourceCount[4] = int.Parse (resource4.Text);
            condition.resourceCount[5] = int.Parse (resource5.Text);
            var boolResource6 = resource6.IsChecked ?? false;
            condition.resourceCount[6]  = boolResource6 ? 1 : 0;
            condition.resourceCount[7]  = int.Parse (resource7.Text);
            condition.resourceCount[8]  = int.Parse (resource8.Text);
            condition.resourceCount[9]  = int.Parse (resource9.Text);
            condition.resourceCount[10] = int.Parse (resource10.Text);
            condition.resourceCount[11] = int.Parse (resource11.Text);
            condition.resourceCount[12] = int.Parse (resource12.Text);
            condition.hasWater          = resource1000.IsChecked ?? false;
            condition.hasAcid           = resource1116.IsChecked ?? false;
            condition.starCount         = int.Parse (starCount.Text);
            condition.IsLogResource     = IsLogResource.IsChecked ?? false;
            return condition;
        }

        private void SetCondition (SearchCondition condition)
        {
            planetCount1.Text        = condition.planetCount1.ToString ();
            planetCount2.Text        = condition.planetCount2.ToString ();
            planetCount3.Text        = condition.planetCount3.ToString ();
            IsInBluePlanet.IsChecked = condition.isBluePlanet;
            GasCount.Text            = condition.GasCount.ToString ();
            IcePlanetCount.Text      = condition.IcePlanetCount.ToString ();
            dysonLumino.Text         = condition.dysonLumino.ToString (CultureInfo.InvariantCulture);
            distanceToBirth.Text     = condition.distanceToBirth.ToString (CultureInfo.InvariantCulture);
            
            IsInDsp.IsChecked  = condition.isInDsp;
            IsInDsp2.IsChecked = condition.isInDsp2;

            resource0.Text  = condition.resourceCount[0].ToString ();
            resource1.Text  = condition.resourceCount[1].ToString ();
            resource2.Text  = condition.resourceCount[2].ToString ();
            resource3.Text  = condition.resourceCount[3].ToString ();
            resource4.Text  = condition.resourceCount[4].ToString ();
            resource5.Text  = condition.resourceCount[5].ToString ();
            resource7.Text  = condition.resourceCount[7].ToString ();
            resource8.Text  = condition.resourceCount[8].ToString ();
            resource9.Text  = condition.resourceCount[9].ToString ();
            resource10.Text = condition.resourceCount[10].ToString ();
            resource11.Text = condition.resourceCount[11].ToString ();
            resource12.Text = condition.resourceCount[12].ToString ();

            resource6.IsChecked     = condition.resourceCount[6] > 0;
            resource1000.IsChecked  = condition.hasWater;
            resource1116.IsChecked  = condition.hasAcid;
            IsLogResource.IsChecked = condition.IsLogResource;

            starCount.Text = condition.starCount.ToString ();
        }
        private void ComboBox_SelectionChanged_Necessary(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            curSelectIndex = necessaryCondition.SelectedIndex;
            if (curSelectIndex < 0)
                return;
            SetCondition (searchNecessaryConditions[curSelectIndex]);
            curSelectLog = false;
        }

        private void ComboBox_SelectionChanged_Log(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            curSelectIndex = LogCondition.SelectedIndex;
            if (curSelectIndex < 0)
                return;
            SetCondition (searchLogConditions[curSelectIndex]);
            curSelectLog = true;
        }

        private void Button_Click_AddNecessary(object sender, System.Windows.RoutedEventArgs e)
        {
            searchNecessaryConditions.Add (FetchCondition ());
            necessaryCondition.Items.Add ("条件" + searchNecessaryConditions.Count);
            curSelectIndex = searchNecessaryConditions.Count - 1;
            necessaryCondition.Text = (string)necessaryCondition.Items[curSelectIndex];
        }

        private void Button_Click_AddLog(object sender, System.Windows.RoutedEventArgs e)
        {
            searchLogConditions.Add (FetchCondition ());
            LogCondition.Items.Add ("条件" + searchLogConditions.Count);
            curSelectIndex = searchLogConditions.Count - 1;
            LogCondition.Text = (string)LogCondition.Items[curSelectIndex];
        }

        private void Button_Click_Del(object sender, System.Windows.RoutedEventArgs e)
        {
            if (curSelectIndex < 0)
                return;
            if (curSelectLog)
            {
                searchLogConditions.RemoveAt(curSelectIndex);
                RefreshConditionUI ();
            }
            else
            {
                searchNecessaryConditions.RemoveAt(curSelectIndex);
                RefreshConditionUI ();
            }
        }

        private void RefreshConditionUI ()
        {
            var cacheIndex = curSelectIndex;
            necessaryCondition.Items.Clear();
            LogCondition.Items.Clear();
            for (int i = 0; i < searchNecessaryConditions.Count; i++)
            {
                necessaryCondition.Items.Add ("条件" + i);
            }
            for (int i = 0; i < searchLogConditions.Count; i++)
            {
                LogCondition.Items.Add ("条件" + i);
            }
            
            if (curSelectLog)
            {
                if(searchLogConditions.Count <= cacheIndex)
                {
                    cacheIndex = searchLogConditions.Count - 1;
                }
                curSelectIndex = cacheIndex;
                if (curSelectIndex < 0)
                    return;
                LogCondition.SelectedIndex = curSelectIndex;
                LogCondition.Text          = (string)LogCondition.Items[curSelectIndex];
                SetCondition (searchLogConditions[curSelectIndex]);
            }
            else
            {
                if (searchNecessaryConditions.Count <= cacheIndex)
                {
                    cacheIndex = searchNecessaryConditions.Count - 1;
                }
                curSelectIndex = cacheIndex;
                if (curSelectIndex < 0)
                    return;
                necessaryCondition.SelectedIndex = curSelectIndex;
                necessaryCondition.Text          = (string)necessaryCondition.Items[curSelectIndex];
                SetCondition (searchNecessaryConditions[curSelectIndex]);
            }
               
        }

        private void Button_Click_ImportFile(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title       = "请选择文件夹";
            dialog.Filter      = "json|*.json";
            dialog.InitialDirectory = saveConditionPath;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string text = File.ReadAllText(dialog.FileName);
                jsonCondition             = JsonMapper.ToObject<JsonCondition>(text);
                searchNecessaryConditions = jsonCondition.searchNecessaryConditions;
                searchLogConditions       = jsonCondition.searchLogConditions;
                curSelectIndex            = 0;
                curSelectLog              = false;
                RefreshConditionUI ();
            }
        }

        private void Button_Click_ExportFile(object sender, System.Windows.RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            dialog.SelectedPath = saveConditionPath;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                saveConditionPath = dialog.SelectedPath;
                SaveConditionPath.Text = saveConditionPath;
                jsonCondition.searchNecessaryConditions = searchNecessaryConditions;
                jsonCondition.searchLogConditions = searchLogConditions;
                string text = JsonMapper.ToJson(jsonCondition);
                System.IO.File.WriteAllText(saveConditionPath + "\\conditon_" + fileName + ".json", text, Encoding.UTF8);
            }
        }

        private void Button_Click_ImportSeedFile (object sender, System.Windows.RoutedEventArgs e)
        {
            
            var dialog = new OpenFileDialog();
            dialog.Multiselect      = false;
            dialog.Title            = "请选择文件夹";
            dialog.Filter           = "csv|*.csv";
            dialog.InitialDirectory = saveConditionPath;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path    = Path.GetDirectoryName (dialog.FileName);
                CustomSeedIdS = CsvUtil.OpenCSV (Path.GetDirectoryName (dialog.FileName) + "\\"+ Path.GetFileName (dialog.FileName));
                if (CustomSeedIdS == null || CustomSeedIdS.Count <= 0)
                {
                    //TODO 弹窗提示
                    return;
                }
            }
        }
      
        private void Button_Click_Start(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (StartType.SelectedIndex)
            {
                case 0 :
                    startId         = int.Parse(seedID.Text);
                    onceCount       = int.Parse(searchOnceCount.Text);
                    times           = int.Parse(searchTimes.Text);
                    curSeeds        = 0;
                    lastSeedId      = 0;
                    fileName        = FileName.Text;
                    magCount        = int.Parse(MagCount.Text);
                    bluePlanetCount = int.Parse (BluePlanetCount.Text);
                    if (curThread != null)
                        curThread.Abort();
                    curThread = new Thread(Search);
                    curThread.Start();
                    break;
                case 1 :
                    startId  = int.Parse(seedID.Text);
                    fileName = FileName.Text + "_single";
                    if (curThread != null)
                        curThread.Abort();
                    curThread = new Thread(SingleSearch);
                    curThread.Start();
                    break;
                case 2 :
                    onceCount       = int.Parse(searchOnceCount.Text);
                    times           = int.Parse(searchTimes.Text);
                    curSeeds        = 0;
                    lastSeedId      = 0;
                    fileName        = FileName.Text;
                    magCount        = int.Parse(MagCount.Text);
                    bluePlanetCount = int.Parse (BluePlanetCount.Text);
                    if (curThread != null)
                        curThread.Abort();
                    curThread = new Thread(SearchCustomID);
                    curThread.Start();
                    break;
            }
           
        }
    }
}