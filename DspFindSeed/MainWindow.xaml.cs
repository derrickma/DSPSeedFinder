using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DysonSphereProgramSeed.Algorithms;
using DysonSphereProgramSeed.Dyson;

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
        /// 光度
        /// </summary>
        public float dysonLumino     = 0;
        /// <summary>
        /// 距离初始星球
        /// </summary>
        public float distanceToBirth = 0;
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

        public void Reset ()
        {
            planetCount1    = 99;
            planetCount2    = 99;
            planetCount3    = 99;
            dysonLumino     = 99;
            distanceToBirth = 0;
            isInDsp         = true;
            hasWater = true;
            hasAcid  = true;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition> ();
        public List<SearchCondition> searchLogConditions       = new List<SearchCondition> ();
        public SearchCondition       minConditions             = new SearchCondition ();
        public int                   magCount                  = 0;
        public int                   curSelectIndex            = 0;
        public bool                  curSelectLog              = false;
        int                          startID                   = 0;
        int                          onceCount                 = 1000;
        int                          times                     = 10;
        public MainWindow ()
        {
            InitializeComponent ();
            //读表
            PlanetModelingManager.Start ();
        }
        private string searchlogContent = "";
        private float processValue = 0;

        private SearchCondition FetchCondition ()
        {
            var condition = new SearchCondition ();
            condition.planetCount1       = int.Parse (planetCount1.Text);
            condition.planetCount2       = int.Parse (planetCount2.Text);
            condition.planetCount3       = int.Parse (planetCount3.Text);
            condition.dysonLumino       = float.Parse (dysonLumino.Text);
            condition.distanceToBirth   = float.Parse (distanceToBirth.Text);
            condition.isInDsp           = IsInDsp.IsChecked ?? false;
            condition.resourceCount[0]  = int.Parse (resource0.Text);
            condition.resourceCount[1]  = int.Parse (resource1.Text);
            condition.resourceCount[2]  = int.Parse (resource2.Text);
            condition.resourceCount[3]  = int.Parse (resource3.Text);
            condition.resourceCount[4]  = int.Parse (resource4.Text);
            condition.resourceCount[5]  = int.Parse (resource5.Text);
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
            planetCount1.Text       = condition.planetCount1.ToString ();
            planetCount2.Text       = condition.planetCount2.ToString ();
            planetCount3.Text       = condition.planetCount3.ToString ();
            dysonLumino.Text        = condition.dysonLumino.ToString (CultureInfo.InvariantCulture);
            distanceToBirth.Text    = condition.distanceToBirth.ToString (CultureInfo.InvariantCulture);
            
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
            magCount = int.Parse (MagCount.Text);
            searchNecessaryConditions.Add (FetchCondition ());
            necessaryCondition.Items.Add ("条件" + searchNecessaryConditions.Count);
        }

        private void Button_Click_AddLog(object sender, System.Windows.RoutedEventArgs e)
        {
            searchLogConditions.Add (FetchCondition ());
            LogCondition.Items.Add ("条件" + searchLogConditions.Count);
        }

        private void Button_Click_Del(object sender, System.Windows.RoutedEventArgs e)
        {
            if (curSelectIndex < 0)
                return;
            if (curSelectLog)
            {
                searchLogConditions.RemoveAt(curSelectIndex);
                LogCondition.Items.RemoveAt(curSelectIndex);
                curSelectIndex--;
                if(curSelectIndex >=0)
                    LogCondition.SelectedIndex = curSelectIndex;
            }
            else
            {
                searchNecessaryConditions.RemoveAt(curSelectIndex);
                necessaryCondition.Items.RemoveAt(curSelectIndex);
                curSelectIndex--;
                if (curSelectIndex >= 0)
                    necessaryCondition.SelectedIndex = curSelectIndex;
            }
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
                if (minConditions.dysonLumino > condition.dysonLumino)
                    minConditions.dysonLumino = condition.dysonLumino;
                if (minConditions.distanceToBirth > condition.distanceToBirth)
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
                    Search (i);
                }
                var curTime = (DateTime.Now - startTime).TotalSeconds;
                this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)(() => 
                {
                    searchlogContent = "搜索到 ：" + (startID + onceCount * (j + 1)) + "；用时：" + curTime;
                    processValue = j  / times;
                    SearchLog.Content = searchlogContent;
                    process.Value = processValue;
                }));
            }
        }
      
        private void Button_Click_Start(object sender, System.Windows.RoutedEventArgs e)
        {
            startID = int.Parse(seedID.Text);
            onceCount = int.Parse(searchOnceCount.Text);
            times = int.Parse(searchTimes.Text);
            Thread t = new Thread(Search);
            t.Start();
        }

        public SearchCondition Check (StarData star, int i, GalaxyData galaxyData, SearchCondition condition)
        {
            if(star.planetCount < condition.planetCount3)
                return null;
            if(star.dysonLumino < condition.dysonLumino)
                return null;
            float distanceToBirth = 0;
            if(i > 0)
              distanceToBirth = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
            if(distanceToBirth < condition.distanceToBirth)
                return null;
            bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
            if(condition.isInDsp && !isInDsp)
                return null;
            bool            hasWater     = false;
            bool            hasAcid      = false;
            var             planetCount1 = 0;
            var             planetCount2 = 0;
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
                if (planet.type != EPlanetType.Gas && planet.veinSpotsSketch != null)
                {
                    for (int k = 0; k < data.resourceCount.Length; k++)
                    {
                        data.resourceCount[k] += planet.veinSpotsSketch[k + 1];
                    }
                }
            }
            if(planetCount1 < condition.planetCount1)
                return null;
            if(planetCount2 < condition.planetCount2)
                return null;
            if (!hasWater && condition.hasWater)
                return null;
            if (!hasAcid && condition.hasAcid)
                return null;

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
        
        public void Search (int id)
        {
     
            GameDesc                             gd                      = new GameDesc ();
            gd.SetForNewGame (id, 64);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            Dictionary<int, List<SearchCondition>> necessaryShortStarDatas = new Dictionary<int,  List<SearchCondition>>();
            Dictionary<int, List<SearchCondition>> logShortStarDatas       = new Dictionary<int,  List<SearchCondition>>();
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var star = galaxyData.stars[i];
                //保证满足最低条件，并取到星系的各个计算值。如果不满足最低条件，说该星系任意一个必须条件都不满足，直接下一个星系
                var shorData = Check (star, i, galaxyData, minConditions);
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
            LogFile (necessaryShortStarDatas, logShortStarDatas);

        }

        public void LogFile ( Dictionary<int, List<SearchCondition>> necessaryShortStarDatas, Dictionary<int, List<SearchCondition>> logShortStarDatas)
        {
            var str = "";
            foreach (var item in necessaryShortStarDatas)
            {
                str += "条件" + item.Key + ",";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var data = item.Value[i];
                    str += i + "号，卫星:" + data.planetCount1 + ";潮汐" + data.planetCount2 + ";行星" + data.planetCount3 + ";光度" + data.dysonLumino + ";与初始距离" + data.distanceToBirth
                         + ";戴森球" + (data.isInDsp ? "包括" : "不包括") + "第一行星;" + (data.hasWater ? "有" : "没有") + "水;" + (data.hasWater ? "有" : "没有") + "硫酸";
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

    }
}