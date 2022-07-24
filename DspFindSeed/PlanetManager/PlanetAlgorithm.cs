using System;
using System.Collections.Generic;
using UnityEngine;

namespace DspFindSeed
{
public abstract class PlanetAlgorithm
{
  protected int seed;
  protected PlanetData planet;
  private Vector3[] veinVectors = new Vector3[512];
  private EVeinType[] veinVectorTypes = new EVeinType[512];
  private int veinVectorCount;
  private List<Vector2> tmp_vecs = new List<Vector2>(100);

  public void Reset(int _seed, PlanetData _planet)
  {
    this.seed = _seed;
    this.planet = _planet;
  }

  // public static Quaternion FromeToRotation (Vector3 u, Vector3 v)
  // {
  //   float lTheta = Vector3.Dot (u.normalized, v.normalized);
  //   if (lTheta >= 1f)
  //   {
  //     return Quaternion.identity;
  //   }
  //   else if (lTheta < -1f)
  //   {
  //     Vector3 lSimpleAxis = Vector3.Cross (u, Vector3.right);
  //     if (lSimpleAxis.sqrMagnitude == 0f)
  //     {
  //       lSimpleAxis = Vector3.Cross (u, Vector3.up);
  //     }
  //     return Quaternion.AngleAxis (180f, lSimpleAxis);
  //   }
  //   float   lRadians = Mathf.Acos (lTheta);
  //   Vector3 lAxis    = Vector3.Cross (u, v);
  //   return Quaternion.AngleAxis (lRadians * Mathf.Rad2Deg, lAxis);
  // }
  //
   public static Quaternion FromToRotation(Vector3 fromDir, Vector3 toDir)
    {
        if (fromDir == Vector3.zero || toDir == Vector3.zero) return Quaternion.identity;
        Quaternion q;
        //最大幅值归一化
        //fromDir
        float max = Mathf.Abs(fromDir.x);
        max = max > Mathf.Abs(fromDir.y) ? max : Mathf.Abs(fromDir.y);
        max = (max > Mathf.Abs(fromDir.z)) ? max : Mathf.Abs(fromDir.z);
        fromDir = fromDir / max;

        //toDir
        max = Mathf.Abs(toDir.x);
        max = (max > Mathf.Abs(toDir.y)) ? max : Mathf.Abs(toDir.y);
        max = (max > Mathf.Abs(toDir.z)) ? max : Mathf.Abs(toDir.z);
        toDir = toDir / max;

        //极小数归零
        //最小阈值
        float miniThreshold = 0.001f;
        fromDir.x = Mathf.Abs(fromDir.x) <= miniThreshold ? 0 : fromDir.x;
        fromDir.y = Mathf.Abs(fromDir.y) <= miniThreshold ? 0 : fromDir.y;
        fromDir.z = Mathf.Abs(fromDir.z) <= miniThreshold ? 0 : fromDir.z;
        toDir.x = Mathf.Abs(toDir.x) <= miniThreshold ? 0 : toDir.x;
        toDir.y = Mathf.Abs(toDir.y) <= miniThreshold ?0: toDir.y;
        toDir.z = Mathf.Abs(toDir.z) <= miniThreshold ? 0 : toDir.z;

        Vector3 mid = (fromDir.normalized + toDir.normalized).normalized;
        if (mid == Vector3.zero)
        {
            //相反的两个方向,和向量为0，还原Unity，分为八种情况
            //某一向量为零返回四元数基值，已在开头判定
            // 仅有一个向量不为0,分为仅有x y z 向量不为0三种情况
            //X
            if (fromDir.x != 0 && fromDir.y == 0 && fromDir.z == 0)
                return new Quaternion(0, 1, 0, 0);
            //Y
            else if (fromDir.x == 0 && fromDir.y != 0 && fromDir.z == 0)
                return new Quaternion(1, 0, 0, 0);
            //Z 
            else if (fromDir.x == 0 && fromDir.y == 0 && fromDir.z != 0)
                return new Quaternion(1, 0, 0, 0);

            // 仅有一个向量为0,分为仅有x y z 向量为0三种情况
            //X
            else if (fromDir.x == 0 && fromDir.y != 0 && fromDir.z != 0)
                return new Quaternion(1, 0, 0, 0);
            //Y
            else if (fromDir.x != 0 && fromDir.y == 0 && fromDir.z != 0)
            {
                float X = toDir.normalized.z;
                float Z = fromDir.normalized.x;

                //正负判定
                if (X + Z < 0 || (X + Z == 0 && X < 0))
                    return new Quaternion(-X, 0, -Z, 0);
                else
                    return new Quaternion(X, 0, Z, 0);
            }
            //Z 
            else if (fromDir.x != 0 && fromDir.y != 0 && fromDir.z == 0)
            {
                float X = toDir.normalized.y;
                float Y = fromDir.normalized.x;

                //正负判定
                if (X + Y < 0 || (X + Y == 0 && X < 0))
                    return new Quaternion(-X, -Y, 0, 0);
                else
                    return new Quaternion(X, Y, 0, 0);
            }
            else
            {
                //三个点都不为0
                mid.y = fromDir.z;
                mid.z = toDir.y;
                mid = mid.normalized;
            }
        }
        q = new Quaternion(-toDir.normalized.x, -toDir.normalized.y, -toDir.normalized.z, 0) * new Quaternion(mid.normalized.x, mid.normalized.y, mid.normalized.z, 0);
        return q;
    }
  
  public virtual void CalcWaterPercent()
  {
    if (this.planet.type == EPlanetType.Gas)
      this.planet.windStrength = 0.0f;
    PlanetAlgorithm.CalcLandPercent(this.planet);
  }
  
  
  public static void CalcLandPercent(PlanetData _planet)
  {
    if (_planet == null)
      return;
    PlanetRawData data = _planet.data;
    if (data == null)
      return;
    int      stride     = data.stride;
    int      num1       = stride / 2;
    int      dataLength = data.dataLength;
    ushort[] heightData = data.heightData;
    if (heightData == null)
      return;
    float num2 = (float) ((double) _planet.radius * 100.0 - 20.0);
    if (_planet.type == EPlanetType.Gas)
    {
      _planet.landPercent = 0.0f;
    }
    else
    {
      int num3 = 0;
      int num4 = 0;
      for (int index = 0; index < dataLength; ++index)
      {
        int num5 = index % stride;
        int num6 = index / stride;
        if (num5 > num1)
          --num5;
        if (num6 > num1)
          --num6;
        if ((num5 & 1) == 1 && (num6 & 1) == 1)
        {
          if ((double) heightData[index] >= (double) num2)
            ++num4;
          else if (data.GetModLevel(index) == 3)
            ++num4;
          ++num3;
        }
      }
      _planet.landPercent = num3 > 0 ? (float) num4 / (float) num3 : 0.0f;
    }
  }
  
  public float QueryHeight(PlanetRawData planetRawData, Vector3 vpos)
  {
    vpos.Normalize();
    int   index1 = planetRawData.indexMap[planetRawData.PositionHash(vpos)];
    float num1   = (float) (3.14159274101257 / (double) (planetRawData.precision * 2) * 1.20000004768372);
    float num2   = num1 * num1;
    float num3   = 0.0f;
    float num4   = 0.0f;
    int   stride = planetRawData.stride;
    for (int index2 = -1; index2 <= 3; ++index2)
    {
      for (int index3 = -1; index3 <= 3; ++index3)
      {
        int index4 = index1 + index2 + index3 * stride;
        if ((long) (uint) index4 < (long) planetRawData.dataLength)
        {
          float sqrMagnitude = (planetRawData.vertices[index4] - vpos).sqrMagnitude;
          if ((double) sqrMagnitude <= (double) num2)
          {
            float num5 = (float) (1.0 - (double) Mathf.Sqrt(sqrMagnitude) / (double) num1);
            float num6 = (float) planetRawData.heightData[index4];
            num3 += num5;
            num4 += num6 * num5;
          }
        }
      }
    }
    if ((double) num3 != 0.0)
      return (float) ((double) num4 / (double) num3 * 0.00999999977648258);
    return (float) planetRawData.heightData[0] * 0.01f;
  }

