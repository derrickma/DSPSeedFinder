using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DysonSphereProgramSeed.Dyson;
using Newtonsoft.Json;

namespace DspFindSeed
{
    public enum enumStarType
    {
        None,
        BornStar,
        BlueStar,
        OtherGiantStar,
        OStar,
        NeutronStar,
        BlackHole,
        WhiteDwarf,
        BStar,
        AStar,
        NormalStar,
    }

    public class SearchCondition
    {
        public string       starName;
        public enumStarType starType = enumStarType.None;
        /// <summary>
        /// 卫星数量
        /// </summary>
        public int planetCount1 = 0;
        /// <summary>
        /// 潮汐锁定
        /// </summary>
        public int planetCount2 = 0;
        /// <summary>
        /// 总行星
        /// </summary>
        public int planetCount3 = 0;
        /// <summary>
        /// 光度
        /// </summary>
        public double dysonLumino = 0;
        /// <summary>
        /// 距离初始星球
        /// </summary>
        public double distanceToBirth = 0;
        /// <summary>
        /// 是否第一颗在戴森球半径内
        /// </summary>
        public bool isInDsp = false;
        /// <summary>
        /// 是否第一颗在戴森球半径内
        /// </summary>
        public bool isInDsp2 = false;
        /// <summary>
        /// 所有资源限制
        /// </summary>
        public int[] resourceCount = new int[14];
        /// <summary>
        /// 是否有水
        /// </summary>
        public bool hasWater = false;
        /// <summary>
        /// 是否有硫酸
        /// </summary>
        public bool hasAcid = false;
        /// <summary>
        /// 满足条件的星系至少几个
        /// </summary>
        public int starCount;
        /// <summary>
        /// 是否存矿物数量
        /// </summary>
        public bool IsLogResource;
        /// <summary>
        /// 重氢速率
        /// </summary>
        public double gasSpeed;

        //理论上只有6个星球最多
        public int[] planetNames = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public Dictionary<int, int> planetNameCounts = new Dictionary<int, int> ();

        public void Reset ()
        {
            planetCount1    = 99;
            planetCount2    = 99;
            planetCount3    = 99;
            dysonLumino     = 99;
            gasSpeed        = 99;
            distanceToBirth = 0;
            isInDsp         = true;
            isInDsp2        = true;
            hasWater        = true;
            hasAcid         = true;
        }
    }

    public class SearchConfig
    {
        /// <summary>
        /// 磁石数量要求
        /// </summary>
        public int magCount = 0;
        /// <summary>
        /// 蓝巨星数量要求
        /// </summary>
        public int bluePlanetCount = 0;
        /// <summary>
        /// O恒星数量
        /// </summary>
        public int oPlanetCount = 0;

        public int curMinSearchStarSelectIndex = 32;
        public int curMaxSearchSelectIndex     = 32;

        public int onceCount = 1000;
        public int times     = 100;
    }

    public class JsonCondition
    {
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition> ();
        public List<SearchCondition> searchLogConditions       = new List<SearchCondition> ();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
    #region Field
        public JsonCondition         jsonCondition             = new JsonCondition ();
        public List<SearchCondition> searchNecessaryConditions = new List<SearchCondition> ();
        public List<SearchCondition> searchLogConditions       = new List<SearchCondition> ();
        public SearchCondition       minConditions             = new SearchCondition ();
        public SearchConfig          searchConfig              = new SearchConfig ();
        /// <summary>
        /// 自定义搜索ID
        /// </summary>
        public List<int> CustomSeedIdS;
        /// <summary>
        /// 自定义搜索ID
        /// </summary>
        public List<int> CustomSeedStarCounts;
        /// <summary>
        /// 磁石数量要求
        /// </summary>
        public int magCount = 0;
        /// <summary>
        /// 蓝巨星数量要求
        /// </summary>
        public int bluePlanetCount = 0;
        /// <summary>
        /// O恒星数量
        /// </summary>
        public int oPlanetCount = 0;
        public int       curSelectIndex        = 0;
        public bool      curSelectLog          = false;
        int              startId               = 0;
        private int      curId                 = 0;
        private bool     logInit               = false;
        private int      curMinSearchStarCount = 64;
        private int      curMaxSearchStarCount = 64;
        private DateTime startTime;
        int              onceCount = 1000;
        int              times     = 100;
        public  int      curSeeds;
        public  int      lastSeedId;
        public  Thread   curThread;
        public  string   fileName          = "seed";
        public  string   saveConditionPath = "";
        private string   searchlogContent  = "";
        private float    processValue      = 0;

