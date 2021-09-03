using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using  LitJson;
using DysonSphereProgramSeed.Dyson;
using System.Windows.Forms;
using System.IO;

namespace DspFindSeed
{
    public class SearchCondition
    {
        /// <summary>
        /// 卫星数量
        /// </summary>
        public int   planetCount1     = 0;
        /// <summary>
        /// 潮汐锁定
        /// </summary>
        public int   planetCount2     = 0;
        /// <summary>
        /// 总行星
        /// </summary>
        public int   planetCount3     = 0;
        /// <summary>
        /// 气态巨星数量
        /// </summary>
        public int GasCount = 0;
        /// <summary>
        /// 光度
        /// </summary>
        public double dysonLumino     = 0;
        /// <summary>
        /// 距离初始星球
        /// </summary>
        public double distanceToBirth = 0;
        /// <summary>
        /// 是否第一颗在戴森球半径内
        /// </summary>
        public bool  isInDsp         = false;
        /// <summary>
        /// 所有资源限制
        /// </summary>
        public int[] resourceCount   = new int[13];
        /// <summary>
        /// 是否有水
        /// </summary>
        public bool  hasWater        = false;
        /// <summary>
        /// 是否有硫酸
        /// </summary>
        public bool  hasAcid         = false;
        /// <summary>
        /// 满足条件的星系至少几个
        /// </summary>
        public int starCount;
        /// <summary>
        /// 是否存矿物数量
        /// </summary>
        public bool IsLogResource;
        /// <summary>
        /// 磁石数量
        /// </summary>
        public int magCount;

        public void Reset ()
        {
            planetCount1    = 99;
            planetCount2    = 99;
            planetCount3    = 99;
            GasCount        = 99;
            dysonLumino     = 99;
            distanceToBirth = 0;
            isInDsp         = true;
            hasWater        = true;
            hasAcid         = true;
        }
    }

    public class JsonCondition
    {
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition>();
        public List<SearchCondition> searchLogConditions = new List<SearchCondition>();
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public JsonCondition jsonCondition = new JsonCondition();
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition> ();
        public List<SearchCondition> searchLogConditions       = new List<SearchCondition> ();
        public SearchCondition       minConditions             = new SearchCondition ();
        public int                   magCount                  = 0;
        public int                   curSelectIndex            = 0;
        public bool                  curSelectLog              = false;
        int                          startID                   = 0;
        int                          onceCount                 = 1000;
        int                          times                     = 10;
        public int                   curSeeds;
        public int                   lastSeedId;
        public Thread                curThread;
        public string fileName = "seed";
        public string saveConditionPath = "";
        public MainWindow ()
        {
            InitializeComponent ();
            saveConditionPath = System.Environment.CurrentDirectory;
            //读表
            PlanetModelingManager.Start ();
        }
        private string searchlogContent = "";
        private float processValue = 0;