  public abstract void GenerateTerrain(double modX, double modY);
  public virtual void GenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      bool flag1 = this.planet.galaxy.birthPlanetId == this.planet.id;
      // if (flag1)
      //   this.planet.GenBirthPoints(data, _birthSeed);
      float f                = this.planet.star.resourceCoef;
      bool  infiniteResource = false;
      bool  isRareResource   = false;
      if (flag1)
        f *= 0.6666667f;
      else if (isRareResource)
      {
        if ((double) f > 1.0)
          f = Mathf.Pow(f, 0.8f);
        f *= 0.7f;
      }
      float num7 = 1f * 1.1f;
      Array.Clear((Array) this.veinVectors, 0, this.veinVectors.Length);
      Array.Clear((Array) this.veinVectorTypes, 0, this.veinVectorTypes.Length);
      this.veinVectorCount = 0;
      Vector3 birthPoint;
      if (flag1)
      {
        birthPoint = this.planet.birthPoint;
        birthPoint.Normalize();
        birthPoint *= 0.75f;
      }
      else
      {
        birthPoint.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.y = (float) dotNet35Random2.NextDouble() - 0.5f;
        birthPoint.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.Normalize();
        birthPoint *= (float) (dotNet35Random2.NextDouble() * 0.4 + 0.2);
      }
      if (flag1)
      {
        this.veinVectorTypes[0] = EVeinType.Iron;
        this.veinVectors[0] = this.planet.birthResourcePoint0;
        this.veinVectorTypes[1] = EVeinType.Copper;
        this.veinVectors[1] = this.planet.birthResourcePoint1;
        this.veinVectorCount = 2;
      }
      for (int index3 = 1; index3 < 15 && this.veinVectorCount < this.veinVectors.Length; ++index3)
      {
        EVeinType eveinType = (EVeinType) index3;
        int num8 = destinationArray1[index3];
        if (num8 > 1)
          num8 += dotNet35Random2.Next(-1, 2);
        for (int index4 = 0; index4 < num8; ++index4)
        {
          int num9 = 0;
          Vector3 zero = Vector3.zero;
          bool flag2 = false;
          while (num9++ < 200)
          {
            zero.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.y = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            if (eveinType != EVeinType.Oil)
              zero += birthPoint;
            zero.Normalize();
            float num10 = QueryHeight(data, zero);
            if ((double) num10 >= (double) this.planet.radius && (eveinType != EVeinType.Oil || (double) num10 >= (double) this.planet.radius + 0.5))
            {
              bool flag3 = false;
              float num11 = eveinType == EVeinType.Oil ? 100f : 196f;
              for (int index5 = 0; index5 < this.veinVectorCount; ++index5)
              {
                if ((double) (this.veinVectors[index5] - zero).sqrMagnitude < (double) num1 * (double) num1 * (double) num11)
                {
                  flag3 = true;
                  break;
                }
              }
              if (!flag3)
              {
                flag2 = true;
                break;
              }
            }
          }
          if (flag2)
          {
            this.veinVectors[this.veinVectorCount] = zero;
            this.veinVectorTypes[this.veinVectorCount] = eveinType;
            ++this.veinVectorCount;
            if (this.veinVectorCount == this.veinVectors.Length)
              break;
          }
        }
      }
      data.veinCursor = 1;
      this.tmp_vecs.Clear();
      VeinData vein = new VeinData();
      for (int index6 = 0; index6 < this.veinVectorCount; ++index6)
      {
        this.tmp_vecs.Clear();
        Vector3    normalized     = this.veinVectors[index6].normalized;
        EVeinType  veinVectorType = this.veinVectorTypes[index6];
        int        index7         = (int) veinVectorType;
        Quaternion rotation       = FromToRotation(Vector3.up, normalized);
        Vector3    vector3_1      = rotation * Vector3.right;
        Vector3    vector3_2      = rotation * Vector3.forward;
        this.tmp_vecs.Add(Vector2.zero);
        int num12 = Mathf.RoundToInt(destinationArray2[index7] * (float) dotNet35Random2.Next(20, 25));
        if (veinVectorType == EVeinType.Oil)
          num12 = 1;
        float num13 = destinationArray3[index7];
        if (flag1 && index6 < 2)
        {
          num12 = 6;
          num13 = 0.2f;
        }
        int num14 = 0;
        while (num14++ < 20)
        {
          int count = this.tmp_vecs.Count;
          for (int index8 = 0; index8 < count && this.tmp_vecs.Count < num12; ++index8)
          {
            Vector2 vector2_1 = this.tmp_vecs[index8];
            if ((double) vector2_1.sqrMagnitude <= 36.0)
            {
              double num15 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
              Vector2 vector2_2 = new Vector2((float) Math.Cos(num15), (float) Math.Sin(num15));
              vector2_2 += this.tmp_vecs[index8] * 0.2f;
              vector2_2.Normalize();
              Vector2 vector2_3 = this.tmp_vecs[index8] + vector2_2;
              bool flag4 = false;
              for (int index9 = 0; index9 < this.tmp_vecs.Count; ++index9)
              {
                vector2_1 = this.tmp_vecs[index9] - vector2_3;
                if ((double) vector2_1.sqrMagnitude < 0.850000023841858)
                {
                  flag4 = true;
                  break;
                }
              }
              if (!flag4)
                this.tmp_vecs.Add(vector2_3);
            }
          }
          if (this.tmp_vecs.Count >= num12)
            break;
        }
        float num16 = f;
        if (veinVectorType == EVeinType.Oil)
          num16 = Mathf.Pow(f, 0.5f);
        int num17 = Mathf.RoundToInt(num13 * 100000f * num16);
        if (num17 < 20)
          num17 = 20;
        int num18 = num17 < 16000 ? Mathf.FloorToInt((float) num17 * (15f / 16f)) : 15000;
        int minValue = num17 - num18;
        int maxValue = num17 + num18 + 1;
        for (int index10 = 0; index10 < this.tmp_vecs.Count; ++index10)
        {
          Vector3 vector3_3 = (tmp_vecs[index10].x * vector3_1 + tmp_vecs[index10].y * vector3_2) * num1;
          vein.type = veinVectorType;
          vein.groupIndex = (short) (index6 + 1);
          vein.modelIndex = (short) dotNet35Random2.Next(veinModelIndexs[index7], veinModelIndexs[index7] + veinModelCounts[index7]);
          vein.amount = Mathf.RoundToInt((float) dotNet35Random2.Next(minValue, maxValue) * num7);
          if (veinVectorType != EVeinType.Oil)
            vein.amount = Mathf.RoundToInt((float) vein.amount * 1);
          if (vein.amount < 1)
            vein.amount = 1;
          if (infiniteResource && vein.type != EVeinType.Oil)
            vein.amount = 1000000000;
          vein.productId = veinProducts[index7];
          vein.pos       = normalized + vector3_3;
          if (vein.type == EVeinType.Oil)
            vein.pos = this.planet.aux.RawSnap(vein.pos);
          vein.minerCount = 0;
          float num19 = QueryHeight(data, vein.pos);
          data.EraseVegetableAtPoint(vein.pos);
          vein.pos = vein.pos.normalized * num19;
          if (this.planet.waterItemId == 0 || (double) num19 >= (double) this.planet.radius)
            data.AddVeinData(vein);
        }
      }
      this.tmp_vecs.Clear();
    }
  }

  public virtual int[] MyGenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return null;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      return destinationArray1;
    }
  }
}

public class PlanetAlgorithm0 : PlanetAlgorithm
{
  public override void GenerateTerrain(double modX, double modY)
  {
  }

}
public class PlanetAlgorithm1 : PlanetAlgorithm
{
  public override void GenerateTerrain(double modX, double modY)
  {
    double num1 = 0.01;
    double num2 = 0.012;
    double num3 = 0.01;
    double num4 = 3.0;
    double num5 = -0.2;
    double num6 = 0.9;
    double num7 = 0.5;
    double num8 = 2.5;
    double num9 = 0.3;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    PlanetRawData data = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num10 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num11 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num12 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num13 = simplexNoise1.Noise3DFBM(num10 * num1, num11 * num2, num12 * num3, 6) * num4 + num5;
      double num14 = simplexNoise2.Noise3DFBM(num10 * (1.0 / 400.0), num11 * (1.0 / 400.0), num12 * (1.0 / 400.0), 3) * num4 * num6 + num7;
      double num15 = num14 > 0.0 ? num14 * 0.5 : num14;
      double num16 = num13 + num15;
      double f = num16 > 0.0 ? num16 * 0.5 : num16 * 1.6;
      double num17 = f > 0.0 ? Maths.Levelize3(f, 0.7) : Maths.Levelize2(f, 0.5);
      double num18 = simplexNoise2.Noise3DFBM(num10 * num1 * 2.5, num11 * num2 * 8.0, num12 * num3 * 2.5, 2) * 0.6 - 0.3;
      double num19 = f * num8 + num18 + num9;
      double num20 = num19 < 1.0 ? num19 : (num19 - 1.0) * 0.8 + 1.0;
      double num21 = num17;
      double num22 = num20;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num21 + 0.2) * 100.0);
      data.biomoData[index] = (byte) Mathf.Clamp((float) (num22 * 100.0), 0.0f, 200f);
    }
  }

  private static float diff(float a, float b) => (double) a <= (double) b ? b - a : a - b;
}
public class PlanetAlgorithm2 : PlanetAlgorithm
{
  public override void GenerateTerrain(double modX, double modY)
  {
    modX = (3.0 - modX - modX) * modX * modX;
    double         num1           = 0.0035;
    double         num2           = 0.025 * modX + 0.0035 * (1.0 - modX);
    double         num3           = 0.0035;
    double         num4           = 3.0;
    double         num5           = 1.0 + 1.3 * modY;
    double         num6           = num1 * num5;
    double         num7           = num2 * num5;
    double         num8           = num3 * num5;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int            seed1          = dotNet35Random.Next();
    int            seed2          = dotNet35Random.Next();
    SimplexNoise   simplexNoise1  = new SimplexNoise(seed1);
    SimplexNoise   simplexNoise2  = new SimplexNoise(seed2);
    PlanetRawData  data           = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num9  = (double) data.vertices[index].x * (double) this.planet.radius;
      double num10 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num11 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num12 = simplexNoise1.Noise3DFBM(num9 * num6, num10 * num7, num11 * num8, 6, 0.45, 1.8);
      double num14 = num4;
      double num15 = 0.6 / (Math.Abs(num12 * num14 + num4 * 0.4) + 0.6) - 0.25;
      double num16 = num15 < 0.0 ? num15 * 0.3 : num15;
      double num20 = num16;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num20 + 0.1) * 100.0);
    }
  }
}

public class PlanetAlgorithm3 : PlanetAlgorithm
{
  private double Lerp (double a, double b, double t) => a + (b - a) * t;

  public override void GenerateTerrain (double modX, double modY)
  {
    double         num1           = 0.007;
    double         num2           = 0.007;
    double         num3           = 0.007;
    DotNet35Random dotNet35Random = new DotNet35Random (this.planet.seed);
    int            seed1          = dotNet35Random.Next ();
    int            seed2          = dotNet35Random.Next ();
    SimplexNoise   simplexNoise1  = new SimplexNoise (seed1);
    SimplexNoise   simplexNoise2  = new SimplexNoise (seed2);
    PlanetRawData  data           = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num4  = (double) data.vertices[index].x * (double) this.planet.radius;
      double num5  = (double) data.vertices[index].y * (double) this.planet.radius;
      double num6  = (double) data.vertices[index].z * (double) this.planet.radius;
      double num7  = num4 + Math.Sin (num5 * 0.15) * 3.0;
      double num8  = num5 + Math.Sin (num6 * 0.15) * 3.0;
      double num9  = num6 + Math.Sin (num7 * 0.15) * 3.0;
      double num10 = simplexNoise1.Noise3DFBM (num7 * num1 * 1.0, num8 * num2 * 1.1, num9 * num3 * 1.0, 6, deltaWLen: 1.8);
      double num11 = simplexNoise2.Noise3DFBM (num7 * num1 * 1.3 + 0.5, num8 * num2 * 2.8 + 0.2, num9 * num3 * 1.3 + 0.7, 3) * 2.0;
      double a     = simplexNoise2.Noise3DFBM (num7 * num1 * 6.0, num8 * num2 * 12.0, num9 * num3 * 6.0, 2) * 2.0;
      double num12 = this.Lerp (a, a * 0.1, modX);
      double num13 = simplexNoise2.Noise3DFBM (num7 * num1 * 0.8, num8 * num2 * 0.8, num9 * num3 * 0.8, 2) * 2.0;
      double f     = num10 * 2.0 + 0.92 + (double) Mathf.Clamp01 ((float) (num11 * (double) Mathf.Abs ((float) num13 + 0.5f) - 0.35) * 1f);
      if (f < 0.0)
        f *= 2.0;
      double t = Maths.Levelize2 (f);
      if (t > 0.0)
      {
        double num14 = Maths.Levelize2 (f);
        t = this.Lerp (Maths.Levelize4 (num14), num14, modX);
      }
      double b = t > 0.0 ? (t > 1.0 ? (t > 2.0 ? (double) Mathf.Lerp (1.2f, 2f, (float) t - 2f) + num12 * 0.12 : (double) Mathf.Lerp (0.3f, 1.2f, (float) t - 1f) + num12 * 0.12) : (double) Mathf.Lerp (0.0f, 0.3f, (float) t) + num12 * 0.1) : (double) Mathf.Lerp (-1f, 0.0f, (float) t + 1f);
      double num15 = this.Lerp (
        t > 0.0 ? (t > 1.0 ? (t > 2.0 ? (double) Mathf.Lerp (1.4f, 2.7f, (float) t - 2f) + num12 * 0.12 : (double) Mathf.Lerp (0.3f, 1.4f, (float) t - 1f) + num12 * 0.12) : (double) Mathf.Lerp (0.0f, 0.3f, (float) t) + num12 * 0.1) : (double) Mathf.Lerp (-4f, 0.0f, (float) t + 1f), b, modX);
      double num16 = num15;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num16 + 0.2) * 100.0);
    }
  }
}

