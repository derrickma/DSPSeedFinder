using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DysonSphereProgramSeed.Dyson;


namespace DspFindSeed
{
    public  class SearchCondition
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
        /// 是否为蓝巨星星系
        /// </summary>
        public bool isBluePlanet;
        /// <summary>
        /// 气态巨星数量
        /// </summary>
        public int GasCount = 0;
        /// <summary>
        /// 冰巨星数量
        /// </summary>
        public int IcePlanetCount;
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
        /// 是否第一颗在戴森球半径内
        /// </summary>
        public bool isInDsp2 = false;
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
            isBluePlanet    = true;
            IcePlanetCount  = 99;
            GasCount        = 99;
            dysonLumino     = 99;
            distanceToBirth = 0;
            isInDsp         = true;
            isInDsp2        = true;
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
        #region  Field
        public JsonCondition         jsonCondition             = new JsonCondition();
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition> ();
        public List<SearchCondition> searchLogConditions       = new List<SearchCondition> ();
        public SearchCondition       minConditions             = new SearchCondition ();
        /// <summary>
        /// 自定义搜索ID
        /// </summary>
        public List<int>             CustomSeedIdS;
        /// <summary>
        /// 磁石数量要求
        /// </summary>
        public int                   magCount                  = 0;
        /// <summary>
        /// 蓝巨星数量要求
        /// </summary>
        public int bluePlanetCount = 0;
        /// <summary>
        /// O恒星数量
        /// </summary>
        public int oPlanetCount = 0;
        public int       curSelectIndex = 0;
        public bool      curSelectLog   = false;
        int              startId        = 0;
        private int      curId          = 0;
        private int curSearchStarCount = 64;
        private DateTime startTime;
        int              onceCount = 1000;
        int              times     = 10;
        public  int      curSeeds;
        public  int      lastSeedId;
        public  Thread   curThread;
        public  string   fileName          = "seed";
        public  string   saveConditionPath = "";
        private string   searchlogContent  = "";
        private float    processValue      = 0;
        #endregion
        public MainWindow ()
        {
            InitializeComponent ();
            saveConditionPath = System.Environment.CurrentDirectory;
            //读dsp表
            PlanetModelingManager.Start ();
        }