        private Dictionary<string, int> AllPlanetNameDictionary = new Dictionary<string, int> ()
        {
            { "无要求", 0 },
            { "草原", 1 },
            { "黑石盐滩", 2 }, //黑石盐滩
            { "灰烬冻土", 3 },
            { "冰原冻土", 4 },
            { "火山灰", 5 },
            { "戈壁", 6 },
            { "猩红冰湖", 7 }, //猩红冰湖
            { "气态巨星", 8 },
            { "冰巨星", 9 },
            { "熔岩", 10 },
            { "樱花海", 11 }, //樱花海
            { "飓风石林", 12 }, //飓风石林
            { "干旱荒漠", 13 },
            { "贫瘠荒漠", 14 },
            { "水世界", 15 },
            { "海洋丛林", 16 },
            { "红石", 17 },
            { "热带草原", 18 },
            { "橙晶荒漠", 19 },
            { "极寒冻土", 20 },
            { "潘多拉沼泽", 21 },
        };
        private Dictionary<int, string> LogAllPlanetNameDictionary = new Dictionary<int, string> ()
        {
            { 0, "无要求" },
            { 1, "草原" },
            { 2, "黑石盐滩" }, //黑石盐滩
            { 3, "灰烬冻土" },
            { 4, "冰原冻土" },
            { 5, "火山灰" },
            { 6, "戈壁" },
            { 7, "猩红冰湖" }, //猩红冰湖
            { 8, "气态巨星" },
            { 9, "冰巨星" },
            { 10, "熔岩" },
            { 11, "樱花海" },  //樱花海
            { 12, "飓风石林" }, //飓风石林
            { 13, "干旱荒漠" },
            { 14, "贫瘠荒漠" },
            { 15, "水世界" },
            { 16, "海洋丛林" },
            { 17, "红石" },
            { 18,"热带草原" },
            { 19,"橙晶荒漠"},
            { 20,"极寒冻土"},
            { 21,"潘多拉沼泽"},
        };
    #endregion


        public MainWindow ()
        {
            InitializeComponent ();
            RandomTable.Init ();
            saveConditionPath = System.Environment.CurrentDirectory;
            //读dsp表
            PlanetModelingManager.Start ();
            for (int i = 32; i <= 64; i++)
            {
                SearchMinStarCount.Items.Add (i);
                SearchMaxStarCount.Items.Add (i);
            }

            if (!File.Exists (saveConditionPath + "\\config.json"))
                return;
            string text = File.ReadAllText (saveConditionPath + "\\config.json");
            if (string.IsNullOrEmpty (text))
                return;
            searchConfig                     = JsonConvert.DeserializeObject<SearchConfig> (text);
            searchTimes.Text                 = searchConfig.times.ToString ();
            searchOnceCount.Text             = searchConfig.onceCount.ToString ();
            MagCount.Text                    = searchConfig.magCount.ToString ();
            BluePlanetCount.Text             = searchConfig.bluePlanetCount.ToString ();
            OPlanetCount.Text                = searchConfig.oPlanetCount.ToString ();
            SearchMinStarCount.SelectedIndex = searchConfig.curMinSearchStarSelectIndex;
            SearchMaxStarCount.SelectedIndex = searchConfig.curMaxSearchSelectIndex;
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
                if (minConditions.gasSpeed > condition.gasSpeed)
                    minConditions.gasSpeed = condition.gasSpeed;
                if (minConditions.dysonLumino > condition.dysonLumino)
                    minConditions.dysonLumino = condition.dysonLumino;
                //距离是找一个比我大的，即找到所有必须条件里最大的
                if (minConditions.distanceToBirth < condition.distanceToBirth)
                    minConditions.distanceToBirth = condition.distanceToBirth;
                if (!condition.isInDsp)
                    minConditions.isInDsp = false;
                if (!condition.hasWater)
                    minConditions.hasWater = false;
                if (!condition.hasAcid)
                    minConditions.hasAcid = false;
                if (!condition.isInDsp2)
                    minConditions.isInDsp2 = false;
            }
        }