public class PlanetAlgorithm4 : PlanetAlgorithm
{
  private Vector4[] circles = new Vector4[80];
  private double[]  heights = new double[80];
    public override void GenerateTerrain(double modX, double modY)
  {
    double num1 = 0.007;
    double num2 = 0.007;
    double num3 = 0.007;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    int seed3 = dotNet35Random.Next();
    for (int index = 0; index < 80; ++index)
    {
      VectorLF3 vectorLf3 = RandomTable.SphericNormal(ref seed3, 1.0);
      Vector4 vector4 = new Vector4((float) vectorLf3.x, (float) vectorLf3.y, (float) vectorLf3.z);
      vector4.Normalize();
      vector4 *= this.planet.radius;
      vector4.w = (float) (vectorLf3.magnitude * 8.0 + 8.0);
      vector4.w *= vector4.w;
      this.circles[index] = vector4;
      this.heights[index] = dotNet35Random.NextDouble() * 0.4 + 0.200000002980232;
    }
    PlanetRawData data = this.planet.data;
    for (int index1 = 0; index1 < data.dataLength; ++index1)
    {
      double num4 = (double) data.vertices[index1].x * (double) this.planet.radius;
      double num5 = (double) data.vertices[index1].y * (double) this.planet.radius;
      double num6 = (double) data.vertices[index1].z * (double) this.planet.radius;
      double num7 = simplexNoise1.Noise3DFBM(num4 * num1, num5 * num2, num6 * num3, 4, 0.45, 1.8);
      double num8 = simplexNoise2.Noise3DFBM(num4 * num1 * 5.0, num5 * num2 * 5.0, num6 * num3 * 5.0, 4);
      double num9 = num7 * 1.5;
      double num10 = num8 * 0.2;
      double num11 = num9 * 0.08 + num10 * 2.0;
      double num12 = 0.0;
      for (int index2 = 0; index2 < 80; ++index2)
      {
        double num13 = (double) this.circles[index2].x - num4;
        double num14 = (double) this.circles[index2].y - num5;
        double num15 = (double) this.circles[index2].z - num6;
        double num16 = num13 * num13 + num14 * num14 + num15 * num15;
        if (num16 <= (double) this.circles[index2].w)
        {
          double num17 = num16 / (double) this.circles[index2].w + num10 * 1.2;
          if (num17 < 0.0)
            num17 = 0.0;
          double num18 = num17 * num17;
          double num19 = -15.0 * (num18 * num17) + 131.0 / 6.0 * num18 - 113.0 / 15.0 * num17 + 0.7 + num10;
          if (num19 < 0.0)
            num19 = 0.0;
          double num20 = num19 * num19 * this.heights[index2];
          num12 = num12 > num20 ? num12 : num20;
        }
      }
      double num21 = num12 + num11 + 0.2;
      double num22 = num9 * 2.0 + 0.8;
      double num23 = num22 > 2.0 ? 2.0 : (num22 < 0.0 ? 0.0 : num22);
      double num24 = num23 + (num23 > 1.5 ? -num12 : num12) * 0.5 + num8 * 0.63;
      data.heightData[index1] = (ushort) (((double) this.planet.radius + num21 + 0.1) * 100.0);
      data.biomoData[index1] = (byte) Mathf.Clamp((float) (num24 * 100.0), 0.0f, 200f);
    }
  }
}

public class PlanetAlgorithm5 : PlanetAlgorithm
{
   public override void GenerateTerrain(double modX, double modY)
  {
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    PlanetRawData data = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num1 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num2 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num3 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num4 = 0.0;
      double num5 = Maths.Levelize(num1 * 0.007);
      double num6 = Maths.Levelize(num2 * 0.007);
      double num7 = Maths.Levelize(num3 * 0.007);
      double xin = num5 + simplexNoise1.Noise(num1 * 0.05, num2 * 0.05, num3 * 0.05) * 0.04;
      double yin = num6 + simplexNoise1.Noise(num2 * 0.05, num3 * 0.05, num1 * 0.05) * 0.04;
      double zin = num7 + simplexNoise1.Noise(num3 * 0.05, num1 * 0.05, num2 * 0.05) * 0.04;
      double num8 = Math.Abs(simplexNoise2.Noise(xin, yin, zin));
      double num9 = (0.16 - num8) * 10.0;
      double num10 = num9 > 0.0 ? (num9 > 1.0 ? 1.0 : num9) : 0.0;
      double num11 = num10 * num10;
      double num12 = (simplexNoise1.Noise3DFBM(num2 * 0.005, num3 * 0.005, num1 * 0.005, 4) + 0.22) * 5.0;
      double num13 = num12 > 0.0 ? (num12 > 1.0 ? 1.0 : num12) : 0.0;
      double num14 = Math.Abs(simplexNoise2.Noise3DFBM(xin * 1.5, yin * 1.5, zin * 1.5, 2));
      double num15 = simplexNoise1.Noise3DFBM(num3 * 0.06, num2 * 0.06, num1 * 0.06, 2);
      double num16 = num4 - num11 * 1.2 * num13;
      if (num16 >= 0.0)
        num16 += num8 * 0.25 + num14 * 0.6;
      double num17 = num16 - 0.1;
      double num18 = num8 * 2.1;
      if (num18 < 0.0)
        num18 *= 5.0;
      double num19 = num18 > -1.0 ? (num18 > 2.0 ? 2.0 : num18) : -1.0;
      double num20 = num19 + num15 * 0.6 * num19;
      double num21 = -0.3 - num17;
      if (num21 > 0.0)
      {
        double num22 = simplexNoise2.Noise(num1 * 0.16, num2 * 0.16, num3 * 0.16) - 1.0;
        double num23 = num21 > 1.0 ? 1.0 : num21;
        double num24 = (3.0 - num23 - num23) * num23 * num23;
        num17 = -0.3 - num24 * 3.70000004768372 + num24 * num24 * num24 * num24 * num22 * 0.5;
      }
      data.heightData[index] = (ushort) (((double) this.planet.radius + num17 + 0.2) * 100.0);
      data.biomoData[index] = (byte) Mathf.Clamp((float) (num20 * 100.0), 0.0f, 200f);
    }
  }
}

public class PlanetAlgorithm6 : PlanetAlgorithm
{
    public override void GenerateTerrain(double modX, double modY)
  {
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    PlanetRawData data = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num1 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num2 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num3 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num4 = 0.0;
      double num5 = Maths.Levelize(num1 * 0.007);
      double num6 = Maths.Levelize(num2 * 0.007);
      double num7 = Maths.Levelize(num3 * 0.007);
      double xin = num5 + simplexNoise1.Noise(num1 * 0.05, num2 * 0.05, num3 * 0.05) * 0.04;
      double yin = num6 + simplexNoise1.Noise(num2 * 0.05, num3 * 0.05, num1 * 0.05) * 0.04;
      double zin = num7 + simplexNoise1.Noise(num3 * 0.05, num1 * 0.05, num2 * 0.05) * 0.04;
      double num8 = Math.Abs(simplexNoise2.Noise(xin, yin, zin));
      double num9 = (0.16 - num8) * 10.0;
      double num10 = num9 > 0.0 ? (num9 > 1.0 ? 1.0 : num9) : 0.0;
      double num11 = num10 * num10;
      double num12 = (simplexNoise1.Noise3DFBM(num2 * 0.005, num3 * 0.005, num1 * 0.005, 4) + 0.22) * 5.0;
      double num13 = num12 > 0.0 ? (num12 > 1.0 ? 1.0 : num12) : 0.0;
      double num14 = Math.Abs(simplexNoise2.Noise3DFBM(xin * 1.5, yin * 1.5, zin * 1.5, 2));
      double num15 = num4 - num11 * 1.2 * num13;
      if (num15 >= 0.0)
        num15 += num8 * 0.25 + num14 * 0.6;
      double num16 = num15 - 0.1;
      double num17 = -0.3 - num16;
      if (num17 > 0.0)
      {
        double num18 = num17 > 1.0 ? 1.0 : num17;
        num16 = -0.3 - (3.0 - num18 - num18) * num18 * num18 * 3.70000004768372;
      }
      double num19 = Maths.Levelize(num11 > 0.300000011920929 ? num11 : 0.300000011920929, 0.7);
      double num20 = num16 > -0.800000011920929 ? num16 : (-num19 - num8) * 0.899999976158142;
      double num21 = num20 > -1.20000004768372 ? num20 : -1.20000004768372;
      double num22 = num21 * num11 + (num8 * 2.1 + 0.800000011920929);
      if (num22 > 1.70000004768372 && num22 < 2.0)
        num22 = 2.0;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num21 + 0.2) * 100.0);
      data.biomoData[index] = (byte) Mathf.Clamp((float) (num22 * 100.0), 0.0f, 200f);
    }
  }
}


public class PlanetAlgorithm7 : PlanetAlgorithm
{
  private Vector3[] veinVectors = new Vector3[512];
  private EVeinType[] veinVectorTypes = new EVeinType[512];
  private int veinVectorCount;
  private List<Vector2> tmp_vecs = new List<Vector2>(100);