        private SearchCondition FetchCondition ()
        {
            var condition = new SearchCondition ();
            condition.planetCount1     = int.Parse (planetCount1.Text);
            condition.planetCount2     = int.Parse (planetCount2.Text);
            condition.planetCount3     = int.Parse (planetCount3.Text);
            condition.GasCount         = int.Parse (GasCount.Text);
            condition.dysonLumino      = float.Parse (dysonLumino.Text);
            condition.distanceToBirth  = float.Parse (distanceToBirth.Text);
            condition.isInDsp          = IsInDsp.IsChecked ?? false;
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
            planetCount1.Text    = condition.planetCount1.ToString ();
            planetCount2.Text    = condition.planetCount2.ToString ();
            planetCount3.Text    = condition.planetCount3.ToString ();
            GasCount.Text        = condition.GasCount.ToString ();
            dysonLumino.Text     = condition.dysonLumino.ToString (CultureInfo.InvariantCulture);
            distanceToBirth.Text = condition.distanceToBirth.ToString (CultureInfo.InvariantCulture);
            
            IsInDsp.IsChecked       = condition.isInDsp;

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
      
        private void Button_Click_Start(object sender, System.Windows.RoutedEventArgs e)
        {
            startID    = int.Parse(seedID.Text);
            onceCount  = int.Parse(searchOnceCount.Text);
            times      = int.Parse(searchTimes.Text);
            curSeeds   = 0;
            lastSeedId = 0;
            fileName = FileName.Text;
            magCount = int.Parse(MagCount.Text);
            if (curThread != null)
                curThread.Abort();
            curThread  = new Thread(Search);
            curThread.Start();
        }

        private void Button_Click_SingleStart(object sender, System.Windows.RoutedEventArgs e)
        {
            startID = int.Parse(seedID.Text);
            fileName = FileName.Text + "_single";
            if (curThread != null)
                curThread.Abort();
            curThread = new Thread(SingleSearch);
            curThread.Start();
        }

   


        void Search()
        {
            if (searchNecessaryConditions.Count == 0)
                return;
            minConditions.Reset ();
            //对条件进行优化加工
            foreach (var condition in searchNecessaryConditions)
            {
                if (minConditions.planetCount1 > condition.planetCount1)
                    minConditions.planetCount1 = condition.planetCount1;
                if (minConditions.planetCount2 > condition.planetCount2)
                    minConditions.planetCount2 = condition.planetCount2;
                if (minConditions.planetCount3 > condition.planetCount3)
                    minConditions.planetCount3 = condition.planetCount3;
                if (minConditions.GasCount > condition.GasCount)
                    minConditions.GasCount = condition.GasCount;
                if (minConditions.dysonLumino > condition.dysonLumino)
                    minConditions.dysonLumino = condition.dysonLumino;
                //距离是找一个比我大的，即找到所有必须条件里最大的
                if (minConditions.distanceToBirth < condition.distanceToBirth)
                    minConditions.distanceToBirth = condition.distanceToBirth;
                if(!condition.isInDsp)
                    minConditions.isInDsp       = false;
                if(!condition.hasWater)
                    minConditions.hasWater = false;
                if(!condition.hasAcid)
                    minConditions.hasAcid = false;
            }
            var startTime = DateTime.Now;
            for (int j = 0; j < times; j++)
            {
                for (int i = startID + onceCount * j, max = startID + onceCount * (j + 1); i < max; i++)
                {
                    SeedSearch (i);
                }
                var curTime = (DateTime.Now - startTime).TotalSeconds;
                this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)(() => 
                {
                    searchlogContent = "搜索到 ：" + (startID + onceCount * (j + 1)) + "；用时：" + curTime + ";\n 已命中种子数量：" + curSeeds + ";最后命中的是：" + lastSeedId;
                    processValue = j  / times;
                    SearchLog.Content = searchlogContent;
                    process.Value = processValue;
                }));
                
            }
        }

        public SearchCondition Check (StarData star, int i, GalaxyData galaxyData, SearchCondition condition, ref int curMagCount)
        {
            if(star.planetCount < condition.planetCount3)
                return null;
            if(star.dysonLumino < condition.dysonLumino)
                return null;
            double distanceToBirth = 0;
            if(i > 0)
              distanceToBirth = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
            //如果距离母星的距离比最大距离要求都远，说明不满足任意一个必须条件
            if(distanceToBirth > condition.distanceToBirth)
                return null;
            bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
            if(condition.isInDsp && !isInDsp)
                return null;
            
            bool            hasWater     = false;
            bool            hasAcid      = false;
            var             planetCount1 = 0;
            var             planetCount2 = 0;
            var             gas          = 0; //巨星数量
            var             extraGas     = 0; //多卫星的巨星,如果多巨星两个
            var             gasCount     = 0;//气态巨星数量
            SearchCondition data         = new SearchCondition ();
            for (int j = 0; j < star.planets.Length; j++)
            {
                var planet = star.planets[j];
                DspData.PlanetCompute (galaxyData, star, planet);
                if (planet.waterItemId == 1000)
                    hasWater = true;
                if (planet.waterItemId == 1116)
                    hasAcid = true;
                if (planet.orbitAroundPlanet != null)
                    planetCount1++;
                if (planet.singularity.HasFlag (EPlanetSingularity.TidalLocked) || planet.singularity.HasFlag (EPlanetSingularity.TidalLocked2)
                                                                                || planet.singularity.HasFlag (EPlanetSingularity.TidalLocked4))
                    planetCount2++;
                if (planet.type == EPlanetType.Gas)
                {
                    gas++;
                    //减少字符串比较
                    if (planet.typeString == "气态巨星")
                    {
                        gasCount++;
                    }
                }
                if (planet.singularityString.Contains ("多卫星"))
                    extraGas++;
                if (planet.type != EPlanetType.Gas && planet.veinSpotsSketch != null)
                {
                    for (int k = 0; k < data.resourceCount.Length; k++)
                    {
                        data.resourceCount[k] += planet.veinSpotsSketch[k + 1];
                    }
                    curMagCount += planet.veinSpotsSketch[14];
                }
            }
            //有2个以上的巨星时候，单巨星卫星数量对应要减少
            if (gas >= 2)
                planetCount2 -= gas - 1;
            //有2个以上多巨星时候，单巨星卫星数量继续减少
            if (extraGas >= 2)
                planetCount2 -= extraGas - 1;
            if (gasCount < condition.GasCount)
                return null;
            if(planetCount1 < condition.planetCount1)
                return null;
            if(planetCount2 < condition.planetCount2)
                return null;
            if (!hasWater && condition.hasWater)
                return null;
            if (!hasAcid && condition.hasAcid)
                return null;

            data.GasCount        = gasCount;
            data.planetCount3    = star.planetCount;
            data.dysonLumino     = star.dysonLumino;
            data.distanceToBirth = distanceToBirth;
            data.hasWater        = hasWater;
            data.hasAcid         = hasAcid;
            data.planetCount1    = planetCount1;
            data.planetCount2    = planetCount2;
            data.isInDsp         = isInDsp;
            return data;
        }
        