        void SearchCustomId ()
        {
            ResetMinCondition ();
            startTime = DateTime.Now;
            for (int i = 0; i < CustomSeedIdS.Count; i++)
            {
                curId     = CustomSeedIdS[i];
                var seedStarCount = CustomSeedStarCounts[i];
                SeedSearch (curId, seedStarCount);
            }
            var curTime = (DateTime.Now - startTime).TotalSeconds;
            this.Dispatcher.BeginInvoke ((System.Threading.ThreadStart)(() =>
            {
                searchlogContent  = "搜索自定义种子结束 ：用时：" + curTime + ";\n 已命中种子数量：" + curSeeds + ";最后命中的是：" + lastSeedId;
                SearchLog.Content = searchlogContent;
                process.Value     = 1;
            }));
        }

        void Search ()
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
                str              += "搜索到 ：" + curId + "；用时：" + curTime + ";\n 已命中种子数量：" + curSeeds + ";最后命中的是：" + lastSeedId;
                searchlogContent =  str;
                processValue     =  (j + 1) * 1.0f / times;
                this.Dispatcher.BeginInvoke ((System.Threading.ThreadStart)(() =>
                {
                    SearchLog.Content = searchlogContent;
                    process.Value     = processValue;
                }));
            }
        }

        private string GetEnumName (enumStarType type)
        {
            switch (type)
            {
                case enumStarType.None:
                    return "未知星系";
                case enumStarType.AStar:
                    return "A型恒星";
                case enumStarType.BlackHole:
                    return "黑洞";
                case enumStarType.BlueStar:
                    return "蓝巨星";
                case enumStarType.BornStar:
                    return "初始星系";
                case enumStarType.BStar:
                    return "B型恒星";
                case enumStarType.NeutronStar:
                    return "中子星";
                case enumStarType.NormalStar:
                    return "MKGF普通星系";
                case enumStarType.OStar:
                    return "O型恒星";
                case enumStarType.WhiteDwarf:
                    return "白矮星";
                case enumStarType.OtherGiantStar:
                    return "红黄或白巨星";
                default:
                    return "未知星系";
            }
        }

        private enumStarType GetStarType (StarData star)
        {
            switch (star.type)
            {
                case EStarType.MainSeqStar:
                    switch (star.spectr)
                    {
                        case ESpectrType.O:
                            return enumStarType.OStar;
                        case ESpectrType.B:
                            return enumStarType.BStar;
                        case ESpectrType.A:
                            return enumStarType.AStar;
                        default:
                            return enumStarType.NormalStar;
                    }
                case EStarType.BlackHole:
                    return enumStarType.BlackHole;
                case EStarType.NeutronStar:
                    return enumStarType.NeutronStar;
                case EStarType.WhiteDwarf:
                    return enumStarType.WhiteDwarf;
                case EStarType.GiantStar:
                    if (star.typeString == "蓝巨星")
                        return enumStarType.BlueStar;
                    else
                    {
                        return enumStarType.OtherGiantStar;
                    }
            }
            return enumStarType.None;
        }

        private SearchCondition CheckMagCount (StarData star, int i, GalaxyData galaxyData, SearchCondition condition, ref int curMagCount)
        {
            //先算磁石，具体星球数据
            var data = CheckPlanet (star, galaxyData, condition, ref curMagCount);
            if (data == null)
                return null;
            if (star.planetCount < condition.planetCount3)
                return null;
            if (star.dysonLumino < condition.dysonLumino)
                return null;
            double distanceToBirth = 0;
            if (i > 0)
                distanceToBirth = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
            //如果距离母星的距离比最大距离要求都远，说明不满足任意一个必须条件
            if (distanceToBirth > condition.distanceToBirth)
                return null;
            bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
            if (condition.isInDsp && !isInDsp)
                return null;
            bool isInDsp2 = star.planets.Length >= 2 && star.dysonRadius * 2 > star.planets[1].sunDistance;
            if (condition.isInDsp2 && !isInDsp2)
                return null;

            data.planetCount3    = star.planetCount;
            data.dysonLumino     = star.dysonLumino;
            data.distanceToBirth = distanceToBirth;
            data.isInDsp         = isInDsp;
            data.isInDsp2        = isInDsp2;
            data.starType        = GetStarType (star);
            return data;
        }

        private SearchCondition CheckPlanet (StarData star, GalaxyData galaxyData, SearchCondition condition, ref int curMagCount)
        {
            bool            hasWater = false;
            bool            hasAcid  = false;
            var             count1   = 0;
            var             count2   = 0;
            var             gas      = 0; //巨星数量
            var             gasSpeed = 0f;
            SearchCondition data     = new SearchCondition ();
            for (int j = 0; j < star.planets.Length; j++)
            {
                var planet = star.planets[j];
                DspData.PlanetCompute (galaxyData, star, planet);
                if (AllPlanetNameDictionary.TryGetValue (planet.typeString, out int index))
                {
                    data.planetNames[j] = index;
                    if (!data.planetNameCounts.ContainsKey (index))
                        data.planetNameCounts.Add (index, 1);
                    else
                        data.planetNameCounts[index]++;
                }
                if (planet.waterItemId == 1000) hasWater = true;
                if (planet.waterItemId == 1116) hasAcid  = true;
                if (planet.orbitAroundPlanet != null) count1++;
                if (planet.singularityString.Contains("潮汐锁定永昼永夜")) count2++;
                if (planet.type == EPlanetType.Gas)
                {
                    gas++;
                    if (planet.typeString == "气态巨星" && gasSpeed < planet.gasSpeeds[1])
                        gasSpeed = planet.gasSpeeds[1];
                }

                if (planet.type != EPlanetType.Gas && planet.veinSpotsSketch != null)
                {
                    for (int k = 0; k < data.resourceCount.Length; k++)
                        data.resourceCount[k] += planet.veinSpotsSketch[k + 1];
                    curMagCount += planet.veinSpotsSketch[14];
                }
            }
            if (gas >= 2) count1   -= gas - 1;
            if (count1 < 0) count1 =  0;
            if (count1 < condition.planetCount1) return null;
            if (count2 < condition.planetCount2) return null;
            if (!hasWater && condition.hasWater) return null;
            if (!hasAcid && condition.hasAcid) return null;
            if (gasSpeed < condition.gasSpeed) return null;

            data.planetCount1 = count1;
            data.planetCount2 = count2;
            data.hasWater     = hasWater;
            data.hasAcid      = hasAcid;
            data.gasSpeed     = gasSpeed;
            data.starName     = star.displayName;
            return data;
        }

        public SearchCondition Check (StarData star,               int     i, GalaxyData galaxyData, SearchCondition condition,
            ref int                            curBluePlanetCount, ref int curOPlanetCount)
        {
            //只有i = 62、63需要算磁石的数量
            bool isBluePlanet = star.typeString == "蓝巨星";
            if (isBluePlanet)
                curBluePlanetCount++;
            bool isOPlanetCount = star.typeString == "O型恒星";
            if (isOPlanetCount) curOPlanetCount++;
            if (star.planetCount < condition.planetCount3) return null;
            if (star.dysonLumino < condition.dysonLumino) return null;
            double toBirth = 0;
            if (i > 0)
                toBirth = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
            //如果距离母星的距离比最大距离要求都远，说明不满足任意一个必须条件
            if (toBirth > condition.distanceToBirth) return null;
            bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
            if (condition.isInDsp && !isInDsp) return null;
            bool isInDsp2 = star.planets.Length >= 2 && star.dysonRadius * 2 > star.planets[1].sunDistance;
            if (condition.isInDsp2 && !isInDsp2) return null;
            int magCount = 0;
            var data     = CheckPlanet (star, galaxyData, condition, ref magCount);
            if (data == null)
                return null;
            data.planetCount3    = star.planetCount;
            data.dysonLumino     = star.dysonLumino;
            data.distanceToBirth = toBirth;
            data.isInDsp         = isInDsp;
            data.isInDsp2        = isInDsp2;
            if (toBirth < 0.5f)
                data.starType = enumStarType.BornStar;
            else
                data.starType = GetStarType (star);
            return data;
        }

        public bool Check (SearchCondition shortStarData, GalaxyData galaxyData, SearchCondition condition)
        {
            if (condition.starType != enumStarType.None && shortStarData.starType != condition.starType)
                return false;
            if (shortStarData.planetCount3 < condition.planetCount3)
                return false;
            if (shortStarData.dysonLumino < condition.dysonLumino)
                return false;
            if (shortStarData.distanceToBirth > condition.distanceToBirth)
                return false;
            if (!shortStarData.isInDsp && condition.isInDsp)
                return false;
            if (shortStarData.planetCount1 < condition.planetCount1)
                return false;
            if (shortStarData.planetCount2 < condition.planetCount2)
                return false;
            if (shortStarData.gasSpeed < condition.gasSpeed)
                return false;
            if (!shortStarData.hasWater && condition.hasWater)
                return false;
            if (!shortStarData.hasAcid && condition.hasAcid)
                return false;
            foreach (var pair in condition.planetNameCounts)
            {
                if (!shortStarData.planetNameCounts.ContainsKey (pair.Key))
                    return false;
                if (shortStarData.planetNameCounts[pair.Key] < pair.Value)
                    return false;
            }
            for (int k = 0; k < shortStarData.resourceCount.Length; k++)
            {
                if (shortStarData.resourceCount[k] < condition.resourceCount[k])
                    return false;
            }

            return true;
        }

        public void SeedSearch (int id, int seedStarCount)
        {
            GameDesc gd = new GameDesc ();
            gd.SetForNewGame (id, seedStarCount);
            GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
            if (galaxyData == null)
                return;
            SeedSearch (galaxyData, seedStarCount);
        }

        public void SeedSearch (int id)
        {
            for (int i = curMinSearchStarCount; i <= curMaxSearchStarCount; i++)
            {
                GameDesc gd = new GameDesc ();
                gd.SetForNewGame (id, i);
                GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
                if (galaxyData == null)
                    return;
                SeedSearch (galaxyData, i);
            }
        }

        public void SeedSearch (GalaxyData galaxyData, int starCount)
        {
            Dictionary<int, List<SearchCondition>> necessaryShortStarDatas = new Dictionary<int, List<SearchCondition>> ();
            Dictionary<int, List<SearchCondition>> logShortStarDatas       = new Dictionary<int, List<SearchCondition>> ();
            int                                    curMagCount             = 0;
            int                                    curBluePlanetCount      = 0;
            int                                    curOPlanetCount         = 0;
            for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
            {
                var             star      = galaxyData.stars[i];
                SearchCondition shortData = null;
                //如果星系不是中子和黑洞，则不计算磁石数量，不用上来就计算矿物数量
                if (star.type != EStarType.BlackHole && star.type != EStarType.NeutronStar)
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
                    if (!Check (shortData, galaxyData, condition))
                        continue;
                    if (!necessaryShortStarDatas.ContainsKey (j))
                        necessaryShortStarDatas.Add (j, new List<SearchCondition> ());
                    var listShortData = necessaryShortStarDatas[j];
                    listShortData.Add (shortData);
                }
                //记录条件一样
                for (int j = 0, maxConditions = searchLogConditions.Count; j < maxConditions; j++)
                {
                    var condition = searchLogConditions[j];
                    if (!Check (shortData, galaxyData, condition))
                        continue;
                    if (!logShortStarDatas.ContainsKey (j))
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
            LogFile (curMagCount, curBluePlanetCount, curOPlanetCount, necessaryShortStarDatas, logShortStarDatas, starCount);
        }

        readonly string ConstSingleTitle = "星系名字,星区数量,亮度,行星,距离,星系类型,是否环内行星,星球类型,最大重氢速率,卫星总数,潮汐锁定,是否有水,是否有硫酸,铁矿脉,铜矿脉,硅矿脉,钛矿脉,石矿脉,煤矿脉,原油涌泉,可燃冰矿,金伯利矿,分形硅矿,有机晶体矿,光栅石矿,刺笋矿脉,单极磁矿\n";
        private  string CommonTitle      = "种子ID,星区数量,磁石总数,蓝巨星总数,0型恒星总数,星系数据1,星系数据2,星系数据3,星系数据4,星系数据5,星系数据6,星系数据7,星系数据8,星系数据9,星系数据10,星系数据11\n";

        private string FetchLogStr (Dictionary<int, List<SearchCondition>> ShortStarDatas, bool isNecessary = true)
        {
            var str = "";
            foreach (var item in ShortStarDatas)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    var data           = item.Value[i];
                    var planetNameInfo = ";";
                    foreach (var pair in data.planetNameCounts)
                    {
                        planetNameInfo += LogAllPlanetNameDictionary[pair.Key] + pair.Value + ";";
                    }
                    str += GetEnumName (data.starType) + data.starName + ";卫星" + data.planetCount1 + ";潮汐" + data.planetCount2 + planetNameInfo + ";最大重氢" + data.gasSpeed.ToString ("F3") + ";光度"
                         + data.dysonLumino.ToString ("F4") + ";初始距离"
                         + data.distanceToBirth.ToString ("F1") + ";戴森球" + (data.isInDsp ? "包括" : "不包括") + "1号行星;" + (data.isInDsp2 ? "包括" : "不包括") + "第二行星;"
                         + "磁石：" + data.resourceCount[13] +  (data.hasWater ? ";有" : ";没有") + "水;" + (data.hasAcid ? "有" : "没有") + "硫酸";
                    if (data.IsLogResource)
                    {
                        for (int k = 0; k < data.resourceCount.Length; k++)
                        {
                            var count = data.resourceCount[k];
                            var name  = LDB.veins.dataArray[k];
                            str += ";" + name + ":" + count + ";";
                        }
                    }
                    str += "(" + item.Key + "号" + (isNecessary ? "必须" : "仅记录") + "条件的第" + i + "个星系数据),";
                }
            }

            return str;
        }

        private void LogFile (int                  curMagCount,  int curBluePlanetCount, int curOPlanetCount, Dictionary<int, List<SearchCondition>> necessaryShortStar,
            Dictionary<int, List<SearchCondition>> logShortStar, int starCount)
        {
            var str = "";
            if (!logInit)
            {
                str     += CommonTitle;
                logInit =  true;
            }
            str += lastSeedId + "," + starCount + "," + curMagCount + "," + curBluePlanetCount + "," + curOPlanetCount + ",";
            str += FetchLogStr (necessaryShortStar);
            str += FetchLogStr (logShortStar, false);
            str += "\n";
            System.IO.File.AppendAllText (System.Environment.CurrentDirectory + "\\" + fileName + ".csv", str, Encoding.UTF8);
        }

        void SingleSearch ()
        {
            var singleTitle = ConstSingleTitle;
            for (int k = curMinSearchStarCount; k <= curMaxSearchStarCount; k++)
            {
                GameDesc gd = new GameDesc ();
                gd.SetForNewGame (startId, k);
                GalaxyData galaxyData = UniverseGen.CreateGalaxy (gd);
                if (galaxyData == null)
                    return;
                for (int i = 0, max = galaxyData.stars.Length; i < max; i++)
                {
                    var  star    = galaxyData.stars[i];
                    var  distanc = (float)(star.uPosition - galaxyData.stars[0].uPosition).magnitude / 2400000.0f;
                    bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;

                    singleTitle += star.name + "," + k + "," + star.dysonLumino.ToString ("F4") + "," + star.planetCount + "," +
                                   distanc.ToString ("F3") + "," + star.typeString + "," + (isInDsp ? "是" : "否") + ",";
                    int   cxsdcount      = 0;
                    int[] resource       = new int[LDB.veins.Length];
                    bool  hasWater       = false;
                    bool  hasAcid        = false;
                    var   planetCount1   = 0;
                    float gasSpeed       = 0;
                    var   planetNameInfo = "";
                    foreach (var planet in star.planets)
                    {
                        DspData.PlanetCompute (galaxyData, star, planet);
                        if (AllPlanetNameDictionary.TryGetValue (planet.typeString, out int index))
                        {
                            planetNameInfo += LogAllPlanetNameDictionary[index];
                        }
                        else
                        {
                            planetNameInfo += planet.typeString;
                        }
                        if (!string.IsNullOrEmpty (planet.singularityString))
                            planetNameInfo += "-" + planet.singularityString;
                        planetNameInfo += ";";
                        if (planet.waterItemId == 1000)
                            hasWater = true;
                        if (planet.waterItemId == 1116)
                            hasAcid = true;
                        if (planet.typeString == "气态巨星")
                        {
                            if (gasSpeed < planet.gasSpeeds[1])
                                gasSpeed = planet.gasSpeeds[1];
                        }
                        if (planet.orbitAroundPlanet != null)
                        {
                            planetCount1++;
                        }
                        if (planet.singularityString.Contains ("潮汐锁定"))
                        {
                            cxsdcount++;
                        }
                        for (int j = 0; j < LDB.veins.Length; j++)
                        {
                            var count = star.GetResourceSpots (j + 1);
                            resource[j] += count;
                        }
                    }
                    singleTitle += planetNameInfo + "," + gasSpeed + "," + planetCount1 + "," + cxsdcount +
                                   "," + (hasWater ? "是," : "否,") + (hasAcid ? "是," : "否,");

                    for (int j = 0; j < LDB.veins.Length; j++)
                    {
                        singleTitle += resource[j] + ",";
                    }
                    singleTitle += "\n";
                }
            }
            System.IO.File.WriteAllText (System.Environment.CurrentDirectory + "\\" + fileName + ".csv", singleTitle, Encoding.UTF8);
            this.Dispatcher.BeginInvoke ((System.Threading.ThreadStart)(() =>
            {
                SearchLog.Content = "成功写入单个种子 ：" + startId + "的所有信息";
            }));
        }
    }
}