  public override void GenerateTerrain(double modX, double modY)
  {
    double         num1           = 0.008;
    double         num2           = 0.01;
    double         num3           = 0.01;
    double         num4           = 3.0;
    double         num5           = -2.4;
    double         num6           = 0.9;
    double         num7           = 0.5;
    double         num8           = 2.5;
    double         num9           = 0.3;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int            seed1          = dotNet35Random.Next();
    int            seed2          = dotNet35Random.Next();
    SimplexNoise   simplexNoise1  = new SimplexNoise(seed1);
    SimplexNoise   simplexNoise2  = new SimplexNoise(seed2);
    PlanetRawData  data           = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num10 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num11 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num12 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num13 = simplexNoise1.Noise3DFBM(num10 * num1, num11 * num2, num12 * num3, 6) * num4 + num5;
      double num14 = simplexNoise2.Noise3DFBM(num10 * (1.0 / 400.0), num11 * (1.0 / 400.0), num12 * (1.0 / 400.0), 3) * num4 * num6 + num7;
      double num15 = num14 > 0.0 ? num14 * 0.5 : num14;
      double num16 = num13 + num15;
      double f     = num16 > 0.0 ? num16 * 0.5 : num16 * 1.6;
      double num17 = f > 0.0 ? Maths.Levelize3(f, 0.7) : Maths.Levelize2(f, 0.5);
      double num18 = simplexNoise2.Noise3DFBM(num10 * num1 * 2.5, num11 * num2 * 8.0, num12 * num3 * 2.5, 2) * 0.6 - 0.3;
      double num19 = f * num8 + num18 + num9;
      double num20 = num19 < 1.0 ? num19 : (num19 - 1.0) * 0.8 + 1.0;
      double num21 = num17;
      double num22 = num20;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num21) * 100.0);
      data.biomoData[index]  = (byte) Mathf.Clamp((float) (num22 * 100.0), 0.0f, 200f);
    }
  }
  public override void GenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      int num7 = this.planet.galaxy.birthPlanetId == this.planet.id ? 1 : 0;
      float f = this.planet.star.resourceCoef;
      bool infiniteResource = false;
      bool isRareResource = false;
      if (num7 != 0)
        f *= 0.6666667f;
      else if (isRareResource)
      {
        if ((double) f > 1.0)
          f = Mathf.Pow(f, 0.8f);
        f *= 0.7f;
      }
      float num8 = 1f * 1.1f;
      Array.Clear((Array) this.veinVectors, 0, this.veinVectors.Length);
      Array.Clear((Array) this.veinVectorTypes, 0, this.veinVectorTypes.Length);
      this.veinVectorCount = 0;
      Vector3 vector3_1;
      if (this.planet.galaxy.birthPlanetId == this.planet.id)
      {
        Pose pose = this.planet.PredictPose(120.0);
        Vector3 vector3_2 = (Vector3) Maths.QInvRotateLF(pose.rotation, this.planet.star.uPosition - (VectorLF3) pose.position * 40000.0);
        vector3_2.Normalize();
        vector3_1 = vector3_2 * 0.75f;
      }
      else
      {
        vector3_1.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        vector3_1.y = (float) dotNet35Random2.NextDouble() - 0.5f;
        vector3_1.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        vector3_1.Normalize();
        vector3_1 *= (float) (dotNet35Random2.NextDouble() * 0.4 + 0.2);
      }
      for (int index3 = 1; index3 < 15 && this.veinVectorCount < this.veinVectors.Length; ++index3)
      {
        EVeinType eveinType = (EVeinType) index3;
        int num9 = destinationArray1[index3];
        if (num9 > 1)
          num9 += dotNet35Random2.Next(-1, 2);
        for (int index4 = 0; index4 < num9; ++index4)
        {
          int num10 = 0;
          Vector3 zero = Vector3.zero;
          bool flag1 = false;
          while (num10++ < 200)
          {
            zero.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.y = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            if (eveinType != EVeinType.Oil)
              zero += vector3_1;
            zero.Normalize();
            if (eveinType != EVeinType.Bamboo || (double) QueryHeight(data, zero) <= (double) this.planet.realRadius - 4.0)
            {
              bool flag2 = false;
              float num11 = eveinType == EVeinType.Oil ? 100f : 196f;
              for (int index5 = 0; index5 < this.veinVectorCount; ++index5)
              {
                if ((double) (this.veinVectors[index5] - zero).sqrMagnitude < (double) num1 * (double) num1 * (double) num11)
                {
                  flag2 = true;
                  break;
                }
              }
              if (!flag2)
              {
                flag1 = true;
                break;
              }
            }
          }
          if (flag1)
          {
            this.veinVectors[this.veinVectorCount] = zero;
            this.veinVectorTypes[this.veinVectorCount] = eveinType;
            ++this.veinVectorCount;
            if (this.veinVectorCount == this.veinVectors.Length)
              break;
          }
        }
      }
      data.veinCursor = 1;
      this.tmp_vecs.Clear();
      VeinData vein = new VeinData();
      for (int index6 = 0; index6 < this.veinVectorCount; ++index6)
      {
        this.tmp_vecs.Clear();
        Vector3    normalized     = this.veinVectors[index6].normalized;
        EVeinType  veinVectorType = this.veinVectorTypes[index6];
        int        index7         = (int) veinVectorType;
        Quaternion rotation       = FromToRotation(Vector3.up, normalized);
        Vector3    vector3_3      = rotation * Vector3.right;
        Vector3    vector3_4      = rotation * Vector3.forward;
        this.tmp_vecs.Add(Vector2.zero);
        int num12 = Mathf.RoundToInt(destinationArray2[index7] * (float) dotNet35Random2.Next(20, 25));
        if (veinVectorType == EVeinType.Oil)
          num12 = 1;
        int num13 = 0;
        while (num13++ < 20)
        {
          int count = this.tmp_vecs.Count;
          for (int index8 = 0; index8 < count && this.tmp_vecs.Count < num12; ++index8)
          {
            Vector2 vector2_1 = this.tmp_vecs[index8];
            if ((double) vector2_1.sqrMagnitude <= 36.0)
            {
              double num14 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
              Vector2 vector2_2 = new Vector2((float) Math.Cos(num14), (float) Math.Sin(num14));
              vector2_2 += this.tmp_vecs[index8] * 0.2f;
              vector2_2.Normalize();
              Vector2 vector2_3 = this.tmp_vecs[index8] + vector2_2;
              bool flag = false;
              for (int index9 = 0; index9 < this.tmp_vecs.Count; ++index9)
              {
                vector2_1 = this.tmp_vecs[index9] - vector2_3;
                if ((double) vector2_1.sqrMagnitude < 0.850000023841858)
                {
                  flag = true;
                  break;
                }
              }
              if (!flag)
                this.tmp_vecs.Add(vector2_3);
            }
          }
          if (this.tmp_vecs.Count >= num12)
            break;
        }
        float num15 = f;
        if (veinVectorType == EVeinType.Oil)
          num15 = Mathf.Pow(f, 0.5f);
        int num16 = Mathf.RoundToInt(destinationArray3[index7] * 100000f * num15);
        if (num16 < 20)
          num16 = 20;
        int num17 = num16 < 16000 ? Mathf.FloorToInt((float) num16 * (15f / 16f)) : 15000;
        int minValue = num16 - num17;
        int maxValue = num16 + num17 + 1;
        for (int index10 = 0; index10 < this.tmp_vecs.Count; ++index10)
        {
          Vector3 vector3_5 = (this.tmp_vecs[index10].x * vector3_3 + this.tmp_vecs[index10].y * vector3_4) * num1;
          vein.type = veinVectorType;
          vein.groupIndex = (short) (index6 + 1);
          vein.modelIndex = (short) dotNet35Random2.Next(veinModelIndexs[index7], veinModelIndexs[index7] + veinModelCounts[index7]);
          vein.amount = Mathf.RoundToInt((float) dotNet35Random2.Next(minValue, maxValue) * num8);
          if (veinVectorType != EVeinType.Oil)
            vein.amount = Mathf.RoundToInt((float) vein.amount * 1);
          if (vein.amount < 1)
            vein.amount = 1;
          if (infiniteResource && vein.type != EVeinType.Oil)
            vein.amount = 1000000000;
          vein.productId = veinProducts[index7];
          vein.pos       = normalized + vector3_5;
          if (vein.type == EVeinType.Oil)
            vein.pos = this.planet.aux.RawSnap(vein.pos);
          vein.minerCount = 0;
          float num18 = QueryHeight(data, vein.pos);
          data.EraseVegetableAtPoint(vein.pos);
          vein.pos = vein.pos.normalized * num18;
          data.AddVeinData(vein);
        }
      }
      this.tmp_vecs.Clear();
    }
  }

   public override int[] MyGenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return null;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      return destinationArray1;
    }
  }
}

public class PlanetAlgorithm8 : PlanetAlgorithm
{
  public override void GenerateTerrain (double modX, double modY)
  {
    double        num1         = 0.002 * modX;
    double        num2         = 0.002 * modX * modX * 6.66667;
    double        num3         = 0.002 * modX;
    SimplexNoise  simplexNoise = new SimplexNoise (new DotNet35Random (this.planet.seed).Next ());
    PlanetRawData data         = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num4 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num5 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num6 = (double) data.vertices[index].z * (double) this.planet.radius;
      float  num7 = Mathf.Clamp ((float) (simplexNoise.Noise3DFBM (num4 * num1, num5 * num2, num6 * num3, 6, 0.45, 1.8) + 1.0 + modY * 0.00999999977648258), 0.0f, 2f);
      float  num8;
      if ((double) num7 < 1.0)
      {
        float f = Mathf.Cos (num7 * 3.141593f) * 1.1f;
        num8 = (float) (1.0 - ((double) Mathf.Clamp (Mathf.Sign (f) * Mathf.Pow (f, 4f), -1f, 1f) + 1.0) * 0.5);
      }
      else
      {
        float f = Mathf.Cos ((float) (((double) num7 - 1.0) * 3.14159274101257)) * 1.1f;
        num8 = (float) (2.0 - ((double) Mathf.Clamp (Mathf.Sign (f) * Mathf.Pow (f, 4f), -1f, 1f) + 1.0) * 0.5);
      }
      double num9  = (double) num8;
      double num10 = (double) num8;
      double num11 = num10 < 1.0 ? Math.Max (num10 - 0.2, 0.0) * 1.25 : num10;
      double num12 = Maths.Levelize2 (num11 > 1.0 ? Math.Min (num11 * num11, 2.0) : num11);
      data.heightData[index] = (ushort) (((double) this.planet.radius + num9 + 0.1) * 100.0);
      data.biomoData[index]  = (byte) Mathf.Clamp ((float) (num12 * 100.0), 0.0f, 200f);
    }
  }
}

public class PlanetAlgorithm9 : PlanetAlgorithm
{
   public override void GenerateTerrain(double modX, double modY)
  {
    double num1 = 0.01;
    double num2 = 0.012;
    double num3 = 0.01;
    double num4 = 3.0;
    double num5 = -0.2;
    double num6 = 0.9;
    double num7 = 0.5;
    double num8 = 2.5;
    double num9 = 0.3;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    PlanetRawData data = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num10 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num11 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num12 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num13 = simplexNoise1.Noise3DFBM(num10 * num1 * 0.75, num11 * num2 * 0.5, num12 * num3 * 0.75, 6) * num4 + num5;
      double num14 = simplexNoise2.Noise3DFBM(num10 * (1.0 / 400.0), num11 * (1.0 / 400.0), num12 * (1.0 / 400.0), 3) * num4 * num6 + num7;
      double num15 = num14 > 0.0 ? num14 * 0.5 : num14;
      double num16 = num13 + num15;
      double f = num16 > 0.0 ? num16 * 0.5 : num16 * 1.6;
      double num17 = (f > 0.0 ? Maths.Levelize3(f, 0.7) : Maths.Levelize2(f, 0.5)) + 0.618;
      double num18 = num17 > -1.0 ? num17 * 1.5 : num17 * 4.0;
      double num19 = simplexNoise2.Noise3DFBM(num10 * num1 * 2.5, num11 * num2 * 8.0, num12 * num3 * 2.5, 2) * 0.6 - 0.3;
      double num20 = f * num8 + num19 + num9;
      double num21 = simplexNoise1.Noise3DFBM(num10 * num1 * modX, num11 * num2 * modX, num12 * num3 * modX, 6) * num4 + num5;
      double num22 = simplexNoise2.Noise3DFBM(num10 * (1.0 / 400.0), num11 * (1.0 / 400.0), num12 * (1.0 / 400.0), 3) * num4 * num6 + num7;
      double num23 = num22 > 0.0 ? num22 * 0.5 : num22;
      double num24 = Math.Pow((num21 + num23 + 5.0) * 0.13, 6.0) * 24.0 - 24.0;
      double num25 = num18 >= -modY ? 0.0 : Math.Pow(Math.Min(Math.Abs(num18 + modY) / 5.0, 1.0), 1.0);
      double num26 = num18 * (1.0 - num25) + num24 * num25;
      double num27 = num26 > 0.0 ? num26 * 0.5 : num26;
      double num28 = Math.Max(simplexNoise2.Noise3DFBM(num10 * num1 * 1.5, num11 * num2 * 2.0, num12 * num3 * 1.5, 6) * num4 + num5 + 1.0, -0.99);
      double num29 = num28 > 0.0 ? num28 * 0.25 : num28;
      double num30 = Math.Max(num18 + 1.0, 0.0);
      double num31 = (double) Mathf.Clamp01((float) (num30 - 1.0));
      double num32 = Math.Min(num30 > 1.0 ? num31 * num29 * 1.15 + 1.0 : num30, 2.0);
      data.heightData[index] = (ushort) (((double) this.planet.radius + num27 + 0.2) * 100.0);
      data.biomoData[index] = (byte) Mathf.Clamp((float) (num32 * 100.0), 0.0f, 200f);
    }
  }
}

public class PlanetAlgorithm10 : PlanetAlgorithm
{
  private const int       kCircleCount   = 10;
  private       Vector4[] ellipses       = new Vector4[10];
  private       double[]  eccentricities = new double[10];
  private       double[]  heights        = new double[10];
  private double Remap(
    double sourceMin,
    double sourceMax,
    double targetMin,
    double targetMax,
    double x)
  {
    return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
  }

  private double Max(double a, double b) => a <= b ? b : a;