        public bool Check (StarData star,SearchCondition shortStarData, GalaxyData galaxyData, SearchCondition condition)
        {
            if(star.planetCount < condition.planetCount3)
                return false;
            if(star.dysonLumino < condition.dysonLumino)
                return false;
            if(shortStarData.distanceToBirth > condition.distanceToBirth)
                return false;
            if(!shortStarData.isInDsp && condition.isInDsp)
                return false;
            if(shortStarData.planetCount1 < condition.planetCount1)
                return false;
            if(shortStarData.planetCount2 < condition.planetCount2)
                return false;
            if (shortStarData.GasCount < condition.GasCount)
                return false;
            if (!shortStarData.hasWater && condition.hasWater)
                return false;
            if (!shortStarData.hasAcid && condition.hasAcid)
                return false;
            for (int k = 0; k < shortStarData.resourceCount.Length; k++)
            {
                if (shortStarData.resourceCount[k] < condition.resourceCount[k])
                    return false;
            }
            
            return true;
        }
        
        public void SeedSearch(int id)
        {
     
            GameDesc                             gd                      = new GameDesc ();
            gd.SetForNewGame (id, 64);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            Dictionary<int, List<SearchCondition>> necessaryShortStarDatas = new Dictionary<int,  List<SearchCondition>>();
            Dictionary<int, List<SearchCondition>> logShortStarDatas       = new Dictionary<int,  List<SearchCondition>>();
            int                                    curMagCount                = 0;
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var star = galaxyData.stars[i];
                //保证满足最低条件，并取到星系的各个计算值。如果不满足最低条件，说该星系任意一个必须条件都不满足，直接下一个星系
                var shorData = Check (star, i, galaxyData, minConditions, ref curMagCount);
                if (shorData == null)
                    continue;
                //先检查必须条件，如果check成功记录下来(且继续检查，因为可以同条件），check失败检查下一个条件
                for (int j = 0, maxConditions = searchNecessaryConditions.Count; j < maxConditions; j++)
                {
                    var condition = searchNecessaryConditions[j];
                    if(!Check (star, shorData, galaxyData, condition))
                        continue;
                    if(!necessaryShortStarDatas.ContainsKey (j))
                        necessaryShortStarDatas.Add (j, new List<SearchCondition> ());
                    var listShortData = necessaryShortStarDatas[j];
                    listShortData.Add (shorData);
                }
                //记录条件一样
                for (int j = 0, maxConditions = searchLogConditions.Count; j < maxConditions; j++)
                {
                    var condition = searchLogConditions[j];
                    if(!Check (star, shorData, galaxyData, condition))
                        continue;
                    if(!logShortStarDatas.ContainsKey (j))
                        logShortStarDatas.Add (j, new List<SearchCondition> ());
                    var listShortData = logShortStarDatas[j];
                    listShortData.Add (shorData);
                }
            }
            if (curMagCount < magCount)
                return;
            bool isFail = false;
            //现在还要检查必须条件的星系数量是否够
            for (int j = 0, maxConditions = searchNecessaryConditions.Count; j < maxConditions; j++)
            {
                //一个都没有直接失败了
                if (!necessaryShortStarDatas.ContainsKey (j))
                {
                    isFail = true;
                    break;
                }
                var condition = searchNecessaryConditions[j];
                if (condition.starCount > necessaryShortStarDatas[j].Count)
                {
                    isFail = true;
                    break;
                }
            }
            if (isFail)
                return;
            curSeeds++;
            lastSeedId = galaxyData.seed;
            LogFile (curMagCount, necessaryShortStarDatas, logShortStarDatas);
        }

        public void LogFile (int curMagCount, Dictionary<int, List<SearchCondition>> necessaryShortStarDatas, Dictionary<int, List<SearchCondition>> logShortStarDatas)
        {
            var str = "磁石总数 ： " + curMagCount + ",";
            foreach (var item in necessaryShortStarDatas)
            {
                str += "条件" + item.Key + ",";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var data = item.Value[i];
                    str += i + "号,卫星:" + data.planetCount1 + ";潮汐" + data.planetCount2 + ";行星" + data.planetCount3 + ";气态巨星" + data.GasCount + ";光度" + data.dysonLumino + ";与初始距离" 
                         + data.distanceToBirth.ToString("F1") + ";戴森球" + (data.isInDsp ? "包括" : "不包括") + "第一行星;" + (data.hasWater ? "有" : "没有") + "水;" + (data.hasWater ? "有" : "没有") + "硫酸";
                    if(data.IsLogResource)
                    {
                        for (int k = 0; k < data.resourceCount.Length; k++)
                        {
                            var count = data.resourceCount[k];
                            var name  = LDB.veins.dataArray[k];
                            str += ";" + name + ":" + count + ";";
                        }
                    }
                    str += ",";
                }
            }
            str += "\n";
            System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\seed.csv", str,Encoding.UTF8);
        }

        
        string SingleTitle = "星系名字,亮度,行星,距离,是否环内行星,气态巨星数量,冰巨星数量,卫星总数,潮汐锁定,是否有水,是否有硫酸,铁矿脉,铜矿脉,硅矿脉,钛矿脉,石矿脉,煤矿脉,原油涌泉,可燃冰矿,金伯利矿,分形硅矿,有机晶体矿,光栅石矿,刺笋矿脉,单极磁矿\n";
        void SingleSearch()
        {
            GameDesc gd    = new GameDesc ();
            gd.SetForNewGame (startID, 64);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var  star    = galaxyData.stars[i];
                var  distanc = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
                bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
                SingleTitle += star.name + "," + star.dysonLumino + "," + star.planetCount + "," + distanc + "," + (isInDsp?"是":"否") + ",";
                var   gasCount1    = 0; //气态巨星
                var   gasCount2    = 0; //冰巨星
                int   cxsdcount    = 0;
                int   gzs          = 0;
                int[] resource     = new int[LDB.veins.Length];
                bool  hasWater     = false;
                bool  hasAcid      = false;
                var   planetCount1 = 0;
                foreach (var planet in star.planets)
                {
                    DspData.PlanetCompute (galaxyData, star, planet);
                    if (planet.waterItemId == 1000)
                        hasWater = true;
                    if (planet.waterItemId == 1116)
                        hasAcid = true;
                    if (planet.typeString == "气态巨星")
                    {
                        gasCount1++;
                    }
                    if (planet.typeString == "冰巨星")
                    {
                        gasCount2++;
                    }
                    if (planet.orbitAroundPlanet != null)
                        planetCount1++;
                    if (planet.singularityString.Contains ("潮汐锁定"))
                        cxsdcount++;
                    for(int j = 0; j < LDB.veins.Length; j++)
                    {
                        var count = star.GetResourceSpots (j + 1);
                        resource[j] += count;
                    }
                }
                SingleTitle += gasCount1 +  "," + gasCount2 + "," + planetCount1 + ","+ cxsdcount +"," + (hasWater?"是,":"否,") +  (hasAcid?"是,":"否,");
                
                for(int j = 0; j < LDB.veins.Length; j++)
                {
                    var name = LDB.veins.dataArray[j].name;
                    SingleTitle += resource[j] + ",";
                }
                SingleTitle += "\n";
            }
            System.IO.File.WriteAllText(System.Environment.CurrentDirectory + "\\seedSingle.csv", SingleTitle,Encoding.UTF8);
            this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)(() => 
            {
                SearchLog.Content =  "成功写入单个种子 ：" + startID + "的所有信息";
            }));
        }
        
    }
}