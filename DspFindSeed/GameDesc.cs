using System;
using System.IO;

namespace DspFindSeed
{
public class GameDesc
{
  public DateTime creationTime;
  public Version creationVersion;
  public int galaxyAlgo;
  public int galaxySeed;
  public int starCount;
  public int playerProto;
  public float resourceMultiplier;
  public int[] savedThemeIds;
  public bool achievementEnable;
  public bool isPeaceMode;
  public bool isSandboxMode;
  public static readonly GameDesc developTest = new GameDesc();
  public const float RARE_RESOURCE_MULTIPLIER = 0.1f;
  public const float INFINITE_RESOURCE_MULTIPLIER = 100f;

  public bool isInfiniteResource => (double) this.resourceMultiplier >= 99.5;

  public bool isRareResource => (double) this.resourceMultiplier <= 0.100100003182888;

  static GameDesc()
  {
    DotNet35Random dotNet35Random = new DotNet35Random((int) DateTime.UtcNow.Ticks);
    GameDesc.developTest.SetForNewGame(UniverseGen.algoVersion, dotNet35Random.Next(1, 100000000), 64, 1, 1f);
  }

  public string clusterString
  {
    get
    {
      string str1 = (double) this.resourceMultiplier > 9.94999980926514 ? "99" : (this.resourceMultiplier * 10f).ToString("00");
      string str2 = "-A";
      if (this.isSandboxMode)
        str2 = "-S";
      else if (!this.isPeaceMode)
        str2 = "-C";
      return this.galaxySeed.ToString("00000000") + "-" + this.starCount.ToString() + str2 + str1;
    }
  }

  public long seedKey64
  {
    get
    {
      int galaxySeed = this.galaxySeed;
      int num1 = this.starCount;
      int num2 = (int) ((double) this.resourceMultiplier * 10.0 + 0.5);
      if (num1 > 999)
        num1 = 999;
      else if (num1 < 1)
        num1 = 1;
      if (num2 > 99)
        num2 = 99;
      else if (num2 < 1)
        num2 = 1;
      int num3 = 0;
      if (this.isSandboxMode)
        num3 = 999;
      else if (!this.isPeaceMode)
        num3 = 0;
      return (long) galaxySeed * 100000000L + (long) num1 * 100000L + (long) num2 * 1000L + (long) num3;
    }
  }

  public float propertyMultiplier
  {
    get
    {
      if (this.isSandboxMode)
        return 0.0f;
      if ((double) this.resourceMultiplier <= 0.150000005960464)
        return 4f;
      if ((double) this.resourceMultiplier <= 0.649999976158142)
        return 2f;
      if ((double) this.resourceMultiplier > 0.649999976158142 && (double) this.resourceMultiplier <= 0.899999976158142)
        return 1.5f;
      if ((double) this.resourceMultiplier > 0.899999976158142 && (double) this.resourceMultiplier <= 1.25)
        return 1f;
      if ((double) this.resourceMultiplier > 1.25 && (double) this.resourceMultiplier <= 1.75)
        return 0.9f;
      if ((double) this.resourceMultiplier > 1.75 && (double) this.resourceMultiplier <= 2.5)
        return 0.8f;
      if ((double) this.resourceMultiplier > 2.5 && (double) this.resourceMultiplier <= 4.0)
        return 0.7f;
      if ((double) this.resourceMultiplier > 4.0 && (double) this.resourceMultiplier <= 6.5)
        return 0.6f;
      return (double) this.resourceMultiplier > 6.5 && (double) this.resourceMultiplier <= 8.5 ? 0.5f : 0.4f;
    }
  }

  public void SetForNewGame(
    int _galaxyAlgo,
    int _galaxySeed,
    int _starCount,
    int _playerProto,
    float _resourceMultiplier)
  {
    this.creationTime = DateTime.UtcNow;
    this.creationVersion = GameConfig.gameVersion;
    this.galaxyAlgo = _galaxyAlgo;
    this.galaxySeed = _galaxySeed;
    this.starCount = _starCount;
    this.playerProto = _playerProto;
    this.resourceMultiplier = _resourceMultiplier;
    ThemeProtoSet themes = LDB.themes;
    int length = themes.Length;
    this.savedThemeIds = new int[length];
    for (int index = 0; index < length; ++index)
      this.savedThemeIds[index] = themes.dataArray[index].ID;
    this.achievementEnable = DateTime.Compare(this.creationTime, new DateTime(2021, 9, 29, 0, 0, 0)) > 0;
    this.isPeaceMode = true;
    this.isSandboxMode = false;
  }

  public void Export(BinaryWriter w)
  {
    w.Write(7);
    w.Write(this.creationTime.Ticks);
    w.Write(this.creationVersion.Major);
    w.Write(this.creationVersion.Minor);
    w.Write(this.creationVersion.Release);
    w.Write(this.creationVersion.Build);
    w.Write(this.galaxyAlgo);
    w.Write(this.galaxySeed);
    w.Write(this.starCount);
    w.Write(this.playerProto);
    w.Write(this.resourceMultiplier);
    int length = this.savedThemeIds.Length;
    w.Write(length);
    for (int index = 0; index < length; ++index)
      w.Write(this.savedThemeIds[index]);
    w.Write(this.achievementEnable);
    w.Write(this.isPeaceMode);
    w.Write(this.isSandboxMode);
  }

  public void Import(BinaryReader r)
  {
    int num = r.ReadInt32();
    this.creationTime = num < 3 ? new DateTime(2021, 1, 21, 6, 58, 30) : new DateTime(r.ReadInt64());
    this.creationTime = DateTime.SpecifyKind(this.creationTime, DateTimeKind.Utc);
    this.creationVersion = new Version(0, 0, 0);
    if (num >= 5)
    {
      if (num >= 6)
      {
        this.creationVersion.Major = r.ReadInt32();
        this.creationVersion.Minor = r.ReadInt32();
        this.creationVersion.Release = r.ReadInt32();
        this.creationVersion.Build = r.ReadInt32();
      }
      else
        this.creationVersion.Build = r.ReadInt32();
    }
    this.galaxyAlgo = r.ReadInt32();
    this.galaxySeed = r.ReadInt32();
    this.starCount = r.ReadInt32();
    this.playerProto = r.ReadInt32();
    this.resourceMultiplier = num < 2 ? 1f : r.ReadSingle();
    if (num >= 1)
    {
      int length = r.ReadInt32();
      this.savedThemeIds = new int[length];
      for (int index = 0; index < length; ++index)
        this.savedThemeIds[index] = r.ReadInt32();
    }
    else
    {
      ThemeProtoSet themes = LDB.themes;
      int length = themes.Length;
      this.savedThemeIds = new int[length];
      for (int index = 0; index < length; ++index)
        this.savedThemeIds[index] = themes.dataArray[index].ID;
    }
    this.achievementEnable = num < 4 ? DateTime.Compare(this.creationTime, new DateTime(2021, 9, 29, 0, 0, 0)) > 0 : r.ReadBoolean();
    if (num >= 7)
    {
      this.isPeaceMode = r.ReadBoolean();
      this.isSandboxMode = r.ReadBoolean();
    }
    else
    {
      this.isPeaceMode = true;
      this.isSandboxMode = false;
    }
  }
}
}