  private static float diff(float a, float b) => (double) a <= (double) b ? b - a : a - b;

    public override void GenerateTerrain(double modX, double modY)
  {
    double num1 = 0.007;
    double num2 = 0.007;
    double num3 = 0.007;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    int seed3 = dotNet35Random.Next();
    int seed4 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    SimplexNoise simplexNoise3 = new SimplexNoise(seed3);
    SimplexNoise simplexNoise4 = new SimplexNoise(seed4);
    int seed5 = dotNet35Random.Next();
    for (int index = 0; index < 10; ++index)
    {
      VectorLF3 vectorLf3 = RandomTable.SphericNormal(ref seed5, 1.0);
      Vector4 vector4 = new Vector4((float) vectorLf3.x, (float) vectorLf3.y, (float) vectorLf3.z);
      vector4.Normalize();
      vector4 *= this.planet.radius;
      vector4.w = (float) (dotNet35Random.NextDouble() * 10.0 + 40.0);
      this.ellipses[index] = vector4;
      this.eccentricities[index] = dotNet35Random.NextDouble() <= 0.5 ? this.Remap(0.0, 1.0, 0.2, 1.0 / 3.0, dotNet35Random.NextDouble()) : this.Remap(0.0, 1.0, 3.0, 5.0, dotNet35Random.NextDouble());
      this.heights[index] = this.Remap(0.0, 1.0, 1.0, 2.0, dotNet35Random.NextDouble());
    }
    PlanetRawData data = this.planet.data;
    for (int index1 = 0; index1 < data.dataLength; ++index1)
    {
      double num4 = (double) data.vertices[index1].x * (double) this.planet.radius;
      double num5 = (double) data.vertices[index1].y * (double) this.planet.radius;
      double num6 = (double) data.vertices[index1].z * (double) this.planet.radius;
      double num7 = Maths.Levelize(num4 * 0.007);
      double num8 = Maths.Levelize(num5 * 0.007);
      double num9 = Maths.Levelize(num6 * 0.007);
      double xin = num7 + simplexNoise3.Noise(num4 * 0.05, num5 * 0.05, num6 * 0.05) * 0.04;
      double yin = num8 + simplexNoise3.Noise(num5 * 0.05, num6 * 0.05, num4 * 0.05) * 0.04;
      double zin = num9 + simplexNoise3.Noise(num6 * 0.05, num4 * 0.05, num5 * 0.05) * 0.04;
      double num10 = Math.Abs(simplexNoise4.Noise(xin, yin, zin));
      double num11 = (0.16 - num10) * 10.0;
      double num12 = num11 > 0.0 ? (num11 > 1.0 ? 1.0 : num11) : 0.0;
      double num13 = num12 * num12;
      double num14 = (simplexNoise3.Noise3DFBM(num5 * 0.005, num6 * 0.005, num4 * 0.005, 4) + 0.22) * 5.0;
      double num15 = num14 > 0.0 ? (num14 > 1.0 ? 1.0 : num14) : 0.0;
      double num16 = Math.Abs(simplexNoise4.Noise3DFBM(xin * 1.5, yin * 1.5, zin * 1.5, 2));
      double x = simplexNoise2.Noise3DFBM(num4 * num1 * 5.0, num5 * num2 * 5.0, num6 * num3 * 5.0, 4);
      double num17 = x * 0.2;
      double a1 = 0.0;
      for (int index2 = 0; index2 < 10; ++index2)
      {
        double num18 = (double) this.ellipses[index2].x - num4;
        double num19 = (double) this.ellipses[index2].y - num5;
        double num20 = (double) this.ellipses[index2].z - num6;
        double num21 = this.eccentricities[index2] * num18 * num18 + num19 * num19 + num20 * num20;
        double num22 = this.Remap(-1.0, 1.0, 0.2, 5.0, x) * num21;
        if (num22 < (double) this.ellipses[index2].w * (double) this.ellipses[index2].w)
        {
          double num23 = 1.0 - (1.0 - (double) Mathf.Sqrt((float) (num22 / ((double) this.ellipses[index2].w * (double) this.ellipses[index2].w))));
          double num24 = 1.0 - num23 * num23 * num23 * num23 + num17 * 2.0;
          if (num24 < 0.0)
            num24 = 0.0;
          a1 = this.Max(a1, this.heights[index2] * num24);
        }
      }
      double num25 = num4 + Math.Sin(num5 * 0.15) * 2.0;
      double num26 = num5 + Math.Sin(num6 * 0.15) * 2.0;
      double num27 = num6 + Math.Sin(num25 * 0.15) * 2.0;
      double num28 = num25 * num1;
      double num29 = num26 * num2;
      double num30 = num27 * num3;
      double f = (double) Mathf.Pow((float) ((simplexNoise1.Noise3DFBM(num28 * 0.6, num29 * 0.6, num30 * 0.6, 4, deltaWLen: 1.8) + 1.0) * 0.5), 1.3f);
      double num31 = this.Remap(-1.0, 1.0, -0.1, 0.15, simplexNoise2.Noise3DFBM(num28 * 6.0, num29 * 6.0, num30 * 6.0, 5));
      double num32 = simplexNoise2.Noise3DFBM(num28 * 5.0 * 3.0, num29 * 5.0, num30 * 5.0, 1);
      double num33 = simplexNoise2.Noise3DFBM(num28 * 5.0 * 3.0 + num32 * 0.3, num29 * 5.0 + num32 * 0.3, num30 * 5.0 + num32 * 0.3, 5) * 0.1;
      double num34 = Math.Min(1.0, Maths.Levelize(Maths.Levelize4(f)));
      if (num34 <= 0.8)
      {
        if (num34 > 0.4)
          num34 += num33;
        else
          num34 += num31;
      }
      double num35 = this.Max(num34 * 2.5 - num34 * a1, num31 * 2.0);
      double num36 = (2.0 - num35) / 2.0;
      double num37 = num35 - num13 * 1.2 * num15 * num36;
      if (num37 >= 0.0)
        num37 += (num10 * 0.25 + num16 * 0.6) * num36;
      double a2 = num37 - 0.1;
      double num38 = Math.Abs(this.Max(a2, -1.0));
      double num39 = 100.0;
      if (num34 < 0.4)
        num38 += this.Remap(-1.0, 1.0, -0.2, 0.2, simplexNoise1.Noise3DFBM(num28 * 2.0 + num39, num29 * 2.0 + num39, num30 * 2.0 + num39, 5));
      data.heightData[index1] = (ushort) (((double) this.planet.radius + a2 + 0.1) * 100.0);
      data.biomoData[index1] = (byte) Mathf.Clamp((float) (num38 * 100.0), 0.0f, 200f);
    }
  }
}
public class PlanetAlgorithm11 : PlanetAlgorithm
{
  private Vector3[] veinVectors = new Vector3[512];
  private EVeinType[] veinVectorTypes = new EVeinType[512];
  private int veinVectorCount;
  private List<Vector2> tmp_vecs = new List<Vector2>(100);

