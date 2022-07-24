using System;
using System.Collections.Generic;
using UnityEngine;

namespace DspFindSeed
{
    public class ProtoSet<T> : ProtoTable where T : Proto
    {
        public  T[]                  dataArray;
        private Dictionary<int, int> dataIndices;

        public override void Init(int length) => this.dataArray = new T[length];

        public override Proto this[int index]
        {
            get => (Proto) this.dataArray[index];
            set => this.dataArray[index] = value as T;
        }

        public override int Length => this.dataArray.Length;

        public virtual void OnAfterDeserialize()
        {
            this.dataIndices = new Dictionary<int, int>();
            for (int index = 0; index < this.dataArray.Length; ++index)
            {
                this.dataArray[index].name                 = this.dataArray[index].Name;
                this.dataArray[index].sid                  = this.dataArray[index].SID;
                this.dataIndices[this.dataArray[index].ID] = index;
            }
        }

        public T Select(int id) => this.dataIndices.ContainsKey(id) ? this.dataArray[this.dataIndices[id]] : default (T);

        public bool Exist(int id) => this.dataIndices.ContainsKey(id);
    }
    
    public class ThemeProtoSet : ProtoSet<ThemeProto>
    {
    }
    public class ThemeProto : Proto
    {
        public static int[]            themeIds;
        public        string           DisplayName;
        public        EPlanetType      PlanetType;
        public        string           MaterialPath;
        public        float            Temperature;
        public        EThemeDistribute Distribute;
        public        int[]            Algos;
        public        Vector2          ModX;
        public        Vector2          ModY;
        public        int[]            Vegetables0;
        public        int[]            Vegetables1;
        public        int[]            Vegetables2;
        public        int[]            Vegetables3;
        public        int[]            Vegetables4;
        public        int[]            Vegetables5;
        public        int[]            VeinSpot;
        public        float[]          VeinCount;
        public        float[]          VeinOpacity;
        public        int[]            RareVeins;
        public        float[]          RareSettings;
        public        int[]            GasItems;
        public        float[]          GasSpeeds;
        public        bool             UseHeightForBuild;
        public        float            Wind;
        public        float            IonHeight;
        public        float            WaterHeight;
        public        int              WaterItemId;
        public        int[]            Musics;
        public        string           SFXPath;
        public        float            SFXVolume;
        public        float            CullingRadius;
    
        public string displayName { get; set; }
      }
    
    public class VeinProtoSet : ProtoSet<VeinProto>
    {
    }
    
    public class VeinProto : Proto
    {
        public string IconPath;
        public string Description;
        public int    ModelIndex;
        public int    ModelCount;
        public float  CircleRadius;
        public int    MiningItem;
        public int    MiningTime;
        public int    MiningAudio;
        public int    MiningEffect;
        public int    MinerBaseModelIndex;
        public int    MinerCircleModelIndex;

        public string description { get; set; }
    }
    
    public class ItemProtoSet : ProtoSet<ItemProto>
    {
    }
    
    public class ItemProto : Proto
    {
        public EItemType Type;
        public string    MiningFrom;
        public string    ProduceFrom;
        public int       StackSize;
        public bool      IsFluid;
        public bool      IsEntity;
        public bool      CanBuild;
        public bool      BuildInGas;
        public string    IconPath;
        public int       ModelIndex;
        public int       ModelCount;
        public int       HpMax;
        public int       Ability;
        public long      HeatValue;
        public long      Potential;
        public float     ReactorInc;
        public int       FuelType;
        public int       BuildIndex;
        public int       BuildMode;
        public int       GridIndex;
        public int       UnlockKey;
        public int       PreTechOverride;
        public int[]     DescFields;
        public string    Description;
        [NonSerialized]
        public bool isRaw;
        [NonSerialized]
        public int handcraftProductCount;
        [NonSerialized]
        public int maincraftProductCount;
        [NonSerialized]
        public bool missingTech;
        public const  int     kMaxProtoId  = 12000;
        public const  int     kWaterId     = 1000;
        public const  int     kDroneId     = 5001;
        public const  int     kShipId      = 5002;
        public const  int     kRocketId    = 1503;
        public const  int     kWarperId    = 1210;
        public const  int     kVeinMinerId = 2301;
        public const  int     kOilMinerId  = 2307;
        public static int     stationCollectorId;
        public static int[][] fuelNeeds = new int[64][];
        public static int[]   fluids;
        public static int[]   itemIds;

        public string miningFrom { get; set; }

        public string produceFrom { get; set; }

        public string description { get; set; }

        public int index { get; set; }
    }
}