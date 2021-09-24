
using System;
using System.Collections.Generic;
using DysonSphereProgramSeed.Algorithms;
using DysonSphereProgramSeed.Dyson;

namespace DspFindSeed
{
  public class DspData
  {
    public static void PlanetCompute (GalaxyData galaxyData, StarData star, PlanetData planetData)
    {
      GenerateVeins (planetData, star, galaxyData, true);
    }

    public static void PlanetComputePlus(GalaxyData galaxyData, StarData star, PlanetData planetData)
    {
      PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planetData);
      planetData.data = new PlanetRawData(planetData.precision);
      planetData.data.SeedExt_CalcVerts();
      planetAlgorithm.GenerateTerrain(planetData.mod_x, planetData.mod_y);
      planetAlgorithm.GenerateVeins(star, galaxyData, false);
    }

    public static void GenerateVeins (PlanetData planetData, StarData star, GalaxyData galaxy, bool sketchOnly)
    {
      ThemeProto themeProto = LDB.themes.Select (planetData.theme);
      if (themeProto == null)
        return;
      URandom1 urandom1_1 = new URandom1 (planetData.seed);
      urandom1_1.Next ();
      urandom1_1.Next ();
      urandom1_1.Next ();
      urandom1_1.Next ();
      int         _birthSeed = urandom1_1.Next ();
      URandom1    urandom1_2 = new URandom1 (urandom1_1.Next ());
      VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
      int[]       numArray1  = new int[veinProtos.Length];
      if (themeProto.VeinSpot != null)
        Array.Copy ((Array)themeProto.VeinSpot, 0, (Array)numArray1, 1, Math.Min (themeProto.VeinSpot.Length, numArray1.Length - 1));
      float       p      = 1f;
      ESpectrType spectr = star.spectr;
      switch (star.type)
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
          ++numArray1[9];
          ++numArray1[9];
          for (int index = 1; index < 12 && urandom1_1.NextDouble () < 0.449999988079071; ++index)
            ++numArray1[9];
          ++numArray1[10];
          ++numArray1[10];
          for (int index = 1; index < 12 && urandom1_1.NextDouble () < 0.449999988079071; ++index)
            ++numArray1[10];
          ++numArray1[12];
          for (int index = 1; index < 12 && urandom1_1.NextDouble () < 0.5; ++index)
            ++numArray1[12];
          break;
        case EStarType.NeutronStar:
          p = 4.5f;
          ++numArray1[14];
          for (int index = 1; index < 12 && urandom1_1.NextDouble () < 0.649999976158142; ++index)
            ++numArray1[14];
          break;
        case EStarType.BlackHole:
          p = 5f;
          ++numArray1[14];
          for (int index = 1; index < 12 && urandom1_1.NextDouble () < 0.649999976158142; ++index)
            ++numArray1[14];
          break;
      }
      for (int index1 = 0; index1 < themeProto.RareVeins.Length; ++index1)
      {
        int   rareVein     = themeProto.RareVeins[index1];
        float num2         = star.index != 0 ? themeProto.RareSettings[index1 * 4 + 1] : themeProto.RareSettings[index1 * 4];
        float rareSetting1 = themeProto.RareSettings[index1 * 4 + 2];
        float num4         = 1f - Mathf.Pow (1f - num2, p);
        if (urandom1_1.NextDouble () < (double)num4)
        {
          ++numArray1[rareVein];
          for (int index2 = 1; index2 < 12 && urandom1_1.NextDouble () < (double)rareSetting1; ++index2)
            ++numArray1[rareVein];
        }
      }
      planetData.veinSpotsSketch = numArray1;
    }

  }
}