  public override void GenerateTerrain(double modX, double modY)
  {
    double         num1           = 0.007;
    double         num2           = 0.007;
    double         num3           = 0.007;
    double         num4           = 0.002 * modX;
    double         num5           = 0.002 * modX * 4.0;
    double         num6           = 0.002 * modX;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int            seed1          = dotNet35Random.Next();
    int            seed2          = dotNet35Random.Next();
    int            seed3          = dotNet35Random.Next();
    SimplexNoise   simplexNoise1  = new SimplexNoise(seed1);
    SimplexNoise   simplexNoise2  = new SimplexNoise(seed2);
    SimplexNoise   simplexNoise3  = new SimplexNoise(seed3);
    PlanetRawData  data           = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num7  = (double) data.vertices[index].x * (double) this.planet.radius;
      double num8  = (double) data.vertices[index].y * (double) this.planet.radius;
      double num9  = (double) data.vertices[index].z * (double) this.planet.radius;
      double num10 = simplexNoise2.Noise3DFBM(num7 * num1 * 4.0, num8 * num2 * 8.0, num9 * num3 * 4.0, 3);
      double num11 = 0.6;
      double num12 = Maths.Levelize2(Math.Pow(this.Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise1.Noise3DFBM(num7 * num1 * num11, num8 * num1 * 1.5 * 2.5, num9 * num1 * num11, 6, 0.45, 1.8) * 0.95 + num10 * 0.05), modY) + 1.0);
      double num13 = Maths.Levelize3(Math.Pow(this.Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise3.Noise3DFBM(num7 * num4, num8 * num5, num9 * num6, 5, 0.55)), 0.65)) * num12;
      double num14 = Math.Max(-0.3, (num13 - 0.4) * 0.9);
      data.heightData[index] = (ushort) (((double) this.planet.radius + num14) * 100.0);
      data.biomoData[index]  = (byte) Mathf.Clamp((float) (num13 * 100.0), 0.0f, 200f);
    }
  }

  private double Remap(
    double sourceMin,
    double sourceMax,
    double targetMin,
    double targetMax,
    double x)
  {
    return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
  }
  public override void GenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      bool flag1 = this.planet.galaxy.birthPlanetId == this.planet.id;
      // if (flag1)
      //   this.planet.GenBirthPoints(data, _birthSeed);
      float f = this.planet.star.resourceCoef;
      bool infiniteResource = false;
      bool isRareResource = false;
      if (flag1)
        f *= 0.6666667f;
      else if (isRareResource)
      {
        if ((double) f > 1.0)
          f = Mathf.Pow(f, 0.8f);
        f *= 0.7f;
      }
      float num7 = 1f * 1.1f;
      Array.Clear((Array) this.veinVectors, 0, this.veinVectors.Length);
      Array.Clear((Array) this.veinVectorTypes, 0, this.veinVectorTypes.Length);
      this.veinVectorCount = 0;
      Vector3 birthPoint;
      if (flag1)
      {
        birthPoint = this.planet.birthPoint;
        birthPoint.Normalize();
        birthPoint *= 0.75f;
      }
      else
      {
        birthPoint.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.y = (float) dotNet35Random2.NextDouble() - 0.5f;
        birthPoint.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.Normalize();
        birthPoint *= (float) (dotNet35Random2.NextDouble() * 0.4 + 0.2);
      }
      if (flag1)
      {
        this.veinVectorTypes[0] = EVeinType.Iron;
        this.veinVectors[0] = this.planet.birthResourcePoint0;
        this.veinVectorTypes[1] = EVeinType.Copper;
        this.veinVectors[1] = this.planet.birthResourcePoint1;
        this.veinVectorCount = 2;
      }
      for (int index3 = 1; index3 < 15 && this.veinVectorCount < this.veinVectors.Length; ++index3)
      {
        EVeinType eveinType = (EVeinType) index3;
        int num8 = destinationArray1[index3];
        if (num8 > 1)
          num8 += dotNet35Random2.Next(-1, 2);
        for (int index4 = 0; index4 < num8; ++index4)
        {
          int num9 = 0;
          Vector3 zero = Vector3.zero;
          bool flag2 = false;
          while (num9++ < 200)
          {
            zero.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.y = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            if (eveinType != EVeinType.Oil)
              zero += birthPoint;
            zero.Normalize();
            float num10 = QueryHeight(data, zero);
            if ((double) num10 >= (double) this.planet.radius && (eveinType != EVeinType.Oil || (double) num10 >= (double) this.planet.radius + 0.5) && (eveinType > EVeinType.Copper || (double) num10 <= (double) this.planet.radius + 0.699999988079071) && (eveinType != EVeinType.Silicium && eveinType != EVeinType.Titanium || (double) num10 > (double) this.planet.radius + 0.699999988079071))
            {
              bool flag3 = false;
              float num11 = eveinType == EVeinType.Oil ? 100f : 196f;
              for (int index5 = 0; index5 < this.veinVectorCount; ++index5)
              {
                if ((double) (this.veinVectors[index5] - zero).sqrMagnitude < (double) num1 * (double) num1 * (double) num11)
                {
                  flag3 = true;
                  break;
                }
              }
              if (!flag3)
              {
                flag2 = true;
                break;
              }
            }
          }
          if (flag2)
          {
            this.veinVectors[this.veinVectorCount] = zero;
            this.veinVectorTypes[this.veinVectorCount] = eveinType;
            ++this.veinVectorCount;
            if (this.veinVectorCount == this.veinVectors.Length)
              break;
          }
        }
      }
      data.veinCursor = 1;
      this.tmp_vecs.Clear();
      VeinData vein = new VeinData();
      for (int index6 = 0; index6 < this.veinVectorCount; ++index6)
      {
        this.tmp_vecs.Clear();
        Vector3    normalized     = this.veinVectors[index6].normalized;
        EVeinType  veinVectorType = this.veinVectorTypes[index6];
        int        index7         = (int) veinVectorType;
        Quaternion rotation       = FromToRotation(Vector3.up, normalized);
        Vector3    vector3_1      = rotation * Vector3.right;
        Vector3    vector3_2      = rotation * Vector3.forward;
        this.tmp_vecs.Add(Vector2.zero);
        int num12 = Mathf.RoundToInt(destinationArray2[index7] * (float) dotNet35Random2.Next(20, 25));
        if (veinVectorType == EVeinType.Oil)
          num12 = 1;
        float num13 = destinationArray3[index7];
        if (flag1 && index6 < 2)
        {
          num12 = 6;
          num13 = 0.2f;
        }
        int num14 = 0;
        while (num14++ < 20)
        {
          int count = this.tmp_vecs.Count;
          for (int index8 = 0; index8 < count && this.tmp_vecs.Count < num12; ++index8)
          {
            Vector2 vector2_1 = this.tmp_vecs[index8];
            if ((double) vector2_1.sqrMagnitude <= 36.0)
            {
              double num15 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
              Vector2 vector2_2 = new Vector2((float) Math.Cos(num15), (float) Math.Sin(num15));
              vector2_2 += this.tmp_vecs[index8] * 0.2f;
              vector2_2.Normalize();
              Vector2 vector2_3 = this.tmp_vecs[index8] + vector2_2;
              bool flag4 = false;
              for (int index9 = 0; index9 < this.tmp_vecs.Count; ++index9)
              {
                vector2_1 = this.tmp_vecs[index9] - vector2_3;
                if ((double) vector2_1.sqrMagnitude < 0.850000023841858)
                {
                  flag4 = true;
                  break;
                }
              }
              if (!flag4)
                this.tmp_vecs.Add(vector2_3);
            }
          }
          if (this.tmp_vecs.Count >= num12)
            break;
        }
        float num16 = f;
        if (veinVectorType == EVeinType.Oil)
          num16 = Mathf.Pow(f, 0.5f);
        int num17 = Mathf.RoundToInt(num13 * 100000f * num16);
        if (num17 < 20)
          num17 = 20;
        int num18 = num17 < 16000 ? Mathf.FloorToInt((float) num17 * (15f / 16f)) : 15000;
        int minValue = num17 - num18;
        int maxValue = num17 + num18 + 1;
        for (int index10 = 0; index10 < this.tmp_vecs.Count; ++index10)
        {
          Vector3 vector3_3 = (tmp_vecs[index10].x * vector3_1 + tmp_vecs[index10].y * vector3_2) * num1;
          vein.type = veinVectorType;
          vein.groupIndex = (short) (index6 + 1);
          vein.modelIndex = (short) dotNet35Random2.Next(veinModelIndexs[index7], veinModelIndexs[index7] + veinModelCounts[index7]);
          vein.amount = Mathf.RoundToInt((float) dotNet35Random2.Next(minValue, maxValue) * num7);
          if (veinVectorType != EVeinType.Oil)
            vein.amount = Mathf.RoundToInt((float) vein.amount * 1);
          if (vein.amount < 1)
            vein.amount = 1;
          if (infiniteResource && vein.type != EVeinType.Oil)
            vein.amount = 1000000000;
          vein.productId = veinProducts[index7];
          vein.pos       = normalized + vector3_3;
          if (vein.type == EVeinType.Oil)
            vein.pos = this.planet.aux.RawSnap(vein.pos);
          vein.minerCount = 0;
          float num19 = QueryHeight(data, vein.pos);
          data.EraseVegetableAtPoint(vein.pos);
          vein.pos = vein.pos.normalized * num19;
          if (this.planet.waterItemId == 0 || (double) num19 >= (double) this.planet.radius)
            data.AddVeinData(vein);
        }
      }
      this.tmp_vecs.Clear();
    }
  }
  
  public override int[] MyGenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return null;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      return destinationArray1;
    }
  }

  
}

public class PlanetAlgorithm12 : PlanetAlgorithm
{
  private Vector3[] veinVectors = new Vector3[512];
  private EVeinType[] veinVectorTypes = new EVeinType[512];
  private int veinVectorCount;
  private List<Vector2> tmp_vecs = new List<Vector2>(100);

  private double CurveEvaluate(double t)
  {
    t /= 0.6;
    return t >= 1.0 ? 0.0 : Math.Pow(1.0 - t, 3.0) + Math.Pow(1.0 - t, 2.0) * 3.0 * t;
  }

  public override void GenerateTerrain(double modX, double modY)
  {
    double         num1           = 1.1 * modX;
    double         num2           = 0.2;
    double         num3           = 8.0;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int            seed1          = dotNet35Random.Next();
    int            seed2          = dotNet35Random.Next();
    SimplexNoise   simplexNoise1  = new SimplexNoise(seed1);
    SimplexNoise   simplexNoise2  = new SimplexNoise(seed2);
    PlanetRawData  data           = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num4  = Math.Abs(Math.Asin((double) data.vertices[index].y)) * 2.0 / Math.PI;
      double x     = (double) data.vertices[index].x;
      double num5  = (double) data.vertices[index].y * 2.5 * modY;
      double z     = (double) data.vertices[index].z;
      double num6  = simplexNoise2.Noise3DFBM(x * num1, num5 * num1, z * num1, 3, 0.4) * 0.2;
      double num7  = simplexNoise1.RidgedNoise(x * num1, num5 * num1 - num6, z * num1, 6, 0.7, initialAmp: 0.8);
      double num8  = simplexNoise1.Noise3DFBMInitialAmp(x * num1, num5 * num1 - num6, z * num1, 6, 0.6, initialAmp: 0.7);
      double num9  = num8 * (num7 + num8);
      double val   = (Math.Pow(Maths.Clamp01(this.Remap(-8.0, 8.0, 0.0, 1.0, num2 + num3 * num9 * num7 + 0.5)) + 0.5, 1.5) - this.CurveEvaluate(num4 * 0.9)) * 2.0;
      double num10 = Maths.Clamp(val, 0.0, 2.0) * 1.1 - 0.2;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num10) * 100.0);
      data.biomoData[index]  = (byte) Mathf.Clamp((float) (val * 100.0), 0.0f, 200f);
    }
  }

  private double Remap(
    double sourceMin,
    double sourceMax,
    double targetMin,
    double targetMax,
    double x)
  {
    return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
  }
  public override void GenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      bool flag1 = this.planet.galaxy.birthPlanetId == this.planet.id;
      // if (flag1)
      //   this.planet.GenBirthPoints(data, _birthSeed);
      float f = this.planet.star.resourceCoef;
      bool infiniteResource = false;
      bool isRareResource = false;
      if (flag1)
        f *= 0.6666667f;
      else if (isRareResource)
      {
        if ((double) f > 1.0)
          f = Mathf.Pow(f, 0.8f);
        f *= 0.7f;
      }
      float num7 = 1f * 1.1f;
      Array.Clear((Array) this.veinVectors, 0, this.veinVectors.Length);
      Array.Clear((Array) this.veinVectorTypes, 0, this.veinVectorTypes.Length);
      this.veinVectorCount = 0;
      Vector3 birthPoint;
      if (flag1)
      {
        birthPoint = this.planet.birthPoint;
        birthPoint.Normalize();
        birthPoint *= 0.75f;
      }
      else
      {
        birthPoint.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.y = (float) dotNet35Random2.NextDouble() - 0.5f;
        birthPoint.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.Normalize();
        birthPoint *= (float) (dotNet35Random2.NextDouble() * 0.4 + 0.2);
      }
      if (flag1)
      {
        this.veinVectorTypes[0] = EVeinType.Iron;
        this.veinVectors[0] = this.planet.birthResourcePoint0;
        this.veinVectorTypes[1] = EVeinType.Copper;
        this.veinVectors[1] = this.planet.birthResourcePoint1;
        this.veinVectorCount = 2;
      }
      for (int index3 = 1; index3 < 15 && this.veinVectorCount < this.veinVectors.Length; ++index3)
      {
        EVeinType eveinType = (EVeinType) index3;
        int num8 = destinationArray1[index3];
        if (num8 > 1)
          num8 += dotNet35Random2.Next(-1, 2);
        for (int index4 = 0; index4 < num8; ++index4)
        {
          int num9 = 0;
          Vector3 zero = Vector3.zero;
          bool flag2 = false;
          while (num9++ < 200)
          {
            zero.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.y = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            if (eveinType != EVeinType.Oil)
              zero += birthPoint;
            zero.Normalize();
            float num10 = QueryHeight(data,zero);
            if ((double) num10 >= (double) this.planet.radius && (eveinType != EVeinType.Oil || (double) num10 >= (double) this.planet.radius + 0.5) && (eveinType != EVeinType.Fireice || (double) num10 >= (double) this.planet.radius + 1.20000004768372))
            {
              bool flag3 = false;
              float num11 = eveinType == EVeinType.Oil ? 100f : 196f;
              for (int index5 = 0; index5 < this.veinVectorCount; ++index5)
              {
                if ((double) (this.veinVectors[index5] - zero).sqrMagnitude < (double) num1 * (double) num1 * (double) num11)
                {
                  flag3 = true;
                  break;
                }
              }
              if (!flag3)
              {
                flag2 = true;
                break;
              }
            }
          }
          if (flag2)
          {
            this.veinVectors[this.veinVectorCount] = zero;
            this.veinVectorTypes[this.veinVectorCount] = eveinType;
            ++this.veinVectorCount;
            if (this.veinVectorCount == this.veinVectors.Length)
              break;
          }
        }
      }
      data.veinCursor = 1;
      this.tmp_vecs.Clear();
      VeinData vein = new VeinData();
      for (int index6 = 0; index6 < this.veinVectorCount; ++index6)
      {
        this.tmp_vecs.Clear();
        Vector3    normalized     = this.veinVectors[index6].normalized;
        EVeinType  veinVectorType = this.veinVectorTypes[index6];
        int        index7         = (int) veinVectorType;
        Quaternion rotation       = FromToRotation(Vector3.up, normalized);
        Vector3    vector3_1      = rotation * Vector3.right;
        Vector3    vector3_2     = rotation * Vector3.forward;
        this.tmp_vecs.Add(Vector2.zero);
        int num12 = Mathf.RoundToInt(destinationArray2[index7] * (float) dotNet35Random2.Next(20, 25));
        if (veinVectorType == EVeinType.Oil)
          num12 = 1;
        float num13 = destinationArray3[index7];
        if (flag1 && index6 < 2)
        {
          num12 = 6;
          num13 = 0.2f;
        }
        int num14 = 0;
        while (num14++ < 20)
        {
          int count = this.tmp_vecs.Count;
          for (int index8 = 0; index8 < count && this.tmp_vecs.Count < num12; ++index8)
          {
            Vector2 vector2_1 = this.tmp_vecs[index8];
            if ((double) vector2_1.sqrMagnitude <= 36.0)
            {
              double num15 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
              Vector2 vector2_2 = new Vector2((float) Math.Cos(num15), (float) Math.Sin(num15));
              vector2_2 += this.tmp_vecs[index8] * 0.2f;
              vector2_2.Normalize();
              Vector2 vector2_3 = this.tmp_vecs[index8] + vector2_2;
              bool flag4 = false;
              for (int index9 = 0; index9 < this.tmp_vecs.Count; ++index9)
              {
                vector2_1 = this.tmp_vecs[index9] - vector2_3;
                if ((double) vector2_1.sqrMagnitude < 0.850000023841858)
                {
                  flag4 = true;
                  break;
                }
              }
              if (!flag4)
                this.tmp_vecs.Add(vector2_3);
            }
          }
          if (this.tmp_vecs.Count >= num12)
            break;
        }
        float num16 = f;
        if (veinVectorType == EVeinType.Oil)
          num16 = Mathf.Pow(f, 0.5f);
        int num17 = Mathf.RoundToInt(num13 * 100000f * num16);
        if (num17 < 20)
          num17 = 20;
        int num18 = num17 < 16000 ? Mathf.FloorToInt((float) num17 * (15f / 16f)) : 15000;
        int minValue = num17 - num18;
        int maxValue = num17 + num18 + 1;
        for (int index10 = 0; index10 < this.tmp_vecs.Count; ++index10)
        {
          Vector3 vector3_3 = (this.tmp_vecs[index10].x * vector3_1 + this.tmp_vecs[index10].y * vector3_2) * num1;
          vein.type = veinVectorType;
          vein.groupIndex = (short) (index6 + 1);
          vein.modelIndex = (short) dotNet35Random2.Next(veinModelIndexs[index7], veinModelIndexs[index7] + veinModelCounts[index7]);
          vein.amount = Mathf.RoundToInt((float) dotNet35Random2.Next(minValue, maxValue) * num7);
          if (veinVectorType != EVeinType.Oil)
            vein.amount = Mathf.RoundToInt((float) vein.amount * 1);
          if (vein.amount < 1)
            vein.amount = 1;
          if (infiniteResource && vein.type != EVeinType.Oil)
            vein.amount = 1000000000;
          vein.productId = veinProducts[index7];
          vein.pos       = normalized + vector3_3;
          if (vein.type == EVeinType.Oil)
            vein.pos = this.planet.aux.RawSnap(vein.pos);
          vein.minerCount = 0;
          float num19 = QueryHeight(data, vein.pos);
          data.EraseVegetableAtPoint(vein.pos);
          vein.pos = vein.pos.normalized * num19;
          if (this.planet.waterItemId == 0 || (double) num19 >= (double) this.planet.radius)
            data.AddVeinData(vein);
        }
      }
      this.tmp_vecs.Clear();
    }
  }
  
    public override int[] MyGenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return null;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      return destinationArray1;
    }
  }

}