        void ResetMinCondition ()
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
                if (minConditions.IcePlanetCount > condition.IcePlanetCount)
                    minConditions.IcePlanetCount = condition.IcePlanetCount;
                if (minConditions.GasCount > condition.GasCount)
                    minConditions.GasCount = condition.GasCount;
                if (minConditions.dysonLumino > condition.dysonLumino)
                    minConditions.dysonLumino = condition.dysonLumino;
                //距离是找一个比我大的，即找到所有必须条件里最大的
                if (minConditions.distanceToBirth < condition.distanceToBirth)
                    minConditions.distanceToBirth = condition.distanceToBirth;
                if(!condition.isInDsp)
                    minConditions.isInDsp = false;
                if(!condition.hasWater)
                    minConditions.hasWater = false;
                if(!condition.hasAcid)
                    minConditions.hasAcid = false;
                if(!condition.isInDsp2)
                    minConditions.isInDsp2 = false;
                if(!condition.isBluePlanet)
                    minConditions.isBluePlanet = false;
            }
        }
        void SearchCustomID ()
        {
            ResetMinCondition ();
            startTime = DateTime.Now;
            int index     = 0;
            for (int i = 0; i < CustomSeedIdS.Count; i++)
            {                
                curId = CustomSeedIdS[i];
                SeedSearch (curId);
            }
            var curTime = (DateTime.Now - startTime).TotalSeconds;
            this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)(() => 
            {
                searchlogContent  = "搜索自定义种子结束 ：用时：" + curTime + ";\n 已命中种子数量：" + curSeeds + ";最后命中的是：" + lastSeedId;
                SearchLog.Content = searchlogContent;
                process.Value     = 1;
            }));
        }
        void Search()
        {
            ResetMinCondition ();
            startTime = DateTime.Now;
            for (int j = 0; j < times; j++)
            {
                for (int i = startId + onceCount * j, max = startId + onceCount * (j + 1); i < max; i++)
                {
                    curId = i;
                    SeedSearch (curId);
                }
                var curTime = (DateTime.Now - startTime).TotalSeconds;
                var str     = "";
                if (j == times - 1)
                {
                    str += "搜索结束。";
                }
                str     += "搜索到 ：" + curId + "；用时：" + curTime + ";\n 已命中种子数量：" + curSeeds + ";最后命中的是：" + lastSeedId;
                searchlogContent  = str;
                processValue      = j * 1.0f  / times;
                this.Dispatcher.BeginInvoke ((System.Threading.ThreadStart)(() =>
                {
                    SearchLog.Content = searchlogContent;
                    process.Value     = processValue;
                }));
            }
            return;
        }

        public SearchCondition CheckMagCount(StarData star, int i, GalaxyData galaxyData, SearchCondition condition, ref int curMagCount)
        {
            //先算磁石，具体星球数据
            bool            hasWater       = false;
            bool            hasAcid        = false;
            var             planetCount1   = 0;
            var             planetCount2   = 0;
            var             gas            = 0; //巨星数量
            var             extraGas       = 0; //多卫星的巨星,如果多巨星两个
            var             gasCount       = 0; //气态巨星数量
            var             icePlanetCount = 0;//冰巨星数量
            SearchCondition data           = new SearchCondition ();
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
                    if (planet.typeString == "冰巨星")
                    {
                        icePlanetCount++;
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
            if (icePlanetCount < condition.IcePlanetCount)
                return null;
            if(planetCount1 < condition.planetCount1)
                return null;
            if(planetCount2 < condition.planetCount2)
                return null;
            if (!hasWater && condition.hasWater)
                return null;
            if (!hasAcid && condition.hasAcid)
                return null;
            
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
            bool isInDsp2 = star.planets.Length >= 2 &&  star.dysonRadius * 2 > star.planets[1].sunDistance;
            if(condition.isInDsp2 && !isInDsp2)
                return null;
        
            data.GasCount        = gasCount;
            data.IcePlanetCount  = icePlanetCount;
            data.planetCount3    = star.planetCount;
            data.dysonLumino     = star.dysonLumino;
            data.distanceToBirth = distanceToBirth;
            data.hasWater        = hasWater;
            data.hasAcid         = hasAcid;
            data.planetCount1    = planetCount1;
            data.planetCount2    = planetCount2;
            data.isInDsp         = isInDsp;
            data.isInDsp2        = isInDsp2;
            data.isBluePlanet    = false;
            return data;
        }

        public SearchCondition Check (StarData star, int i, GalaxyData galaxyData, SearchCondition condition,  ref int curBluePlanetCount, ref int curOPlanetCount)
        {
            //只有i = 62、63需要算磁石的数量
            bool isBluePlanet = star.typeString == "蓝巨星";
            if (isBluePlanet)
                curBluePlanetCount++;
            bool isOPlanetCount = star.typeString == "O型恒星";
            if (isOPlanetCount)
                curOPlanetCount++;
            //先算完ref的值，才会return;
            if (condition.isBluePlanet && !isBluePlanet)
                return null;
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
            bool isInDsp2 = star.planets.Length >= 2 && star.dysonRadius * 2 > star.planets[1].sunDistance;
            if(condition.isInDsp2 && !isInDsp2)
                return null;
            
            bool            hasWater       = false;
            bool            hasAcid        = false;
            var             planetCount1   = 0;
            var             planetCount2   = 0;
            var             gas            = 0; //巨星数量
            var             extraGas       = 0; //多卫星的巨星,如果多巨星两个
            var             gasCount       = 0; //气态巨星数量
            var             icePlanetCount = 0;//冰巨星数量
            SearchCondition data           = new SearchCondition ();
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
                if (planet.singularity.HasFlag (EPlanetSingularity.TidalLocked))
                    planetCount2++;
                if (planet.type == EPlanetType.Gas)
                {
                    gas++;
                    //减少字符串比较
                    if (planet.typeString == "气态巨星")
                    {
                        gasCount++;
                    }
                    if (planet.typeString == "冰巨星")
                    {
                        icePlanetCount++;
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
            if (icePlanetCount < condition.IcePlanetCount)
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
            data.IcePlanetCount  = icePlanetCount;
            data.dysonLumino     = star.dysonLumino;
            data.distanceToBirth = distanceToBirth;
            data.hasWater        = hasWater;
            data.hasAcid         = hasAcid;
            data.planetCount1    = planetCount1;
            data.planetCount2    = planetCount2;
            data.isInDsp         = isInDsp;
            data.isInDsp2        = isInDsp2;
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
            gd.SetForNewGame (id, curSearchStarCount);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            Dictionary<int, List<SearchCondition>> necessaryShortStarDatas = new Dictionary<int,  List<SearchCondition>>();
            Dictionary<int, List<SearchCondition>> logShortStarDatas       = new Dictionary<int,  List<SearchCondition>>();
            int                                    curMagCount             = 0;
            int                                    curBluePlanetCount             = 0;
            int curOPlanetCount = 0;
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var star = galaxyData.stars[i];
                SearchCondition shortData = null;
                //如果星系不是中子和黑洞，则不计算磁石数量，不用上来就计算矿物数量
                if ( star.type != EStarType.BlackHole && star.type != EStarType.NeutronStar)
                {
                    //保证满足最低条件，并取到星系的各个计算值。如果不满足最低条件，说该星系任意一个必须条件都不满足，直接下一个星系
                    shortData = Check (star, i, galaxyData, minConditions, ref curBluePlanetCount, ref curOPlanetCount);
                    if (shortData == null)
                        continue;
                }
                else //否则一定要算完磁石总数才会结束
                {
                    //只有最后两个星系才会有磁石
                    shortData = CheckMagCount (star, i, galaxyData, minConditions, ref curMagCount);
                    if (shortData == null)
                        continue;
                }
             
                //先检查必须条件，如果check成功记录下来(且继续检查，因为可以同条件），check失败检查下一个条件
                for (int j = 0, maxConditions = searchNecessaryConditions.Count; j < maxConditions; j++)
                {
                    var condition = searchNecessaryConditions[j];
                    if(!Check (star, shortData, galaxyData, condition))
                        continue;
                    if(!necessaryShortStarDatas.ContainsKey (j))
                        necessaryShortStarDatas.Add (j, new List<SearchCondition> ());
                    var listShortData = necessaryShortStarDatas[j];
                    listShortData.Add (shortData);
                }
                //记录条件一样
                for (int j = 0, maxConditions = searchLogConditions.Count; j < maxConditions; j++)
                {
                    var condition = searchLogConditions[j];
                    if(!Check (star, shortData, galaxyData, condition))
                        continue;
                    if(!logShortStarDatas.ContainsKey (j))
                        logShortStarDatas.Add (j, new List<SearchCondition> ());
                    var listShortData = logShortStarDatas[j];
                    listShortData.Add (shortData);
                }
            }
            if (curMagCount < magCount)
                return;
            if (curBluePlanetCount < bluePlanetCount)
                return;
            if (curOPlanetCount < oPlanetCount)
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
            LogFile (curMagCount, curBluePlanetCount, curOPlanetCount, necessaryShortStarDatas, logShortStarDatas);
        }
        string SingleTitle = "星系名字,亮度,行星,距离,星系类型,是否环内行星,气态巨星数量,冰巨星数量,卫星总数,潮汐锁定,是否有水,是否有硫酸,铁矿脉,铜矿脉,硅矿脉,钛矿脉,石矿脉,煤矿脉,原油涌泉,可燃冰矿,金伯利矿,分形硅矿,有机晶体矿,光栅石矿,刺笋矿脉,单极磁矿\n";
        public void LogFile (int curMagCount, int curBluePlanetCount, int curOPlanetCount, Dictionary<int, List<SearchCondition>> necessaryShortStarDatas, Dictionary<int, List<SearchCondition>> logShortStarDatas)
        {
            var str = lastSeedId  + ",磁石： " + curMagCount + "," + "蓝巨星：" + curBluePlanetCount + "," + "O型恒星：" + curOPlanetCount + ",";
            foreach (var item in necessaryShortStarDatas)
            {
                str += "条件" + item.Key + ",";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var data = item.Value[i];
                    str += i + "号,卫星:" + data.planetCount1 + ";潮汐" + data.planetCount2 + ";行星" + data.planetCount3 + ";气态巨星" + data.GasCount + ";冰巨星" + data.IcePlanetCount + ";光度" + data.dysonLumino + ";与初始距离" 
                         + data.distanceToBirth.ToString("F1")  + ";戴森球" + (data.isInDsp ? "包括" : "不包括") + "第一行星;"+ ";戴森球" + (data.isInDsp2 ? "包括" : "不包括") + "第二行星;" + (data.isBluePlanet ? "是" : "不是") + "蓝巨星星系;" + (data.hasWater ? "有" : "没有") + "水;" + (data.hasWater ? "有" : "没有") + "硫酸";
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
            foreach (var item in logShortStarDatas)
            {
                str += "条件" + item.Key + ",";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var data = item.Value[i];
                    str += i + "号,卫星:" + data.planetCount1 + ";潮汐" + data.planetCount2 + ";行星" + data.planetCount3 + ";气态巨星" + data.GasCount + ";冰巨星" + data.IcePlanetCount + ";光度" + data.dysonLumino + ";与初始距离" 
                         + data.distanceToBirth.ToString("F1")  + ";戴森球" + (data.isInDsp ? "包括" : "不包括") + "第一行星;"+ ";戴森球" + (data.isInDsp2 ? "包括" : "不包括") + "第二行星;" + (data.isBluePlanet ? "是" : "不是") + "蓝巨星星系;" + (data.hasWater ? "有" : "没有") + "水;" + (data.hasWater ? "有" : "没有") + "硫酸";
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
            System.IO.File.AppendAllText(System.Environment.CurrentDirectory + "\\" + fileName + ".csv", str,Encoding.UTF8);
        }
        
        void SingleSearch()
        {
            GameDesc gd    = new GameDesc ();
            gd.SetForNewGame (startId, curSearchStarCount);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var  star         = galaxyData.stars[i];
                var  distanc = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
                bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
                SingleTitle += star.name + "," + star.dysonLumino + "," + star.planetCount + "," + distanc + "," + star.typeString + "," +  (isInDsp?"是":"否") + ",";
                var   gasCount1    = 0; //气态巨星
                var   gasCount2    = 0; //冰巨星
                int   cxsdcount    = 0;
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
                SearchLog.Content =  "成功写入单个种子 ：" + startId + "的所有信息";
            }));
        }
        
    }
}