public class PlanetAlgorithm13 : PlanetAlgorithm
{
  private Vector3[] veinVectors = new Vector3[512];
  private EVeinType[] veinVectorTypes = new EVeinType[512];
  private int veinVectorCount;
  private List<Vector2> tmp_vecs = new List<Vector2>(100);
  private float[] notClampedBiomo;
  
  
  public override void GenerateTerrain(double modX, double modY)
  {
    double        num1         = 0.007 * modX;
    double        num2         = 0.007 * modX;
    double        num3         = 0.007 * modX;
    SimplexNoise  simplexNoise = new SimplexNoise(new DotNet35Random(this.planet.seed).Next());
    PlanetRawData data         = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num4 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num5 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num6 = (double) data.vertices[index].z * (double) this.planet.radius;
      double x    = this.Remap(0.0, 2.0, 0.0, 4.0, Math.Pow(this.Remap(-1.0, 1.0, 0.0, 1.0, simplexNoise.Noise3DFBM(num4 * num1, num5 * num2, num6 * num3, 6)), modY) * (49.0 / 16.0));
      if (x < 1.0)
        x = Math.Pow(x, 2.0);
      double num7 = Math.Min(x - 0.2, 4.0);
      Math.Max(1.0 - Math.Abs(1.0 - num7), 0.0);
      if (num7 > 2.0)
        num7 = num7 <= 3.0 ? 2.0 - 1.0 * (num7 - 2.0) : (num7 <= 3.5 ? 1.0 : 1.0 + 2.0 * (num7 - 3.5));
      double num8 = num7;
      data.heightData[index] = (ushort) (((double) this.planet.radius + num8 + 0.1) * 100.0);
      data.biomoData[index]  = (byte) Mathf.Clamp((float) (num7 * 100.0), 0.0f, 200f);
    }
  }

  private double Remap(
    double sourceMin,
    double sourceMax,
    double targetMin,
    double targetMax,
    double x)
  {
    return (x - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
  }

  private static float diff(float a, float b) => (double) a <= (double) b ? b - a : a - b;
  public override void GenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      bool flag1 = this.planet.galaxy.birthPlanetId == this.planet.id;
      // if (flag1)
      //   this.planet.GenBirthPoints(data, _birthSeed);
      float f = this.planet.star.resourceCoef;
      bool infiniteResource = false;
      bool isRareResource = false;
      if (flag1)
        f *= 0.6666667f;
      else if (isRareResource)
      {
        if ((double) f > 1.0)
          f = Mathf.Pow(f, 0.8f);
        f *= 0.7f;
      }
      float num7 = 1f * 1.1f;
      Array.Clear((Array) this.veinVectors, 0, this.veinVectors.Length);
      Array.Clear((Array) this.veinVectorTypes, 0, this.veinVectorTypes.Length);
      this.veinVectorCount = 0;
      Vector3 birthPoint;
      if (flag1)
      {
        birthPoint = this.planet.birthPoint;
        birthPoint.Normalize();
        birthPoint *= 0.75f;
      }
      else
      {
        birthPoint.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.y = (float) dotNet35Random2.NextDouble() - 0.5f;
        birthPoint.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
        birthPoint.Normalize();
        birthPoint *= (float) (dotNet35Random2.NextDouble() * 0.4 + 0.2);
      }
      if (flag1)
      {
        this.veinVectorTypes[0] = EVeinType.Iron;
        this.veinVectors[0] = this.planet.birthResourcePoint0;
        this.veinVectorTypes[1] = EVeinType.Copper;
        this.veinVectors[1] = this.planet.birthResourcePoint1;
        this.veinVectorCount = 2;
      }
      for (int index3 = 1; index3 < 15 && this.veinVectorCount < this.veinVectors.Length; ++index3)
      {
        EVeinType eveinType = (EVeinType) index3;
        int num8 = destinationArray1[index3];
        if (num8 > 1)
          num8 += dotNet35Random2.Next(-1, 2);
        for (int index4 = 0; index4 < num8; ++index4)
        {
          int num9 = 0;
          Vector3 zero = Vector3.zero;
          bool flag2 = false;
          while (num9++ < 200)
          {
            zero.x = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.y = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            zero.z = (float) (dotNet35Random2.NextDouble() * 2.0 - 1.0);
            if (eveinType != EVeinType.Oil)
              zero += birthPoint;
            zero.Normalize();
            float num10 = QueryHeight(data, zero);
            if ((double) num10 >= (double) this.planet.radius && (eveinType != EVeinType.Oil || (double) num10 >= (double) this.planet.radius + 0.5) && (eveinType > EVeinType.Titanium || (double) num10 <= (double) this.planet.radius + 0.699999988079071))
            {
              bool flag3 = false;
              float num11 = eveinType == EVeinType.Oil ? 100f : 196f;
              for (int index5 = 0; index5 < this.veinVectorCount; ++index5)
              {
                if ((double) (this.veinVectors[index5] - zero).sqrMagnitude < (double) num1 * (double) num1 * (double) num11)
                {
                  flag3 = true;
                  break;
                }
              }
              if (!flag3)
              {
                flag2 = true;
                break;
              }
            }
          }
          if (flag2)
          {
            this.veinVectors[this.veinVectorCount] = zero;
            this.veinVectorTypes[this.veinVectorCount] = eveinType;
            ++this.veinVectorCount;
            if (this.veinVectorCount == this.veinVectors.Length)
              break;
          }
        }
      }
      data.veinCursor = 1;
      this.tmp_vecs.Clear();
      VeinData vein = new VeinData();
      for (int index6 = 0; index6 < this.veinVectorCount; ++index6)
      {
        this.tmp_vecs.Clear();
        Vector3    normalized     = this.veinVectors[index6].normalized;
        EVeinType  veinVectorType = this.veinVectorTypes[index6];
        int        index7         = (int) veinVectorType;
        Quaternion rotation       = FromToRotation(Vector3.up, normalized);
        Vector3    vector3_1      = rotation * Vector3.right;
        Vector3    vector3_2      = rotation * Vector3.forward;
        this.tmp_vecs.Add(Vector2.zero);
        int num12 = Mathf.RoundToInt(destinationArray2[index7] * (float) dotNet35Random2.Next(20, 25));
        if (veinVectorType == EVeinType.Oil)
          num12 = 1;
        float num13 = destinationArray3[index7];
        if (flag1 && index6 < 2)
        {
          num12 = 6;
          num13 = 0.2f;
        }
        int num14 = 0;
        while (num14++ < 20)
        {
          int count = this.tmp_vecs.Count;
          for (int index8 = 0; index8 < count && this.tmp_vecs.Count < num12; ++index8)
          {
            Vector2 vector2_1 = this.tmp_vecs[index8];
            if ((double) vector2_1.sqrMagnitude <= 36.0)
            {
              double num15 = dotNet35Random2.NextDouble() * Math.PI * 2.0;
              Vector2 vector2_2 = new Vector2((float) Math.Cos(num15), (float) Math.Sin(num15));
              vector2_2 += this.tmp_vecs[index8] * 0.2f;
              vector2_2.Normalize();
              Vector2 vector2_3 = this.tmp_vecs[index8] + vector2_2;
              bool flag4 = false;
              for (int index9 = 0; index9 < this.tmp_vecs.Count; ++index9)
              {
                vector2_1 = this.tmp_vecs[index9] - vector2_3;
                if ((double) vector2_1.sqrMagnitude < 0.850000023841858)
                {
                  flag4 = true;
                  break;
                }
              }
              if (!flag4)
                this.tmp_vecs.Add(vector2_3);
            }
          }
          if (this.tmp_vecs.Count >= num12)
            break;
        }
        float num16 = f;
        if (veinVectorType == EVeinType.Oil)
          num16 = Mathf.Pow(f, 0.5f);
        int num17 = Mathf.RoundToInt(num13 * 100000f * num16);
        if (num17 < 20)
          num17 = 20;
        int num18 = num17 < 16000 ? Mathf.FloorToInt((float) num17 * (15f / 16f)) : 15000;
        int minValue = num17 - num18;
        int maxValue = num17 + num18 + 1;
        for (int index10 = 0; index10 < this.tmp_vecs.Count; ++index10)
        {
          Vector3 vector3_3 = (this.tmp_vecs[index10].x * vector3_1 + this.tmp_vecs[index10].y * vector3_2) * num1;
          vein.type = veinVectorType;
          vein.groupIndex = (short) (index6 + 1);
          vein.modelIndex = (short) dotNet35Random2.Next(veinModelIndexs[index7], veinModelIndexs[index7] + veinModelCounts[index7]);
          vein.amount = Mathf.RoundToInt((float) dotNet35Random2.Next(minValue, maxValue) * num7);
          if (veinVectorType != EVeinType.Oil)
            vein.amount = Mathf.RoundToInt((float) vein.amount * 1);
          if (vein.amount < 1)
            vein.amount = 1;
          if (infiniteResource && vein.type != EVeinType.Oil)
            vein.amount = 1000000000;
          vein.productId = veinProducts[index7];
          vein.pos       = normalized + vector3_3;
          if (vein.type == EVeinType.Oil)
            vein.pos = this.planet.aux.RawSnap(vein.pos);
          vein.minerCount = 0;
          float num19 = QueryHeight(data, vein.pos);
          data.EraseVegetableAtPoint(vein.pos);
          vein.pos = vein.pos.normalized * num19;
          if (this.planet.waterItemId == 0 || (double) num19 >= (double) this.planet.radius)
            data.AddVeinData(vein);
        }
      }
      this.tmp_vecs.Clear();
    }
  }
  
  public override int[] MyGenerateVeins()
  {
    lock (this.planet)
    {
      ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
      if (themeProto == null)
        return null;
      DotNet35Random dotNet35Random1 = new DotNet35Random(this.planet.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      int _birthSeed = dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      PlanetRawData data = this.planet.data;
      float num1 = 2.1f / this.planet.radius;
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[] veinModelIndexs = PlanetModelingManager.veinModelIndexs;
      int[] veinModelCounts = PlanetModelingManager.veinModelCounts;
      int[] veinProducts = PlanetModelingManager.veinProducts;
      int[] destinationArray1 = new int[veinProtos.Length];
      float[] destinationArray2 = new float[veinProtos.Length];
      float[] destinationArray3 = new float[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy((Array) themeProto.VeinSpot, 0, (Array) destinationArray1, 1, Math.Min(themeProto.VeinSpot.Length, destinationArray1.Length - 1));
      if (themeProto.VeinCount != null)
        Array.Copy((Array) themeProto.VeinCount, 0, (Array) destinationArray2, 1, Math.Min(themeProto.VeinCount.Length, destinationArray2.Length - 1));
      if (themeProto.VeinOpacity != null)
        Array.Copy((Array) themeProto.VeinOpacity, 0, (Array) destinationArray3, 1, Math.Min(themeProto.VeinOpacity.Length, destinationArray3.Length - 1));
      float p = 1f;
      ESpectrType spectr = this.planet.star.spectr;
      switch (this.planet.star.type)
      {
        case EStarType.MainSeqStar:
          switch (spectr)
          {
            case ESpectrType.M:
              p = 2.5f;
              break;
            case ESpectrType.K:
              p = 1f;
              break;
            case ESpectrType.G:
              p = 0.7f;
              break;
            case ESpectrType.F:
              p = 0.6f;
              break;
            case ESpectrType.A:
              p = 1f;
              break;
            case ESpectrType.B:
              p = 0.4f;
              break;
            case ESpectrType.O:
              p = 1.6f;
              break;
          }
          break;
        case EStarType.GiantStar:
          p = 2.5f;
          break;
        case EStarType.WhiteDwarf:
          p = 3.5f;
          ++destinationArray1[9];
          ++destinationArray1[9];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[9];
          destinationArray2[9] = 0.7f;
          destinationArray3[9] = 1f;
          ++destinationArray1[10];
          ++destinationArray1[10];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.449999988079071; ++index)
            ++destinationArray1[10];
          destinationArray2[10] = 0.7f;
          destinationArray3[10] = 1f;
          ++destinationArray1[12];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.5; ++index)
            ++destinationArray1[12];
          destinationArray2[12] = 0.7f;
          destinationArray3[12] = 0.3f;
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++destinationArray1[14];
          for (int index = 1; index < 12 && dotNet35Random1.NextDouble() < 0.649999976158142; ++index)
            ++destinationArray1[14];
          destinationArray2[14] = 0.7f;
          destinationArray3[14] = 0.3f;
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int rareVein = themeProto.RareVeins[index1];
        float num2 = this.planet.star.index == 0 ? themeProto.RareSettings[index1 * 4] : themeProto.RareSettings[index1 * 4 + 1];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float rareSetting2 = themeProto.RareSettings[index1 * 4 + 3];
        float num3 = rareSetting2;
        float num4 = 1f - Mathf.Pow(1f - num2, p);
        float num5 = 1f - Mathf.Pow(1f - rareSetting2, p);
        float num6 = 1f - Mathf.Pow(1f - num3, p);
        if (dotNet35Random1.NextDouble() < (double) num4)
        {
          ++destinationArray1[rareVein];
          destinationArray2[rareVein] = num5;
          destinationArray3[rareVein] = num5;
          for (int index2 = 1; index2 < 12 && dotNet35Random1.NextDouble() < (double) rareSetting1; ++index2)
            ++destinationArray1[rareVein];
        }
      }
      return destinationArray1;
    }
  }

}

public class PlanetAlgorithm14 : PlanetAlgorithm
{
  private const double LAVAWIDTH = 0.12;

  public override void GenerateTerrain(double modX, double modY)
  {
    double num1 = 0.007;
    double num2 = 0.007;
    double num3 = 0.007;
    DotNet35Random dotNet35Random = new DotNet35Random(this.planet.seed);
    int seed1 = dotNet35Random.Next();
    int seed2 = dotNet35Random.Next();
    int seed3 = dotNet35Random.Next();
    int seed4 = dotNet35Random.Next();
    SimplexNoise simplexNoise1 = new SimplexNoise(seed1);
    SimplexNoise simplexNoise2 = new SimplexNoise(seed2);
    SimplexNoise simplexNoise3 = new SimplexNoise(seed3);
    SimplexNoise simplexNoise4 = new SimplexNoise(seed4);
    PlanetRawData data = this.planet.data;
    for (int index = 0; index < data.dataLength; ++index)
    {
      double num4 = (double) data.vertices[index].x * (double) this.planet.radius;
      double num5 = (double) data.vertices[index].y * (double) this.planet.radius;
      double num6 = (double) data.vertices[index].z * (double) this.planet.radius;
      double num7 = Maths.Levelize(num4 * 0.007 / 2.0);
      double num8 = Maths.Levelize(num5 * 0.007 / 2.0);
      double num9 = Maths.Levelize(num6 * 0.007 / 2.0);
      double xin = num7 + simplexNoise3.Noise(num4 * 0.05, num5 * 0.05, num6 * 0.05) * 0.04;
      double yin = num8 + simplexNoise3.Noise(num5 * 0.05, num6 * 0.05, num4 * 0.05) * 0.04;
      double zin = num9 + simplexNoise3.Noise(num6 * 0.05, num4 * 0.05, num5 * 0.05) * 0.04;
      double num10 = (0.12 - Math.Abs(simplexNoise4.Noise(xin, yin, zin))) * 10.0;
      double num11 = num10 > 0.0 ? (num10 > 1.0 ? 1.0 : num10) : 0.0;
      double num12 = num11 * num11;
      double num13 = (simplexNoise3.Noise3DFBM(num5 * 0.005, num6 * 0.005, num4 * 0.005, 4) + 0.22) * 5.0;
      double num14 = num13 > 0.0 ? (num13 > 1.0 ? 1.0 : num13) : 0.0;
      Math.Abs(simplexNoise4.Noise3DFBM(xin * 1.5, yin * 1.5, zin * 1.5, 2));
      double num15 = num4 + Math.Sin(num5 * 0.15) * 3.0;
      double num16 = num5 + Math.Sin(num6 * 0.15) * 3.0;
      double num17 = num6 + Math.Sin(num15 * 0.15) * 3.0;
      double num18 = 0.0;
      double num19 = simplexNoise1.Noise3DFBM(num15 * num1 * 1.0, num16 * num2 * 1.1, num17 * num3 * 1.0, 6, deltaWLen: 1.8);
      double num20 = simplexNoise2.Noise3DFBM(num15 * num1 * 1.3 + 0.5, num16 * num2 * 2.8 + 0.2, num17 * num3 * 1.3 + 0.7, 3) * 2.0;
      double num21 = simplexNoise2.Noise3DFBM(num15 * num1 * 6.0, num16 * num2 * 12.0, num17 * num3 * 6.0, 2) * 2.0;
      double num22 = simplexNoise2.Noise3DFBM(num15 * num1 * 0.8, num16 * num2 * 0.8, num17 * num3 * 0.8, 2) * 2.0;
      double f = num19 * 2.0 + 0.92 + (double) Mathf.Clamp01((float) (num20 * (double) Mathf.Abs((float) num22 + 0.5f) - 0.35) * 1f);
      if (f < 0.0)
        f = 0.0;
      double t = Maths.Levelize2(f);
      if (t > 0.0)
        t = Maths.Levelize4(Maths.Levelize2(f));
      double num23 = t > 0.0 ? (t > 1.0 ? (t > 2.0 ? (double) Mathf.Lerp(1.4f, 2.7f, (float) t - 2f) + num21 * 0.12 : (double) Mathf.Lerp(0.3f, 1.4f, (float) t - 1f) + num21 * 0.12) : (double) Mathf.Lerp(0.0f, 0.3f, (float) t) + num21 * 0.1) : (double) Mathf.Lerp(-4f, 0.0f, (float) t + 1f);
      if (f < 0.0)
        f *= 2.0;
      if (f < 1.0)
        f = Maths.Levelize(f);
      double num24 = num18 - num12 * 1.2 * num14;
      if (num24 >= 0.0)
        num24 = num23;
      double num25 = num24 - 0.1;
      double num26 = (double) Mathf.Abs((float) f);
      double num27 = Math.Pow((double) Mathf.Clamp01((float) ((-num25 + 2.0) / 2.5)), 10.0);
      double num28 = (1.0 - num27) * num26 + num27 * 2.0;
      double num29 = num28 > 0.0 ? (num28 > 2.0 ? 2.0 : num28) : 0.0;
      double num30 = num29 + (num29 > 1.8 ? -num21 * 0.8 : num21 * 0.2) * (1.0 - num27);
      double num31 = -0.3 - num25;
      if (num31 > 0.0)
      {
        double num32 = simplexNoise2.Noise(num15 * 0.16, num16 * 0.16, num17 * 0.16) - 1.0;
        double num33 = num31 > 1.0 ? 1.0 : num31;
        double num34 = (3.0 - num33 - num33) * num33 * num33;
        num25 = -0.3 - num34 * 10.0 + num34 * num34 * num34 * num34 * num32 * 0.5;
      }
      data.heightData[index] = (ushort) (((double) this.planet.radius + num25 + 0.2) * 100.0);
      data.biomoData[index] = (byte) Mathf.Clamp((float) (num30 * 100.0), 0.0f, 200f);
    }
  }

}
}